using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlothBot.MessagingPipeline
{
    public class HelpMessageHandler : IMessageHandler
    {
        private readonly IMessageHandler[] _handlers;

        public HelpMessageHandler(IMessageHandler[] handlers)
        {
            _handlers = handlers;
        }
        public IEnumerable<CommandDescription> GetSupportedCommands()
        {
            return new CommandDescription[] { };
        }

        public bool DoesHandle(IncomingMessage message)
        {
            return message.BotIsMentioned && message.TargetedText.Equals("help", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<ResponseMessage> Handle(IncomingMessage message)
        {
            var helpReply = new StringBuilder();

            helpReply.AppendLine("This is what i understand. When issuing commands, you can \"double quote things\" if it makes sense.");
            helpReply.AppendLine();
            helpReply.AppendLine();
            helpReply.AppendLine("*Command* : _What it does_");
            helpReply.AppendLine();
            foreach (var help in _handlers.SelectMany(h => h.GetSupportedCommands()))
            {
                helpReply.AppendLine($"*{help.Command}*: _{help.Description}_");
            }

            yield return new ResponseMessage()
            {
                Text = helpReply.ToString(),
                Channel = message.Channel,
                ResponseType = ResponseType.DirectMessage
            };
        }
    }
}
