using FluentAssertions;
using NUnit.Framework;
using SlothBot.Questionaire;

namespace SlothBot.Tests
{
    [Category("Question Logic")]
    public class OptionallRepeatingQuestionaireBehaviour : BaseQuestionaireTestFixture<IQuestionaire>
    {
        public override IQuestionaire SetupFixture()
        {
            return null;
        }

        [Test]
        public void should_optionally_repeat_a_question()
        {
            sut = Questions.OptionallyRepeat(() => new[] { Questions.ForQuestionaire(new Question("whats your name?", _ => QuestionResult.Ok())) }, "repeat?");

            Ask();

            for (int i = 0; i < 10; i++)
            {
                sut.IsComplete.Should().BeFalse($"[before] steps have been added @ iteration {i}");

                Answer("anything");
                sut.IsComplete.Should().BeFalse($"[after] steps have been added @ iteration {i}");

                Answer("yes");
            }

            Answer("my real name");
            Answer("no");

            sut.IsComplete.Should().BeTrue("repeating wasnt requested");
        }
    }
}
