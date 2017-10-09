<p align="center">
<img src="https://github.com/captainjono/slothbot/blob/master/img/slothbot.jpg" alt="SlothBot" />
</p>

# SlothBot
A lightweight C# Slackbot with minimum depdencies designed for streamlined integrations into new and existing codebases.

## Features
 - Only dependency is SlackConnector
 - Middleware can send multiple messages for each message received
 - Plugins allow lower level interaction with Slack
 - Automatically builds up `help` text with all supported commands
 - Supports long running processes (async)
 - Typing Indicator - indicate to the end user that the bot has received the message and is processing the request

## Get me started!
The SlackConnector will manage your connection, so you only need to call Connect() once and your done

```csharp
    /// Going online with your bot
    /// 
    /// var myBot = SlothBotFactory.Create(new PingMessageHandler())
    /// await myBot.Connect() 
    ///
    /// Now in slack... 
    /// me: @myBot ping
    /// myBot: Pong! @me 
    public class SlothBotFactory
    {
        public static SlothBot Create(string slackApiKey, params IMessageHandler[] handlers)
        {
            return new SlothBot(new StaticBotCfg()
            {
                SlackApiKey = slackApiKey
            },
            messageHandlers: handlers);
        }
    }
```

## More details

### React to a chat message
Simply implement IMessageHandler and return true from DoesHandle(). This will trigger Handle() to be called

```csharp
public class PingMessageHandler : IMessageHandler
    {
        public IEnumerable<CommandDescription> GetSupportedCommands()
        {
            return new[]
            {
                new CommandDescription()
                {
                    Command = "ping",
                    Description = "Replies to the user who sent the message with a \"Pong!\" response"
                }
            };
        }

        public bool DoesHandle(IncomingMessage message)
        {
            return message.BotIsMentioned &&
                   message.TargetedText.StartsWith("ping", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<ResponseMessage> Handle(IncomingMessage message)
        {
            yield return message.ReplyToChannel($"Pong! @{message.Username}");
        }
    }
```

### Sometimes you need more control
Implement an IPlugin instead and you get access to the raw ISlackConnection and ISlothBot

```csharp
    public class LogBackupPlugin : IPlugin
    {
        private ILogSource _source;
        private Timer _logUploader;

        public LogBackupPlugin(ILogSource source)
        {
            _source = source;
        }

        public void Start(ISlackConnection connection, ISlothBot bot)
        {
            _logUploader = new Timer((_) => UploadOnceADay(connection), null, TimeSpan.FromDays(1), TimeSpan.FromDays(1));
        }

		public void Stop()
        {
            _logUploader.Dispose();
        }

        private void UploadOnceADay(ISlackConnection connection)
        {
            connection.Upload(connection.ConnectedHubs.FirstOrDefault().Value, _source.GetCurrentLog(), "{0}.log".FormatWith(DateTime.Now.ToShortTimeString());
        }
    }
```

### Use your prefered logging framework
Just implement ISlothLog

```csharp
    public interface ISlothLog
    {
	//General messages
        void Info(string message, params object[] args);
	//Critical messages
        void Error(string message, params object[] args);
	//Errors that will be automatically recovered from
        void Warn(string message, params object[] args);
    }
```

```csharp
public class AnotherFrameworkLogger : ISlothLog
    {
        private readonly ILogger _log;

        public AnotherFrameworkLogger(ILogger log)
        {
            _log = log;
        }

        public void Info(string message, params object[] args)
        {
            _log.Information(message.FormatWith(args));
        }

        public void Error(string message, params object[] args)
        {
            _log.Error(message.FormatWith(args));
        }

        public void Warn(string message, params object[] args)
        {
            _log.Warning(message.FormatWith(args));
        }
    }
```

### IoC ready
Containers are great, SlothBot comes out of the box ready for Constructor injection