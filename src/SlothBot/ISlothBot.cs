using System.Collections.Generic;
using System.Threading.Tasks;
using SlothBot.MessagingPipeline;

namespace SlothBot
{
    /// <summary>
    /// How a sloth bot should look and act
    /// </summary>
    public interface ISlothBot
    {
        /// <summary>
        /// Connects the bot to slack in a persistant style
        /// </summary>
        /// <returns></returns>
        Task Connect();
        /// <summary>
        /// Disconnects the bot from slack
        /// </summary>
        /// <returns></returns>
        Task Disconnect();
        /// <summary>
        /// Reconnects the bot to slack incase of an error
        /// </summary>
        /// <returns></returns>
        Task Reconnect();
        /// <summary>
        /// Ensures slack is responding
        /// </summary>
        /// <returns></returns>
        Task Ping();
        /// <summary>
        /// Gets the username of the bot
        /// </summary>
        /// <returns></returns>
        string GetBotUserName();
        /// <summary>
        /// Sets message handlers which will be used by the bot to interact with others
        /// </summary>
        /// <param name="messageHandler"></param>
        void SetupHandlers(IMessageHandler[] messageHandler);
        /// <summary>
        /// Sets any plugins the bot will run once its connected to slack
        /// </summary>
        /// <param name="plugins"></param>
        void SetupPlugins(IPlugin[] plugins);
        /// <summary>
        /// Sends a message to slack
        /// </summary>
        /// <param name="responseMessage">The message to send, ensure channel and text is set</param>
        /// <returns></returns>
        Task SendMessage(ResponseMessage responseMessage);
        /// <summary>
        /// A shortcut method used to send a text message to a channel 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task Say(string channel, string message, params object[] args);
        /// <summary>
        /// Lists all the channels the the bot can connect too
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> ListChannels();

        /// <summary>
        /// Given a channel name, returns its id
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        string GetChannelId(string channelName);
    }
}
