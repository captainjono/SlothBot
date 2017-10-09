using System;
using System.Collections.Generic;
using SlothBot.MessagingPipeline;

namespace SlothBot.Examples.ConsoleHost
{
    public class SlothBotFactory
    {
        public static SlothBot Create(string slackApiKey, params IMessageHandler[] handlers)
        {
            return new SlothBot(new StaticBotCfg()
            {
                SlackApiKey = slackApiKey
            },
            messageHandlers: handlers);
        }
    }

    public static class ConsoleHost
    {
        public static void Main(string[] args)
        {
            var bot = SlothBotFactory.Create("xoxb-254374892295-nbnuF3WfGlMDkn7yHucoDpQD", new PingMessageHandler());
            bot.Connect()
               .ContinueWith(async _ =>
                {
                    await bot.Say("general", "Hey guys, whats up?");
                });

            Console.ReadLine();
        }
    }

    public class PingMessageHandler : IMessageHandler
    {
        public IEnumerable<CommandDescription> GetSupportedCommands()
        {
            return new[]
            {
                new CommandDescription()
                {
                    Command = "ping",
                    Description = "Replies to the user who sent the message with a \"Pong!\" response"
                }
            };
        }

        public bool DoesHandle(IncomingMessage message)
        {
            return message.BotIsMentioned &&
                   message.TargetedText.StartsWith("ping", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<ResponseMessage> Handle(IncomingMessage message)
        {
            yield return message.ReplyToChannel($"Pong! @{message.Username}");
        }
    }
}
