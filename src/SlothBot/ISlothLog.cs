namespace SlothBot
{
    /// <summary>
    /// Used by sloth bot to log any information, errors and warnings it may
    /// produce during its life-cycle
    /// </summary>
    public interface ISlothLog
    {
        /// <summary>
        /// A general info message
        /// </summary>
        /// <param name="message">A string which can be formatted (string.format)</param>
        /// <param name="args">Optional args to format the string with</param>
        void Info(string message, params object[] args);
        /// <summary>
        /// An error occoured with sloth bot or its plugins/handlers
        /// </summary>
        /// <param name="message">A string which can be formatted (string.format)</param>
        /// <param name="args">Optional args to format the string with</param>
        void Error(string message, params object[] args);
        /// <summary>
        /// A warning message about something that happened errounously but could be recovered from
        /// </summary>
        /// <param name="message">A string which can be formatted (string.format)</param>
        /// <param name="args">Optional args to format the string with</param>
        void Warn(string message, params object[] args);
    }
}
