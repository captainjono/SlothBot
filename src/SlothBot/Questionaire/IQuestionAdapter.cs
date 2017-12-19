using SlothBot.MessagingPipeline;

namespace SlothBot.Questionaire
{
    public interface IQuestionAdapter
    {
        bool IsComplete { get; }
        IQuestion Question { get; }
        QuestionResult AnswerQuestionWith(IncomingMessage answer);
    }
}
