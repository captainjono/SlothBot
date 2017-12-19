using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace SlothBot.MessagingPipeline
{
    public abstract class AsyncMessageHandler : IMessageHandler
    {
        public abstract IEnumerable<CommandDescription> GetSupportedCommands();

        public abstract bool DoesHandle(IncomingMessage message);

        public IEnumerable<ResponseMessage> Handle(IncomingMessage message)
        {
            return HandleAsync(message).ToEnumerable();
        }

        public abstract IObservable<ResponseMessage> HandleAsync(IncomingMessage message);
    }
}
