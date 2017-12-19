using System.Diagnostics;
using NUnit.Framework;
using SlothBot.MessagingPipeline;
using SlothBot.Questionaire;
using SlothBot.Tests.Properties;

namespace SlothBot.Tests
{
    [TestFixture]
    [Category("Questionaires")]
    public abstract class BaseQuestionaireTestFixture<T> : BaseTestFixture<T>
        where T : IQuestionaire
    {
        public void Ask()
        {
            if (sut != null && !sut.IsComplete)
                Debug.WriteLine($"[Question] {sut.Current.Question.Ask}");
        }

        public void Answer(string answer, string username = null)
        {
            Debug.WriteLine($"[{username ?? "me"}] {answer}");
            var reply = sut.AnswerQuestionWith(new IncomingMessage() { FullText = answer, Username = username });
            Debug.WriteLine($"[Reply] {reply.Reply}");
            Ask();
        }

        [SetUp]
        public void AskFirstQuestion()
        {
            Ask();
        }
    }
}
