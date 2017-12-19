using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using FluentAssertions;
using NUnit.Framework;
using SlothBot.MessagingPipeline;
using SlothBot.Questionaire;

namespace SlothBot.Tests
{
    public class TestQuestions : MultipleQuestions
    {
        public static string StateA { get; set; }
        public static string StateB { get; set; }

        public string AnswerQuestion(IncomingMessage message)
        {
            return "ok";
        }

        public TestQuestions() : base("test questions", new IQuestion[] 
        {
            new Question("What should i set stateA as?",
            answer =>
            {
                TestQuestions.StateA = answer;

                return QuestionResult.Ok("done");
            }),
            new Question("What should i set stateb as?",
                answer =>
                {
                    TestQuestions.StateB = answer;
                    return QuestionResult.Ok("done");
                }),
        })
        {

        }

        public TestQuestions(string description, IQuestion[] questions) : base(description, questions)
        {
        }
    }

    public class TestQuestionaireHandler : QuestionnaireMessageHanlder<TestQuestions>
    {
        public override TestQuestions QuestionaireFor(IncomingMessage message)
        {
            return new TestQuestions();
        }

        protected override bool TriggersAQuestionaire(IncomingMessage message)
        {
            return message.FullText.Contains("new");
        }

        protected override IObservable<ResponseMessage> OnQuestionaireCompleted(TestQuestions questions, string username)
        {
            return Observable.Return(new ResponseMessage()
            {
                Text = $"<@{username}> done. Thats all i needed to know!",
                UserId = username
            });
        }

        public override IEnumerable<CommandDescription> GetSupportedCommands()
        {
            return new CommandDescription[] {};
        }
    }
    
    [Category("Question Logic")]
    public class QuestionaireBehaviour : BaseMessageHandlerTest<QuestionnaireMessageHanlder<TestQuestions>>
    {
        public override QuestionnaireMessageHanlder<TestQuestions> SetupFixture()
        {
            return new TestQuestionaireHandler();
        }
        
        [Test]
        [TestCase("mike, sally")]
        [TestCase("mike")]

        public void should_walk_through_questionaire(string users)
        {
            foreach (var username in users.Split(','))
            {
                ResponseMessage[] response = null;

                sut.QuestionsBeingAsked.Count.Should().Be(0);

                response = Say("new something", username);
                response[0].Text.Should().Contain(sut.QuestionsBeingAsked[username].Current.Question.Ask);
                
                sut.QuestionsBeingAsked.Count.Should().Be(1);

                response = Say("stateA", username);
                response[0].Text.Should().Contain($"<@{username}> done");

                response = Say("stateB", username);
                response[0].Text.Should().Contain($"<@{username}> done");
            }

            sut.QuestionsBeingAsked.Count.Should().Be(0, "all questionares have been answered");
        }
    }
}
