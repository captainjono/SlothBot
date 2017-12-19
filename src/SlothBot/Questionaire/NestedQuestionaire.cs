using System;
using System.Collections.Generic;
using System.Linq;
using SlothBot.MessagingPipeline;

namespace SlothBot.Questionaire
{
    public class NestedQuestionaire<T> : IQuestionaire
    {
        private Action<T> _onComplete;
        protected readonly Stack<IQuestionAdapter> Steps = new Stack<IQuestionAdapter>();

        public IQuestionAdapter Current => Steps.Count > 0 ? Steps.Peek() : null;

        public string Description { get; protected set; }
        public bool IsComplete => Steps.Count == 0;

        public T Result { get; protected set; }

        public NestedQuestionaire(T result) : this()
        {
            Result = result;
        }

        public NestedQuestionaire(string description = null, Action<T> onComplete = null)
        {
            _onComplete = onComplete ?? (_ => { });
            Description = description;
        }

        public NestedQuestionaire<T> SetupWith(params IQuestionAdapter[] questions)
        {
            foreach (var q in questions.Reverse())
            {
                Steps.Push(q);
            }

            return this;
        }

        public NestedQuestionaire<T> OnComplete(Action<T> onComplete)
        {
            _onComplete = onComplete;

            return this;
        }

        public virtual QuestionaireResult AnswerQuestionWith(IncomingMessage message)
        {
            if (IsComplete) return QuestionaireResult.Error("The questionaire is complete");

            var answer = Current.AnswerQuestionWith(message);
            if (!answer.WasSuccessful) return QuestionaireResult.Error($"Sorry i didnt understand that @{message.UserId}");

            if(Current.IsComplete)
                MarkQuestionAsComplete();

            if (answer.ResponseQuestions != null)
                SetupWith(answer.ResponseQuestions);

            if (IsComplete)
                _onComplete(Result);

            return answer.WasSuccessful ? QuestionaireResult.Ok(answer.Response) : QuestionaireResult.Error(answer.Response);

        }

        protected void MarkQuestionAsComplete()
        {
            Steps.Pop();
        }
    }
}