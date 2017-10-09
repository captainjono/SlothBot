namespace SlothBot
{
    /// <summary>
    /// A basic slack configruation object that lives in memory
    /// </summary>
    public class StaticBotCfg : ISlackConfig
    {
        /// <summary>
        /// The API 
        /// </summary>
        public string SlackApiKey { get; set; }
    }
}
