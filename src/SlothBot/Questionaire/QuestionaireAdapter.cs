using SlothBot.MessagingPipeline;

namespace SlothBot.Questionaire
{
    public class QuestionaireAdapter : IQuestionAdapter
    {
        public bool IsComplete => _questionaire.IsComplete;

        private readonly IQuestionaire _questionaire;

        public QuestionaireAdapter(IQuestionaire questions)
        {
            _questionaire = questions;
        }

        public IQuestion Question => _questionaire.Current.Question;

        public QuestionResult AnswerQuestionWith(IncomingMessage answer)
        {
            var result = _questionaire.AnswerQuestionWith(answer);

            return result.WasSuccessful ? QuestionResult.Ok(result.Reply) : QuestionResult.Error(result.Reply);
        }
    }
}
