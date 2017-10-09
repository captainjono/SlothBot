using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SlackConnector;
using SlackConnector.Models;
using SlothBot.MessagingPipeline;

namespace SlothBot
{
    /// <summary>
    /// An implementation of sloth bot which is tightly coupled with SlackConnector. 
    /// </summary>
    public class SlothBot : ISlothBot
    {
        private readonly ISlackConfig _slackConfig;
        private readonly ISlothLog _log;
        private Func<IncomingMessage, IEnumerable<ResponseMessage>> _middleware;
        private ISlackConnection _connection;

        private bool _isDisconnecting;
        private IPlugin[] _plugins;

        /// <summary>
        /// Creates a new sloth bot ready for connect()'ion
        /// </summary>
        /// <param name="slackConfig">The cfg that tells slack who this bot is</param>
        /// <param name="log">The logger sloth bot will use, defaults to ConsoleLog</param>
        /// <param name="messageHandlers">Any message handlers sloth bot will react to messages with. Also can be set later with SetHandlers()</param>
        /// <param name="plugins">Any message handlers sloth bot will integrate with. Also can be set later with SetPlugins()</param>
        public SlothBot(ISlackConfig slackConfig, ISlothLog log = null, IMessageHandler[] messageHandlers = null, IPlugin[] plugins = null)
        {
            _slackConfig = slackConfig;
            _log = log ?? new ConsoleBotLog();
            _plugins = plugins;
            _middleware = _ => new ResponseMessage[] { };
            _slackConfig = slackConfig;
            SetupHandlers(messageHandlers ?? new IMessageHandler[] { });
        }

        /// <summary>
        /// Sets the plugins that this bot will integrate with
        /// </summary>
        /// <param name="plugins"></param>
        public void SetupPlugins(IPlugin[] plugins)
        {
            _plugins = plugins;
        }

        /// <summary>
        /// Sets the message handlers that this bot will react to messages with
        /// </summary>
        /// <param name="plugins"></param>
        public void SetupHandlers(IMessageHandler[] messageHandler)
        {
            _middleware = msg =>
            {
                return messageHandler
                        .Where(m => m.DoesHandle(msg))
                        .SelectMany(m => m.Handle(msg));
            };
        }

        /// <summary>
        /// Connects to slack in a persistant manner. Reconnections will *mostly* be handled for you.
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            _log.Info("Connecting...");

            var connector = new SlackConnector.SlackConnector();

            _connection = await connector.Connect(_slackConfig.SlackApiKey);
            _connection.OnMessageReceived += MessageReceived;
            _connection.OnDisconnect += OnDisconnect;

            _log.Info("Connected!");
            _log.Info("Bots Name: {0}", _connection.Self.Name);
            _log.Info("Team Name: {0}", _connection.Team.Name);
            StartPlugins();
        }

        private void StartPlugins()
        {
            foreach (var plugin in _plugins)
            {
                try
                {
                    plugin.Start(_connection, this);
                }
                catch (Exception e)
                {
                    _log.Error("Could not start plugin '{0}' because '{1}'".FormatWith( plugin.GetType().Name, e));
                }
            }
        }

        private void StopPlugins()
        {
            foreach (var plugin in _plugins)
            {
                try
                {
                    plugin.Stop();
                }
                catch (Exception e)
                {
                    _log.Error("Could not stop plugin '{0}' because '{1}'".FormatWith(plugin.GetType().Name, e));
                }
            }
        }
        
        public async Task Disconnect()
        {
            _isDisconnecting = true;

            if (_connection != null && _connection.IsConnected)
            {
                await _connection.Close();
            }
        }

        private void OnDisconnect()
        {
            if (_isDisconnecting)
            {
                _log.Info("Disconnected.");
            }
            else
            {
                _log.Info("Disconnected from server, attempting to reconnect...");
                Reconnect();
            }
        }

