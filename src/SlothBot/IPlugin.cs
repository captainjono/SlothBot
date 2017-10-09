using SlackConnector;

namespace SlothBot
{
    /// <summary>
    /// A general purpose hook to get access to lower level sloth bot operations
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Called when sloth bot first connects to slack and will ready the plugin
        /// and start any work it may do
        /// </summary>
        /// <param name="connection">The raw slack connection</param>
        /// <param name="bot">The connected sloth bot</param>
        void Start(ISlackConnection connection, ISlothBot bot);
        /// <summary>
        /// Stops the plugins and cleans up any resources
        /// </summary>
        void Stop();
    }
}
