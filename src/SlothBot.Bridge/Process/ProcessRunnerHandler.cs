using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using SlothBot.MessagingPipeline;
using SlothBot.Questionaire;

namespace SlothBot.Bridge.Process
{
    public class ProcessRunnerHandler : QuestionnaireMessageHanlder<ProcessMetaQuestionaire>
    {
        private readonly ICreateProcesses _processFactory;
        private readonly IProcessCreatorRepository _runnableProcesses;

        public ProcessRunnerHandler(ICreateProcesses processFactory, IProcessCreatorRepository runnableProcesses)
        {
            _processFactory = processFactory;
            _runnableProcesses = runnableProcesses;
            _runnableProcesses = runnableProcesses;
        }

        public override ProcessMetaQuestionaire QuestionaireFor(IncomingMessage message)
        {
            return new ProcessMetaQuestionaire(_runnableProcesses.GetProcessForUse(GetProcessToRun(message)));
        }

        protected override bool TriggersAQuestionaire(IncomingMessage message)
        {
            return message.BotIsMentioned && _runnableProcesses.CanUseAlias(message.UserEmail, GetProcessToRun(message));
        }

        private string GetProcessToRun(IncomingMessage message)
        {
            if (message.TargetedText.IsNullOrWhiteSpace()) return null;
            var hasSpace = message.TargetedText.IndexOf(' ');
            return message.TargetedText.Substring(0, hasSpace == -1 ? message.TargetedText.Length : hasSpace);
        }

        protected override IObservable<ResponseMessage> OnQuestionaireCompleted(ProcessMetaQuestionaire questions, string username)
        {
            return RxObservable.Create<ResponseMessage>(o =>
            {
                o.OnNext(new ResponseMessage() { Text = $"Got enough info to execute {questions.Meta.Alias}" });

                questions.Meta.LogWith(
                    info => o.OnNext(new ResponseMessage() { Text = $"[{questions.Meta.Alias}] {info}" }),
                    errors => o.OnNext(new ResponseMessage() { Text = $"[{questions.Meta.Alias}] {errors}" })
                );

                return _processFactory.Run(questions.Meta)
                                    .ToObservable()
                                    .OnErrorResumeNext(Observable.Empty<long>())
                                    .FinallyR(() => o.OnCompleted())
                                    .Subscribe();
            });
        }

        public override IEnumerable<CommandDescription> GetSupportedCommands()
        {
            return _runnableProcesses.GetAliases();
        }
    }
}