        /// <summary>
        /// Reconnects to slack in case of a network failure or other critical event
        /// </summary>
        /// <returns></returns>
        public Task Reconnect()
        {
            _log.Info("Reconnecting...");
            if (_connection != null)
            {
                _connection.OnMessageReceived -= MessageReceived;
                _connection.OnDisconnect -= OnDisconnect;
                _connection = null;
            }

            _isDisconnecting = false;
            return Connect().ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
                {
                    _log.Info("Connection restored.");
                }
                else
                {
                    _log.Info("Error while reconnecting: {0}", task.Exception);
                }
            });
        }

        /// <summary>
        /// Called when a message is received in any of the channels the bot is connected to
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task MessageReceived(SlackMessage message)
        {
            _log.Info("Message found from '{0}'", message.User.Name);

            var incomingMessage = new IncomingMessage
            {
                RawText = message.Text,
                FullText = message.Text,
                UserId = message.User.Id,
                Username = GetUsername(message),
                UserEmail = message.User.Email,
                Channel = message.ChatHub.Id,
                ChannelType = message.ChatHub.Type == SlackChatHubType.DM
                    ? ResponseType.DirectMessage
                    : ResponseType.Channel,
                UserChannel = await GetUserChannel(message),
                BotName = _connection.Self.Name,
                BotId = _connection.Self.Id,
                BotIsMentioned = message.MentionsBot
            };

            incomingMessage.TargetedText = incomingMessage.GetTargetedText();

            try
            {
                foreach (ResponseMessage responseMessage in _middleware(incomingMessage))
                {
                    await SendMessage(responseMessage);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await SendMessage(new ResponseMessage()
                    {
                        Text = "ERROR WHILE PROCESSING MESSAGE: {0}".FormatWith(ex)
                    });
                }
                catch (Exception)
                {
                    Debug.WriteLine("ERROR WHILE PROCESSING MESSAGE: {0}".FormatWith(ex));
                }
            }
        }

        public async Task Ping()
        {
            await _connection.Ping();
        }

        public async Task SendMessage(ResponseMessage responseMessage)
        {
            var chatHub = await GetChatHub(responseMessage);

            if (chatHub != null && _connection != null)
            {
                if (responseMessage is TypingIndicatorMessage)
                {
                    _log.Info("Indicating typing on channel '{0}'", chatHub.Name);
                    await _connection.IndicateTyping(chatHub);
                }
                else
                {
                    var botMessage = new BotMessage
                    {
                        ChatHub = chatHub,
                        Text = responseMessage.Text,
                        Attachments = GetAttachments(responseMessage.Attachments)
                    };

                    await _connection.Say(botMessage);
                }
            }
            else
            {
                _log.Error("Unable to find channel for message '{0}'. Message not sent", responseMessage.Text);
            }
        }

        public async Task Say(string channel, string message, params object[] args)
        {
            try
            {
                await SendMessage(new ResponseMessage()
                {
                    Channel = channel,
                    Text = message.FormatWith(args)
                });
            }
            catch (Exception e)
            {
                await SendMessage(new ResponseMessage()
                {
                    Channel = channel,
                    Text = "[ERROR] Something crazy happened: {0}".FormatWith(e.Message)
                });
            }
        }

        private IList<SlackAttachment> GetAttachments(List<Attachment> attachments)
        {
            var slackAttachments = new List<SlackAttachment>();

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    slackAttachments.Add(new SlackAttachment
                    {
                        Text = attachment.Text,
                        Title = attachment.Title,
                        Fallback = attachment.Fallback,
                        ImageUrl = attachment.ImageUrl,
                        ThumbUrl = attachment.ThumbUrl,
                        AuthorName = attachment.AuthorName,
                        ColorHex = attachment.Color,
                        Fields = GetAttachmentFields(attachment)
                    });
                }
            }

            return slackAttachments;
        }

        private IList<SlackAttachmentField> GetAttachmentFields(Attachment attachment)
        {
            var attachmentFields = new List<SlackAttachmentField>();

            if (attachment != null && attachment.AttachmentFields != null)
            {
                foreach (var attachmentField in attachment.AttachmentFields)
                {
                    attachmentFields.Add(new SlackAttachmentField
                    {
                        Title = attachmentField.Title,
                        Value = attachmentField.Value,
                        IsShort = attachmentField.IsShort
                    });
                }
            }

            return attachmentFields;
        }

        public string GetUserIdForUsername(string username)
        {
            var user = _connection.UserCache.FirstOrDefault(x => x.Value.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrEmpty(user.Key) ? string.Empty : user.Key;
        }

        public string GetChannelId(string channelName)
        {
            var channel = _connection.ConnectedChannels().FirstOrDefault(x => x.Name.Equals(channelName, StringComparison.OrdinalIgnoreCase));
            return channel != null ? channel.Id : string.Empty;
        }

        public Dictionary<string, string> ListChannels()
        {
            return _connection.ConnectedHubs.Values.ToDictionary(channel => channel.Id, channel => channel.Name);
        }

        public string GetBotUserName()
        {
            return _connection == null ? "Not connected" : _connection.Self.Name;
        }

        private string GetUsername(SlackMessage message)
        {
            return _connection.UserCache.ContainsKey(message.User.Id)
                ? _connection.UserCache[message.User.Id].Name
                : string.Empty;
        }

        private async Task<string> GetUserChannel(SlackMessage message)
        {
            return (await GetUserChatHub(message.User.Id, joinChannel: false) ?? new SlackChatHub()).Id;
        }

        private async Task<SlackChatHub> GetChatHub(ResponseMessage responseMessage)
        {
            SlackChatHub chatHub = null;

            if (responseMessage.ResponseType == ResponseType.Channel)
            {
                chatHub = new SlackChatHub { Id = responseMessage.Channel };
            }
            else if (responseMessage.ResponseType == ResponseType.DirectMessage)
            {
                if (string.IsNullOrEmpty(responseMessage.Channel))
                {
                    chatHub = await GetUserChatHub(responseMessage.UserId);
                }
                else
                {
                    chatHub = new SlackChatHub { Id = responseMessage.Channel };
                }
            }

            return chatHub;
        }

        private async Task<SlackChatHub> GetUserChatHub(string userId, bool joinChannel = true)
        {
            SlackChatHub chatHub = null;

            if (_connection.UserCache.ContainsKey(userId))
            {
                var username = "@{0}".FormatWith(_connection.UserCache[userId].Name);
                chatHub = _connection.ConnectedDMs()
                                     .FirstOrDefault(x => x.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
            }

            if (chatHub == null && joinChannel)
            {
                chatHub = await _connection.JoinDirectMessageChannel(userId);
            }

            return chatHub;
        }
    }

    public static class Extensions
    {
        public static string FormatWith(this string source, params object[] args)
        {
            return string.Format(source, args);
        }
    }
}
