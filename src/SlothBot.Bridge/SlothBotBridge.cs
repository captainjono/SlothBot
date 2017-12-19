using System;
using SlothBot.Bridge.Process;

namespace SlothBot.Bridge
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
        public static ISlothBot CreateBridge(ISlothBridgeConfig cfg)
        {
            return SlothBridge.Create(cfg);
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
            if(!cfg.IsSecure) Console.WriteLine("[WARNING] You are running SlothBot with an insecure flag. Make sure your host is locked down or only trusted people can chat with it");
            if (cfg.SlackApiKey.IsNullOrWhiteSpace())
            {
                Console.WriteLine("Please set a slack API key in the slothbot.json. Also, ensure you have some apps defined in enabledProcesses.json otherwise I am useless");
                return;
            }

            var bot = SlothBotFactory.CreateBridge(cfg);
            bot.Connect();

            Console.ReadLine();
        }
    }
}
