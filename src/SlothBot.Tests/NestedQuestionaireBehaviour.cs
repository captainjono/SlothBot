using FluentAssertions;
using NUnit.Framework;
using SlothBot.MessagingPipeline;
using SlothBot.Questionaire;

namespace SlothBot.Tests
{
    [Category("Question Logic")]
    public class NestedQuestionaireBehaviour : BaseQuestionaireTestFixture<NestedQuestionaire<string>>
    {
        public override NestedQuestionaire<string> SetupFixture()
        {
            return new NestedQuestionaire<string>().SetupWith(
                new MultipleQuestions("12", new[] { Questions.Ask("1", _ => QuestionResult.Ok()), Questions.Ask("2", _ => QuestionResult.Ok()) }).ForQuestionaire(),
                Questions.Ask("3", _ => QuestionResult.Ok()).ForQuestionaire(),
                new MultipleQuestions("45", new[] { Questions.Ask("4", _ => QuestionResult.Ok()), Questions.Ask("5", _ => QuestionResult.Ok()) }).ForQuestionaire()
                );
        }

        [Test]
        public void should_be_complete_if_nothing_added()
        {
            sut = new NestedQuestionaire<string>("empty");
            sut.IsComplete.Should().BeTrue("no steps have been added");

            sut.SetupWith(Questions.ForQuestionaire(new Question("hello", answer => QuestionResult.Ok(""))));

            sut.IsComplete.Should().BeFalse("a question has been added");
            sut.AnswerQuestionWith(new IncomingMessage() { FullText = "anything" });

            sut.IsComplete.Should().BeTrue("all questions have been answered");
        }

        [Test]
        public void should_step_through_nested_questions()
        {
            for (var i = 1; i <= 5; i++)
            {
                sut.Current.Question.Ask.Should().Be(i.ToString(), "the questionaire should iterate through nested questions");
                Answer("");
                sut.IsComplete.Should().Be(i == 5, $"the questionaire should {(i == 5 ? "" : "not")} be complete yet @ iteration {i}");
            }

            sut.IsComplete.Should().BeTrue("the questionaire should be complete");
        }
    }
}
