using System;
using SlothBot.MessagingPipeline;

namespace SlothBot.Questionaire
{
    public class Question : IQuestion
    {
        public string Key { get; }
        public string Ask { get; }
        public Func<string, QuestionResult> Evaluate { get; }

        public Question(string toAsk, Func<string, QuestionResult> evaluateAnswer)
        {
            Ask = toAsk;
            Evaluate = evaluateAnswer;
        }
    }

    public static class Questions
    {
        private static Func<string, Action<string>, QuestionResult> _answerActionFunc = (answer, answerFunc) =>
        {
            try
            {
                answerFunc(answer);
                return QuestionResult.Ok();
            }
            catch (Exception e)
            {
                return QuestionResult.Error(e.Message);
            }
        };

        public static IQuestion Ask(string question, Func<string, QuestionResult> answerFunc)
        {
            return new Question(question, answerFunc);
        }

        public static IQuestion AskThenDo(string question, Action<string> answerFunc)
        {
            return new Question(question, answer => _answerActionFunc(answer, answerFunc));
        }

        public static IQuestionaire TrueFalse(string description, string question, Func<string, QuestionResult> trueLogic, Func<string, QuestionResult> falseLogic)
        {
            return new MultipleQuestions(description, new Question(question, answer =>
            {
                var isTrue = SlothVocabulary.Parser.AsYesNo(answer);

                if (isTrue.HasValue)
                {
                    if (isTrue.Value) return trueLogic(answer);
                    return falseLogic(answer);
                }

                return QuestionResult.Error("Sorry i ddint understand that");
            }));
        }

        public static IQuestionaire TrueFalse(string description, string question, Func<IQuestionAdapter> trueQuestionaire, Func<IQuestionAdapter> falseQuestionaire)
        {
            return new MultipleQuestions(description, new Question(question, answer =>
            {
                var isTrue = SlothVocabulary.Parser.AsYesNo(answer);

                if (isTrue.HasValue)
                {
                    if (isTrue.Value) return QuestionResult.Ok(string.Empty, trueQuestionaire());
                    return QuestionResult.Ok(string.Empty, falseQuestionaire());
                }

                return null;
            }));
        }

        public static IQuestionaire OptionallyRepeat(Func<IQuestionAdapter[]> questionaire, string askToRepeat)
        {
            return new RepeatingQuestionaire(askToRepeat, questionaire);
        }

        public static IQuestionAdapter ForQuestionaire(this IQuestion question)
        {
            return new QuestionAdapter(question);
        }

        public static IQuestionAdapter ForQuestionaire(this IQuestionaire questions)
        {
            return new QuestionaireAdapter(questions);
        }

        public static IQuestionaire OnlyAskIf(this IQuestionaire questions, string ifTrueThenAskQuestion)
        {
            return Questions.TrueFalse(ifTrueThenAskQuestion, ifTrueThenAskQuestion, answer => QuestionResult.Ok(questions), f => QuestionResult.Ok());
        }
    }

    public class RepeatingQuestionaire : NestedQuestionaire<bool>
    {
        private readonly string _repeatQuestion;
        private bool _askToRepeat = true;

        private readonly Func<IQuestionAdapter[]> _toRepeat;

        public RepeatingQuestionaire(string repeatQuestion, Func<IQuestionAdapter[]> toRepeat)
        {
            _repeatQuestion = repeatQuestion;
            _toRepeat = toRepeat;

            SetupWith(toRepeat());
        }

        public override QuestionaireResult AnswerQuestionWith(IncomingMessage message)
        {
            var answer = base.AnswerQuestionWith(message);
            
            if (IsComplete && _askToRepeat)
            {
                SetupWith(ShouldRepeatQuestion());
            }

            return answer;
        }

        private IQuestionAdapter ShouldRepeatQuestion()
        {
            return Questions.ForQuestionaire(Questions.Ask(_repeatQuestion, userAnswer =>
            {
                var repeat = SlothVocabulary.Parser.AsYesNo(userAnswer);
                if (repeat == null) return QuestionResult.Error("say again?");
                if (!repeat.Value) _askToRepeat = false;

                return QuestionResult.Ok(repeat.Value ? "Ok, starting again" : "next question then", repeat.Value ? _toRepeat() : null);
            }));
        }
    }

}
