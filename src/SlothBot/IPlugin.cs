using SlackConnector;

namespace SlothBot
{
    public interface IPlugin
    {
        void Start(ISlackConnection connection, ISlothBot bot);
        void Stop();
    }
}
