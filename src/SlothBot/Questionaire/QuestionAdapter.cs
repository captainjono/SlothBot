using SlothBot.MessagingPipeline;

namespace SlothBot.Questionaire
{
    public class QuestionAdapter : IQuestionAdapter
    {
        private IQuestion _question;

        public QuestionAdapter(IQuestion question)
        {
            _question = question;
        }

        public bool IsComplete
        {
            get { return _question == null; }
        }

        public IQuestion Question
        {
            get { return _question; }
        }

        public QuestionResult AnswerQuestionWith(IncomingMessage answer)
        {
            var result = Question.Evaluate(answer.FullText);
            if (result.WasSuccessful) _question = null;

            return result;
        }
    }
}
