using System.IO;
using Newtonsoft.Json;

namespace SlothBot.Bridge.Process
{
    public interface ISlothBridgeConfig : IProcessRunnerSecurity, ISlackConfig
    {
        string AliasedProcessesFullName { get; set; }
        string PossibleProcessesFullName { get; set; }
    }

    public class SlothBridgeConfiguration : ISlothBridgeConfig
    {
        public string WorkingDirectory { get; set; }
        public bool IsSecure { get; set; }
        public string SlackApiKey { get; set; }
        public string AliasedProcessesFullName { get; set; }
        public string PossibleProcessesFullName { get; set; }

        public static ISlothBridgeConfig FromFile(string jsonFilename)
        {
            if (!File.Exists(jsonFilename)) File.Create(jsonFilename).Dispose();

            return JsonConvert.DeserializeObject<SlothBridgeConfiguration>(File.ReadAllText(jsonFilename)) ?? new SlothBridgeConfiguration();
        }

    }
}
