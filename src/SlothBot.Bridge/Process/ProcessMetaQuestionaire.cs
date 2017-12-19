using System.Collections.Generic;
using System.Linq;
using SlothBot.Bridge.Process.Arguments;
using SlothBot.MessagingPipeline;
using SlothBot.Questionaire;

namespace SlothBot.Bridge.Process
{
    public class ProcessMetaQuestionaire : IQuestionaire
    {
        public Queue<UserSpecifiedProcessArgument> UserSpecifiedArguments { get; private set; }
        public string Description { get; private set; }
        public IQuestionAdapter Current => NextArgumentQuestion();
        public bool IsComplete => UserSpecifiedArguments.Count == 0;

        public ProcessMeta Meta { get; private set; }

        public ProcessMetaQuestionaire(ProcessMeta processMeta)
        {
            Meta = processMeta;
            Description = $"Things i need to know before i can run {processMeta.Alias}";
            UserSpecifiedArguments = new Queue<UserSpecifiedProcessArgument>();

            foreach (var userArgument in processMeta.Arguments.Values.OfType<UserSpecifiedProcessArgument>().ToArray())
            {
                UserSpecifiedArguments.Enqueue(userArgument);
            }
        }

        public void MarkQuestionAsComplete()
        {
            UserSpecifiedArguments.Dequeue();
        }

        private IQuestionAdapter NextArgumentQuestion()
        {
            if (UserSpecifiedArguments.Count > 0)
            {
                var next = UserSpecifiedArguments.Peek();
                return Questions.ForQuestionaire(new Question($"{next.Description}?", (answer) =>
                {
                    next.Set(answer);
                    return QuestionResult.Ok();
                }));
            }

            return null;
        }

        public QuestionaireResult AnswerQuestionWith(IncomingMessage message)
        {
            var reply = NextArgumentQuestion().AnswerQuestionWith(message);

            if(reply.WasSuccessful) MarkQuestionAsComplete();

            return reply.WasSuccessful ? QuestionaireResult.Ok(reply.Response) : QuestionaireResult.Error(reply.Response);
        }
    }
}
