using System;
using Newtonsoft.Json;
using SlothBot.Bridge.Process;

namespace SlothBot.Bridge.Host
{
    /// <summary>
    /// Creates slothBots ready for Connect()
    /// </summary>
    public class SlothBotFactory
    {
        /// <summary>
        /// Creates the SlothBot Bridge used to alias processes
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static ISlothBot CreateBridge(ISlothBridgeConfig cfg, ISlothLog logging = null)
        {
            return SlothBridge.Create(cfg, logging);
        }
    }

    /// <summary>
    /// A basic console host for sloth bot that terminates when a key is pressed
    /// </summary>
    public static class SlothBotBridge
    {
        public static void Main(string[] args)
        {
            var cfg = SlothBridgeConfiguration.FromFile("slothbot.json");
            if (!cfg.IsSecure) Console.WriteLine("[WARNING] You are running SlothBot with an insecure flag. Make sure your host is locked down or only trusted people can chat with it\r\n\r\n");
            if (cfg.SlackApiKey.IsNullOrWhiteSpace())
            {
                PrintApiErrorAndHelp();
                return;
            }

            var bot = SlothBotFactory.CreateBridge(cfg);
            bot.Connect();

            Console.ReadLine();
        }


        private static void PrintApiErrorAndHelp()
        {
            Console.Error.WriteLine("Please set a slack API key in the slothbot.json.");
            var cfg = new SlothBridgeConfiguration { SlackApiKey = "from-slack" };
            Console.Error.WriteLine($"ie.\r\n {PrettyPrint(cfg)}\r\n");

            Console.Error.WriteLine("Also, ensure you have some apps defined in enabledProcesses.json otherwise I am useless");
            var processExample = new ProcessSecurity()
            {
                Path = @"c:\windows\system32\ping.exe",
                ProcessName = "ping",
                UsernamesWhoCanAlias = new[] { "user-email-address" }
            };
            Console.Error.WriteLine($"ie. {PrettyPrint(processExample)}");

            Console.ReadLine();
        }

        private static string PrettyPrint<T>(T toPrint)
        {
            return JsonConvert.SerializeObject(toPrint,
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
    }
}
