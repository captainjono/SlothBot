using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SlothBot.Bridge;
using SlothBot.MessagingPipeline;
using SlothBot.Tests.Properties;

namespace SlothBot.Tests
{
    public static class TestExtensions
    {
        public static void LogToConsole(this IEnumerable<ResponseMessage> msgs)
        {
            foreach (var msg in msgs)
            {
                BaseMessageHandlerTest<TestQuestionaireHandler>.LogMessage(msg.Text, msg.UserId);
            }
        }
        
        public static IDisposable AsTempFileWith(this string name, string contents)
        {
            if (!File.Exists(name)) File.Create(name).Dispose();
            File.WriteAllText(name, contents);

            return new RxObservable.DisposableAction(() => File.Delete(name));
        }
    }

    public abstract class BaseMessageHandlerTest<T> : BaseTestFixture<T>
        where T : IMessageHandler
    {

        public BaseMessageHandlerTest()
        {
            FailTestOnErrorResponse = true;
            Username = "mike";
        }

        public string Username { get; set; }

        public ResponseMessage[] Say(string message, string by = null)
        {
            LogMessage(message, by ?? Username);
            var msg = new IncomingMessage()
            {
                FullText = message,
                TargetedText = message,
                UserId = by ?? Username,
                UserEmail = by ?? Username,
                BotIsMentioned = true
            };

            if (!sut.DoesHandle(msg))
            {
                LogMessage("(doesnt handle)", "Handler");
                return new ResponseMessage[] { };
            }
            var result = sut.Handle(msg);

            var msgs = result.ToArray();
            if (msgs.Length < 1) LogMessage("(no response)", "Handler");
            msgs.Where(m =>
            {
                if (!FailTestOnErrorResponse) return true;

                var failed = m.Text.StartsWith("Ummm...") || m.Text.StartsWith("Oops!");
                if(failed) Assert.Fail($"Always fail on error is set and the message '{message}' caused the failure '{m.Text}'");
                return true;
            }).LogToConsole();

            return msgs;
        }

        public bool FailTestOnErrorResponse { get; protected set; }

        public static void LogMessage(string msg, string by)
        {
            Debug.WriteLine($"[{by}] {msg ?? "(nothing)"}");
        }
    }
}
