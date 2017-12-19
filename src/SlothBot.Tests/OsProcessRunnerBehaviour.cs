using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using SlothBot.Bridge.Process;
using SlothBot.Bridge.Process.Arguments;
using SlothBot.Tests.Properties;

namespace SlothBot.Tests
{
    [Category("Proccess Runner")]
    [Category("Security")]
    public class OsProcessRunnerBehaviour : BaseTestFixture<OsProcessRunner>
    {
        private IProcessRunnerSecurity security;
        private ProcessMeta ping;

        public override OsProcessRunner SetupFixture()
        {
            security = Substitute.For<IProcessRunnerSecurity>();
            ping = ProcessRunnerBehaviour.GetProcessMetaForPing();
            var numberOfTimesToPing = ping.Arguments.Values.OfType<UserSpecifiedProcessArgument>().FirstOrDefault();
            numberOfTimesToPing.Set(1.ToString());

            return new OsProcessRunner(security);
        }

        /// <summary>
        /// As far as i have read, i dont need to worry about unicode traversal attacks
        /// </summary>
        /// <param name="injectionPath"></param>
        /// <returns></returns>
        [Test]
        [Category("Windows only tests")]
        [TestCase(@"c:\program files\..\windows\sytem32\ping.exe")]
        [TestCase(@"%WINDIR%\system32\ping.exe")]
        [TestCase(@"c:\windows\system32\ping.exe", ExpectedException = typeof(AssertionException))]
        [TestCase(@"c:\windows\system32\ping.exe && c:\windows\system32\ping.exe")]
        public async Task should_stop_injection_attacks(string injectionPath)
        {
            
            ping.ProcessName = injectionPath;
            security.IsSecure.Returns(false);
            //security.WorkingDirectory.Returns(AppContext.BaseDirectory);
            
            var outcome = await sut.Run(ping);
            outcome.Should().BeGreaterThan(0, "the process should fail");
        }

        [Test]
        public async Task should_restrict_workingDirectory_when_secure()
        {
            security.IsSecure.Returns(true);

            var outcome = await sut.Run(ping);
            outcome.Should().BeGreaterThan(0, "the process should fail");

            security.IsSecure.Returns(false);

            outcome = await sut.Run(ping);
            outcome.Should().Be(0, "the process should succeed now");
        }
    }
}
