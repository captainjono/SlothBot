using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SlothBot.Bridge.Vocabulary;

namespace SlothBot.Tests
{
    [TestFixture]
    [Category("Vocabulary")]
    public class SlothCommandParserBehaviour
    {
        /// <summary>
        /// I expect the parser to be able to do the actual parsing properly, 
        /// so have only verified the output shape
        /// </summary>
        /// <param name="input"></param>
        /// <param name="command"></param>
        /// <param name="paramCount"></param>
        [Test]
        [TestCase("alias ping.exe \"c:\\program files\\something\"", "alias", 2)]
        [TestCase("alias ping.exe c:\\program files\\something", "alias", 3)]
        [TestCase("something", "something", 0)]
        [TestCase("something", "\"something something\"", 2, ExpectedException = typeof(AssertionException))]

        public void should_parse_commands_correctly(string input, string command, int paramCount)
        {
            var cmd = input.ToSlothCommands().Commands.FirstOrDefault();
            cmd.Command.Should().Be(command, "the command should parse correclty");
            cmd.Params.Length.Should().Be(paramCount, "the params should respect the syntax rules");
        }

        [Test]
        public void should_parse_multiline_commands()
        {
            var cmd = new StringBuilder();
            cmd.AppendLine("hello world \"this\" \"is a command\"");
            cmd.AppendLine("another cmd");
            cmd.AppendLine("\"deploy something\""); 
            cmd.AppendLine();//handle extra whitespace

            var result = cmd.ToString().ToSlothCommands();

            result.Commands.Count().Should().Be(3, "3 commands should be parsed");
            var firstCmd = result.Commands.FirstOrDefault();

            firstCmd.Command.Should().Be("hello");
            firstCmd.Params.Length.Should().Be(3, "there are 3 params");
            firstCmd.Params[0].Should().Be("world");
            firstCmd.Params[1].Should().Be("this");
            firstCmd.Params[2].Should().Be("is a command");

            var secndCmd = result.Commands.Skip(1).FirstOrDefault();
            secndCmd.Command.Should().Be("another");
            secndCmd.Params.Length.Should().Be(1, "there is only 1 param");
            secndCmd.Params[0].Should().Be("cmd");

            var thrdCmd = result.Commands.Skip(2).FirstOrDefault();
            thrdCmd.Command.Should().Be("deploy something");
            thrdCmd.Params.Length.Should().Be(0, "no params are specified");
        }

        [Test]
        public void should_parse_into_components()
        {
            var complexCommand = "CREATE c:\\windows\\cool.exe as \"something i need\" OK".ToSlothCommands();

            var parsedCommand = complexCommand.Commands.FirstOrDefault();
            var parsedIntoTokens = parsedCommand.Ensure().FormatedWith("create".AsKeyword(), "path".AsValue(), "as".AsKeyword(), "description".AsValue(), "OK".AsKeyword());

            parsedIntoTokens["create"].Should().Be("CREATE", "because the cmd keyword should be parsed correctly");
            parsedIntoTokens["path"].Should().Be("c:\\windows\\cool.exe", "because the cmd value should be parsed correctly");
            parsedIntoTokens["description"].Should().Be("something i need", "because the cmd value should be parsed correctly");
            parsedIntoTokens["OK"].Should().Be("OK", "because the cmd keyword should be parsed correctly");
            parsedIntoTokens.ContainsKey("ok").Should().BeFalse("case should be respected");
        }
    }
}
