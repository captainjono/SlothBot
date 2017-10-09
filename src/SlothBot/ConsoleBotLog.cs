using System;

namespace SlothBot
{
    /// <summary>
    /// A logger that outputs to the console
    /// </summary>
    public class ConsoleBotLog : ISlothLog
    {
        public void Info(string message, params object[] args)
        {
            Console.WriteLine("[INFO] {0}".FormatWith(message.FormatWith(args)));
        }

        public void Error(string message, params object[] args)
        {
            Console.WriteLine("[ERROR] {0}".FormatWith(message.FormatWith(args)));
        }

        public void Warn(string message, params object[] args)
        {
            Console.WriteLine("[WARNING] {0}".FormatWith(message.FormatWith(args)));
        }
    }
}
