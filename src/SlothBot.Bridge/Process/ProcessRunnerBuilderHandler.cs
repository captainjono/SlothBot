using System;
using System.Collections.Generic;
using System.Linq;
using SlothBot.Bridge.Vocabulary;
using SlothBot.MessagingPipeline;
using SlothBot.Questionaire;

namespace SlothBot.Bridge.Process
{
    public class ProcessMetaRunnerCreatorHandler : QuestionnaireMessageHanlder<ProcessMetaBuilderQuestionaire>
    {
        private readonly IProcessCreatorRepository _processRepo;

        public ProcessMetaRunnerCreatorHandler(IProcessCreatorRepository processRepo)
        {
            _processRepo = processRepo;
        }

        protected override bool TriggersAQuestionaire(IncomingMessage message)
        {
            return message.FullText.StartsWith("alias", StringComparison.OrdinalIgnoreCase);
        }

        protected override IObservable<ResponseMessage> OnQuestionaireCompleted(ProcessMetaBuilderQuestionaire questions, string username)
        {
            return RxObservable.Create(() =>
            {
                _processRepo.AliasProcess(questions.Result);
                return $"the alias '{questions.Result.Alias}' is setup and ready to use".AsReplyTo(username);
            });
        }

        public override ProcessMetaBuilderQuestionaire QuestionaireFor(IncomingMessage message)
        {
            var cmd = message.FullText.ToSlothCommands().Commands.FirstOrDefault();
            var parsed = cmd.Ensure().FormatedWith("ALIAS".AsKeyword(), "processName".AsValue(), "AS".AsKeyword(), "name".AsValue());

            var processName = parsed["processName"];
            var alias = parsed["name"];

            if (!_processRepo.CanAliasProcess(message.UserEmail, processName))
                throw new Exception("Sorry, you cant alias this process.");

            if (_processRepo.ExistsAlias(alias))
                throw new Exception("Alias is already taken. Try again.");

            return new ProcessMetaBuilderQuestionaire(alias, processName);
        }

        public override IEnumerable<CommandDescription> GetSupportedCommands()
        {
            yield return new CommandDescription()
            {
                Command = "ALIAS \"_path to process_\" AS \"_process name_\"",
                Description = "Creates an alias for a given process so other users can run it in a consistant manner"
            };
        }
    }
}
