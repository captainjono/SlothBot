namespace SlothBot
{
    /// <summary>
    /// The configuration sloth bot uses to connect to slack with
    /// </summary>
    public interface ISlackConfig
    {
        /// <summary>
        /// The API key that slack will give to your bot. see: https://api.slack.com/bot-users
        /// </summary>
        string SlackApiKey { get; }
    }
}
