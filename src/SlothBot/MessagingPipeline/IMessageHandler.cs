using System;
using System.Collections.Generic;

namespace SlothBot.MessagingPipeline
{
    public interface IMessageHandler
    {
        /// <summary>
        /// A user friendly description of what this message handler, and how to trigger it
        /// ie. Ping  -  Replies to the user who sent the message with a "pong" response
        /// </summary>
        /// <returns></returns>
        IEnumerable<CommandDescription> GetSupportedCommands();

        /// <summary>
        /// An expression that inspects an incomming messages for something that indicates 
        /// this class handles it
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DoesHandle(IncomingMessage message);

        /// <summary>
        /// The action to perform when matching message is received
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        IEnumerable<ResponseMessage> Handle(IncomingMessage message);
    }
}
