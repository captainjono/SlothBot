using System.Threading.Tasks;
using SlothBot.MessagingPipeline;

namespace SlothBot
{
    public interface ISlothBot
    {
        Task Disconnect();
        Task Reconnect();
        Task Ping();
        string GetBotUserName();
        void SetupHandlers(IMessageHandler[] messageHandler);
        void SetupPlugins(IPlugin[] plugins);
        Task SendMessage(ResponseMessage responseMessage);
        Task Say(string channel, string message, params object[] args);
    }
}
