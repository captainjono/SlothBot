using FluentAssertions;
using NUnit.Framework;
using SlothBot.MessagingPipeline;
using SlothBot.Questionaire;

namespace SlothBot.Tests
{
    [Category("Question Logic")]
    public class QuestionBehaviour
    {
        [Test]
        [TestCase("yes, yep, cool", true)]
        [TestCase("no, negative, nope", false)]
        [TestCase("no way, what, not, something", null)]
        [Category("Vocabulary")]
        public void should_accept_synonyms_as_answer_to_questions(string answers, bool? expectedOutcome)
        {
            bool outcome;

            foreach (var answer in answers.Split(','))
            {
                outcome = false;
                var question = Questions.TrueFalse(
                    "true false questionarie",
                    "a question",
                    trueAnswer =>
                    {
                        outcome = true;
                        return QuestionResult.Ok("true");
                    },
                    falseAnswer =>
                    {
                        outcome = false;
                        return QuestionResult.Ok("false");

                    }
                );

                var result = question.AnswerQuestionWith(new IncomingMessage() { FullText = answer });
                if (expectedOutcome != null)
                    outcome.Should().Be(expectedOutcome.Value, $"the correct logic of should be picked for {answer}");
                else
                {
                    result.Reply.Should().Contain("understand", "the response is invalid");
                    question.AnswerQuestionWith(new IncomingMessage() { FullText = "yes" });
                    outcome.Should().Be(true, "After an inalid answer, a valid answer should progress question");
                }

                question.IsComplete.Should().BeTrue("the questions have ended");
            }
        }
    }
}
