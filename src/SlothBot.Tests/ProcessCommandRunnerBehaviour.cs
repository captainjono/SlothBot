using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using SlothBot.Bridge.Process;
using SlothBot.Bridge.Process.Arguments;

namespace SlothBot.Tests
{
    [Category("Proccess Runner")]
    public class ProcessRunnerBehaviour : BaseMessageHandlerTest<ProcessRunnerHandler>
    {
        private ProcessMeta sut1;
        private IProcessRunnerSecurity security;

        public override ProcessRunnerHandler SetupFixture()
        {
            security = Substitute.For<IProcessRunnerSecurity>();
            security.IsSecure.Returns(false);

            sut1 = GetProcessMetaForPing();

            var processRepo = Substitute.For<IProcessCreatorRepository>();
            processRepo.CanUseAlias(Username, sut1.Alias).Returns(true);
            processRepo.GetProcessForUse(sut1.Alias).Returns(sut1);

            return new ProcessRunnerHandler(new OsProcessRunner(security), processRepo);
        }

        public static ProcessMeta GetProcessMetaForPing()
        {
            return ProcessMeta.CreateAlias("ping")
                .ForProcess("ping.exe")
                .WithArgument(null, "192.168.232.1")
                .WithUserSpecifiedArgument("Number of times to ping server", "n")
                .WithArgumentFromFile("packetSize.argument", "l") //size of packet in bytes
                .WithArgument("a", "")
                .LogWith(i => LogMessage(i, "OS"), e => LogMessage(e, "OS"));
        }


        [TestFixtureSetUp]
        public void RunOnce()
        {
            new FileContentsProcessArgument("packetSize.argument", "l").Set("667");
        }

        [Test]
        public void should_ask_questions_then_run_if_allowed()
        {
            var willFail = Say("ping", "jackie");
            Assert.IsNull(willFail.FirstOrDefault(), "jackie hasnt got access to ping");

            var response = Say("ping");
            response.Length.Should().BeGreaterThan(0, "there should be a reply to the the ping request");
            response[0].Text.Should().Contain("ping server");

            response = Say(3.ToString(), "mike");
            response[0].Text.Should().Contain("enough info to execute ping", "the command should start to run");
            response[4].Text.Should().Contain("bytes=667", "the size should be correctly parsed");
            response.Last().Text.Should().Contain("Finished", "the command should run successfully");
        }

        [Test]
        public void should_correctly_join_arguments()
        {
            sut1.Arguments["n"].Set("2");
            sut1.Arguments.Values.ToCmdLineArguments().Should().Be("192.168.232.1 -n 2 -l 667 -a", "the arguments should correctly concatenate");
        }
    }
}
