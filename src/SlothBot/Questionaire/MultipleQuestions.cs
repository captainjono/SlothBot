using System.Linq;

namespace SlothBot.Questionaire
{
    public class MultipleQuestions : NestedQuestionaire<object>
    {
        public MultipleQuestions(string description, params IQuestion[] questions) : base(description)
        {
            SetupWith(questions.Select(q => Questions.ForQuestionaire(q)).ToArray());
        }
    }
}
