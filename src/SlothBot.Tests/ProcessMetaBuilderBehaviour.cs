using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using SlothBot.Bridge.Process;

namespace SlothBot.Tests
{
    public class ProcessMetaBuilderBehaviour : BaseMessageHandlerTest<ProcessMetaRunnerCreatorHandler>
    {
        private IProcessCreatorRepository processRepo;

        public override ProcessMetaRunnerCreatorHandler SetupFixture()
        {
            processRepo = Substitute.For<IProcessCreatorRepository>();

            return new ProcessMetaRunnerCreatorHandler(processRepo);
        }

        [Test]
        [TestCase("jack", true)]
        [TestCase("mike", false, ExpectedException = typeof(AssertionException))]
        public void should_allow_certain_users_to_alias_process(string username, bool result)
        {
            processRepo.CanAliasProcess(username, "ping.exe").Returns(result);

            Say("alias ping.exe as ping", username).FirstOrDefault().Text.Should().Contain(result ? "argument" : "sorry", $"the person {(result ? "has" : "hasnt")} got permissions");
        }

        [Test]
        public void should_alias_process_and_support_all_argument_types()
        {
            processRepo.CanAliasProcess("mike", "c:\\windows\\system32\\ping.exe").Returns(true);
            //basic alias
            Say("alias c:\\windows\\system32\\ping.exe as ping");
            Say("No");
            Say("mike@cool.com");
            Say("");
            Say("Tests response with prod");

            //complex alias using all arguments
            Say("alias c:\\windows\\system32\\ping.exe as ping");
            Say("yes");
            Say("myArg1");
            Say("define");
            Say("static arg");
            Say("yes");
            Say("myArg2");
            Say("user");
            Say("Question about arg");
            Say("yes");
            Say("myArg4");
            Say("file");
            Say("test.txt");
            Say("yes");
            Say("myArg5");
            Say("define");
            Say("another static arg");
            Say("no");
            Say("mike@cool.com");
            Say("00:15:00");
            Say("Tests response with prod");
        }
    }
}
