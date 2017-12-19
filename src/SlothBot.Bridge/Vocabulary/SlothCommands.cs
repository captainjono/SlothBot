using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sprache;

namespace SlothBot.Bridge.Vocabulary
{
    public class SlothCommand
    {
        public string Command { get; set; }
        public string[] Params { get; set; }
    }

    public class SlothParserResult
    {
        public string Input { get; set; }
        public IEnumerable<SlothCommand> Commands { get; set; }
    }

    public class SlothCommandException : Exception
    {
        public SlothCommandException(string resultMessage) : base(resultMessage)
        {
        }
    }

    public static class SlothBotCommandParserExtensions
    {
        public static SlothParserResult ToSlothCommands(this string message)
        {
            var result = ParseUserInputToSlothCommand(new Input(message));

            if (result.WasSuccessful)
            {
                return new SlothParserResult()
                {
                    Commands = result.Value.Where(r => !string.IsNullOrWhiteSpace(r.FirstOrDefault()))
                                           .Select(r => new SlothCommand()
                                            {
                                                Command = r.FirstOrDefault(),
                                                Params = r.Skip(1).ToArray()
                                            }),
                    Input = message
                };
            }
            throw new SlothCommandException(result.Message);
        }

        public static EnsureSlothCommand Ensure(this SlothCommand cmd)
        {
            return new EnsureSlothCommand(cmd);
        }

        public static EnsureSlothCommand Ensure(this string text)
        {
            return new EnsureSlothCommand(text.ToSlothCommands().Commands.FirstOrDefault());
        }

        #region Dont reinvent the sheel -> Shamelessly refactored from https://github.com/sprache/Sprache/blob/master/test/Sprache.Tests/Scenarios/CsvTests.cs


        private static readonly Parser<char> TokenSeparator = Parse.Char(' ');

        private static readonly Parser<char> QuotedTokenDelimiter = Parse.Char('"');

        private static readonly Parser<char> QuoteEscape = Parse.Char('"');

        private static Parser<T> Escaped<T>(Parser<T> following)
        {
            return from escape in QuoteEscape
                   from f in following
                   select f;
        }

        private static readonly Parser<char> QuotedTokenContent =
            Parse.AnyChar.Except(QuotedTokenDelimiter).Or(Escaped(QuotedTokenDelimiter));

        private static readonly Parser<char> LiteralTokenContent =
            Parse.AnyChar.Except(TokenSeparator).Except(Parse.String(Environment.NewLine));

        private static readonly Parser<string> QuotedToken =
            from open in QuotedTokenDelimiter
            from content in QuotedTokenContent.Many().Text()
            from end in QuotedTokenDelimiter
            select content;

        private static readonly Parser<string> NewLine =
            Parse.String(Environment.NewLine).Text();

        private static readonly Parser<string> CommandTerminator =
            Parse.Return("").End().XOr(
                NewLine.End()).Or(
                NewLine);

        private static readonly Parser<string> Token =
            QuotedToken.XOr(
                LiteralTokenContent.XMany().Text());

        private static readonly Parser<IEnumerable<string>> Command =
            from leading in Token
            from rest in TokenSeparator.Then(_ => Token).Many()
            from terminator in CommandTerminator
            select Cons(leading, rest);

        private static IEnumerable<T> Cons<T>(T head, IEnumerable<T> rest)
        {
            yield return head;
            foreach (var item in rest)
                yield return item;
        }

        public static readonly Parser<IEnumerable<IEnumerable<string>>> ParseUserInputToSlothCommand =
            Command.XMany().End();
        
        #endregion
    }

    public class EnsureSlothCommand
    {
        private readonly SlothCommand _cmd;

        public EnsureSlothCommand(SlothCommand cmd)
        {
            _cmd = cmd;
        }

        public SlothCommand ParamCountIs(int expectedCount, string errorMessage)
        {
            if (_cmd.Params.Length != expectedCount)
                throw new SlothCommandException(errorMessage);

            return _cmd;
        }
        
        public IDictionary<string, string> FormatedWith(params IFormatParser[] fmts)
        {
            var @params = _cmd.Params ?? new string[] { };
            if (fmts.Length > 1 && !@params.Any())
                throw new ParserFormatException();

            var  input = new[] {_cmd.Command }.Concat(@params).ToArray();

            if (fmts.Length != input.Count())//with the cmd at the start
                throw new ParserFormatException();

            return input.Zip(fmts, (cmd,fmt) => new KeyValuePair<string, string>(fmt.Key, fmt.Parse(cmd)))
                        .ToDictionary(f => f.Key, v => v.Value);
        }

    }

    public interface IFormatParser
    {
        string Key { get; }
        string Parse(string token);
    }

    public class ExactfFormat : IFormatParser
    {
        public string Key { get; private set; }
        public ExactfFormat(string key)
        {
            Key = key;
        }
        
        public string Parse(string token)
        {
            if(token.Length == Key.Length && token.Equals(Key, StringComparison.CurrentCultureIgnoreCase))
                return token;

            throw new ParserFormatException();
        }
    }

    public class ReturnValue : IFormatParser
    {
        public string Key { get; private set; }

        public ReturnValue(string key)
        {
            Key = key;
        }
        public string Parse(string token)
        {
            return token;
        }
    }


    public class ParserFormatException : Exception
    {
    }

    public static class CmdFmt
    {
        public static IFormatParser AsKeyword(this string keyword)
        {
            return new ExactfFormat(keyword);
        }

        public static IFormatParser AsValue(this string key)
        {
            return new ReturnValue(key);
        }

        public static IFormatParser AsValueOfTheRest(this string key)
        {
            return new ReturnValue(key);
        }
    }
}
