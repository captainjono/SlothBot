using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlothBot
{
    public interface ISlothVocabulary
    {
        bool? AsYesNo(string toParse);
        int? AsNumber(string toParse);
        TimeSpan? AsTimeSpan(string toParse);
    }

    public interface IKnowSynonymnsForAWord
    {
        bool IsSynonymnFor(string knownWord, string providedWord);
    }

    public class StaticSynonymnsForAWord : IKnowSynonymnsForAWord
    {
        public bool IsSynonymnFor(string knownWord, string providedWord)
        {
            providedWord = providedWord.Trim().ToLower();

            switch (knownWord)
            {
                case "yes":
                    return new [] { "yes", "yep", "ok", "cool", "sweet", "true"}.Contains(providedWord);
                case "no":
                    return new[] { "no", "nup", "nope", "negative", "false" }.Contains(providedWord);
                default:
                    throw new UnknownWordException($"I down know the meaning of '{knownWord}'");
            }
        }

        public class UnknownWordException : Exception
        {
            public UnknownWordException(string message) : base(message)
            {
            }
        }
    }


    public class SynonmynVocabulary : ISlothVocabulary
    {
        private readonly IKnowSynonymnsForAWord _dictionary;

        public SynonmynVocabulary(IKnowSynonymnsForAWord dictionary)
        {
            _dictionary = dictionary;
        }

        private T If<T>(bool first, T firstIfTrue, bool second, T secondIfTrue)
        {
            if (first) return firstIfTrue;
            if (second) return secondIfTrue;

            return default(T);
        }

        public bool? AsYesNo(string toParse)
        {
            return If<bool?>(
                _dictionary.IsSynonymnFor("yes", toParse), true,
                _dictionary.IsSynonymnFor("no", toParse), false
            );

        }

        public int? AsNumber(string toParse)
        {
            if (int.TryParse(toParse, out var i))
                return i;

            return null;
        }

        public TimeSpan? AsTimeSpan(string toParse)
        {
            if (TimeSpan.TryParse(toParse, out var i))
                return i;

            return null;
        }
    }

    public static class StringVocabularyExtensions
    {
        public static bool Contains(this string context, params string[] toSearchFor)
        {
            return toSearchFor.Any(e => e.IndexOf(e, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }


    public class SlothVocabulary
    {
        public static ISlothVocabulary Parser { get; private set; }

        static SlothVocabulary()
        {
            Parser = new SynonmynVocabulary(new StaticSynonymnsForAWord());
        }
    }
}
