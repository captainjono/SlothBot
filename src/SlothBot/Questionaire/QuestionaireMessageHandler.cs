using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using SlothBot.MessagingPipeline;

namespace SlothBot.Questionaire
{
    public abstract class QuestionnaireMessageHanlder<T> : AsyncMessageHandler
        where T : IQuestionaire
    {
        public Dictionary<string, T> QuestionsBeingAsked = new Dictionary<string, T>();

        public override bool DoesHandle(IncomingMessage message)
        {
            return IsResponseToQuestionaire(message) || TriggersAQuestionaire(message);
        }

        public bool IsResponseToQuestionaire(IncomingMessage message)
        {
            return QuestionsBeingAsked.Any(q => q.Key == message.UserId);
        }

        public abstract T QuestionaireFor(IncomingMessage message);

        public T StartAsking(IncomingMessage message)
        {
            var username = message.UserId;
            if (QuestionsBeingAsked.ContainsKey(username))
            {
                throw new Exception("Cant ask 2 questions to the same user");
            }

            var questions = QuestionaireFor(message);
            MonitorQuestionaire(username, questions);

            return questions;
        }

        protected abstract bool TriggersAQuestionaire(IncomingMessage message);

        public override IObservable<ResponseMessage> HandleAsync(IncomingMessage message)
        {
            return Observable.Create<ResponseMessage>(o =>
            {
                try
                {
                    if (!IsResponseToQuestionaire(message))
                    {
                        o.OnNext(StartAsking(message).AsReplyTo(message.UserId));
                    }
                    else
                    {
                        foreach (var msg in ContinueAsking(message))
                        {
                            o.OnNext(msg);
                        }
                    }

                    //finally, ask a question if there is anything to say
                    var existing = ExistingQuestionFor(message.UserId);
                    if (!existing.IsComplete)
                    {
                        o.OnNext(existing.Current.Question.Ask.AsReplyTo(message.UserId));
                    }
                    else
                    {
                        CompleteAsking(message.UserId);
                        foreach (var msg in OnQuestionaireCompleted(existing, message.UserId).ToEnumerable())
                            o.OnNext(msg);
                    }

                }
                catch (Exception e)
                {
                    o.OnNext(e.AsReplyTo(message.UserId));
                }

                o.OnCompleted();
                return Disposable.Empty;
            })
            .Do(msg => msg.Channel = message.Channel);
        }

        private IEnumerable<ResponseMessage> ContinueAsking(IncomingMessage message)
        {
            var questions = ExistingQuestionFor(message.UserId);
            var response = questions.AnswerQuestionWith(message).AsReplyTo(message.UserId);

            if (!questions.IsComplete)
            {
                yield return response;
            }
        }

        protected abstract IObservable<ResponseMessage> OnQuestionaireCompleted(T questions, string username);

        private void MonitorQuestionaire(string username, T questions)
        {
            QuestionsBeingAsked.Add(username, questions);
        }

        public void CancelAsking(string username)
        {
            QuestionsBeingAsked.Remove(username);
        }

        public void CompleteAsking(string username)
        {
            QuestionsBeingAsked.Remove(username);
        }

        private T ExistingQuestionFor(string username)
        {
            return QuestionsBeingAsked[username];
        }

        /// <summary>
        /// demonstrates how to create a question
        /// </summary>
        /// <param name="next"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        protected IQuestionaire AlreadyAskingQuestionCommand(Func<T> next, string username)
        {
            return Questions.TrueFalse(
                "Should I continue?",
                $"Im already asking you a question <@{username}>. Should we stop that conversation and continue with this?",
                 _ =>
                {
                    QuestionsBeingAsked.Remove(username);
                    MonitorQuestionaire(username, next());
                    return QuestionResult.Ok("Done!");
                },
                 _ =>
                {
                    return QuestionResult.Ok($"Ok then answer {QuestionsBeingAsked[username].Current.Question.Ask}");
                }
            );
        }
    }

    public static class ResponseExtensions
    {
        public static ResponseMessage AsReplyTo(this QuestionaireResult response, string username)
        {
            return new ResponseMessage()
            {
                Text = $"{(response.WasSuccessful ? "Done!" : "Oops!")} <@{username}> {response.Reply}".Trim(),
            };
        }

        public static ResponseMessage AsReplyTo(this string response, string username)
        {
            return new ResponseMessage()
            {
                Text = $"<@{username}> {response}".Trim(),
            };
        }

        public static ResponseMessage AsReplyTo(this Exception response, string username)
        {
            return new ResponseMessage()
            {
                Text = $"Ummm... <@{username}> {response.Message}".Trim(),
            };
        }

        public static ResponseMessage AsReplyTo(this IQuestionaire questionaire, string username)
        {
            if (questionaire.IsComplete)
                return new ResponseMessage()
                {
                    Text = $"<@{username}> all done!",
                    UserId = username
                }; ;
            return new ResponseMessage()
            {
                Text = $"<@{username}> {questionaire.Current.Question.Ask}",
                UserId = username
            }; ;
        }
    }
}
