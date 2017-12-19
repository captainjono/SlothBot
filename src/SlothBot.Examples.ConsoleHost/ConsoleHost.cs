using System;
using System.Collections.Generic;
using SlothBot.MessagingPipeline;

namespace SlothBot.Examples.ConsoleHost
{
    /// <summary>
    /// This is the recommmended way to use sloth bot
    /// </summary>
    public class SlothBotFactory
    {
        public static SlothBot Create(string slackApiKey, params IMessageHandler[] handlers)
        {
            //instead of new'ing up, you may choose to resolve from your container if using IoC
            return new SlothBot(new StaticBotCfg()
            {
                SlackApiKey = slackApiKey
            },
            messageHandlers: handlers);
        }
    }

    /// <summary>
    /// A basic console host for sloth bot that terminates when a key is pressed
    /// </summary>
    public static class ConsoleHost
    {
        public static void Main(string[] args)
        {
            var bot = SlothBotFactory.Create("your-slack-api-key", new PingMessageHandler());
            bot.Connect()
               .ContinueWith(async _ =>
               {
                   await bot.Say("general", "Hey guys, whats up?");
               });

            Console.ReadLine();
        }
    }

    /// <summary>
    /// An example of a basic message handler that replies to a users "ping" request, with "pong"
    /// </summary>
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
            yield return message.ReplyToChannel($"Pong! <@{message.UserId}>");
        }
    }
}
