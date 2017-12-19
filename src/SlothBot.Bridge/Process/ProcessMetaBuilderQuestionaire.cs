using System.Linq;
using System.Text;
using SlothBot.Bridge.Process.Arguments;
using SlothBot.Questionaire;

namespace SlothBot.Bridge.Process
{
    public class ProcessMetaBuilderQuestionaire : NestedQuestionaire<ProcessMeta>
    {
        public ProcessMetaBuilderQuestionaire(string alias, string processName) : base(
            "Creates a new process with a set of known parameters that can be then executed by others")
        {
            Result = ProcessMeta.CreateAlias(alias)
                                .ForProcess(processName);

            SetupWith(
                ArgumentsQuestion().ForQuestionaire(),
                PermissionsQuestion().ForQuestionaire(),
                TimeoutQuetion().ForQuestionaire(),
                DescriptionQuestion().ForQuestionaire()
            );
        }
        
        private IQuestion PermissionsQuestion()
        {
            return Questions.Ask("Which users can run the process? [Any || usernames, comma, seperated]",
                answer =>
                {
                    if (string.IsNullOrWhiteSpace(answer))
                        return QuestionResult.Error("You must specifcy atleast 1 persion");

                    var usernames = answer.Split(',').Select(u => u.Trim()).ToArray();
                    if(usernames.Length > 1)
                        return QuestionResult.Error("You must specifcy atleast 1 persion");

                    Result.GiveUsersPermission(usernames);

                    return QuestionResult.Ok($"Given access to {usernames.ToStringEach()}");
                });
        }
        private IQuestionaire ArgumentsQuestion()
        {
            return Questions.OptionallyRepeat(() => new[]
            {
                new ArgumentsQuestionaire(Result.Alias).OnComplete(r =>
                {
                    Result.Arguments.Add(r.Result.Name, r.Result);
                }).ForQuestionaire()
            },
            "Would you like to add more arguments?")
            .OnlyAskIf("Are there any arguments you want to specify for this process?");
        }


        private IQuestion TimeoutQuetion()
        {
            return Questions.Ask(
                "What is the maximum time this command should run for before trying to terminate it? Dont provide an answer to skip.", answer =>
                {
                    var timeout = SlothVocabulary.Parser.AsTimeSpan(answer);

                    if (timeout.HasValue)
                    {
                        Result.ExecutionTimeout = timeout.Value;
                    }

                    return timeout.HasValue
                        ? QuestionResult.Ok("Timeout set")
                        : answer.IsNullOrWhiteSpace() ? QuestionResult.Ok("No timelimit set") : QuestionResult.Error("I didnt understand that time span.. use [hh:mm:ss]...");
                });
        }


        private IQuestion DescriptionQuestion()
        {
            return Questions.Ask("Now give a meaninful description of this process so others know what it does",
                answer =>
                {
                    Result.WithDescription(answer);

                    return QuestionResult.Ok();
                });
        }
    }

    public enum ArgumentType
    {
        Defined,
        User,
        File
    }

    public class ArgumentTypeResult
    {
        public ArgumentType ArgType { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public IProcessArgument Result { get; set; }
    }

    public class ArgumentsQuestionaire : NestedQuestionaire<ArgumentTypeResult>
    {
        public ArgumentsQuestionaire(string processName) : base($"Specify an argument for {processName}")
        {
            Result = new ArgumentTypeResult();

            SetupWith(
                Questions.ForQuestionaire(new Question("What is the name of the argument?", name =>
                {
                    Result.Name = name;
                    return QuestionResult.Ok();
                })),
                Questions.ForQuestionaire(new Question(
                    "Where will this argument come from? i'll _define_ it now, the _user_ who runs it, from a _file_",
                    answer =>
                    {
                        switch (answer)
                        {
                            case "define":
                                Result.ArgType = ArgumentType.Defined;
                                return QuestionResult.Ok(Questions.Ask("What shall its value always be then?",
                                    value =>
                                    {
                                        Result.Result = new InMemoryProcessArgument(Result.Name, value);
                                        return QuestionResult.Ok();
                                    }));
                            case "user":
                                Result.ArgType = ArgumentType.User;
                                return QuestionResult.Ok(Questions.Ask(
                                    "What question shall as i ask the user when prompting for this argument later?",
                                    description =>
                                    {
                                        Result.Result = new UserSpecifiedProcessArgument(Result.Name, description);
                                        return QuestionResult.Ok();
                                    }));
                            case "file":
                                return QuestionResult.Ok(Questions.Ask("What is the filename (including the path?)",
                                    filename =>
                                    {
                                        Result.Result =
                                            new FileContentsProcessArgument(filename, Result.Name, Encoding.UTF8);
                                        return QuestionResult.Ok();
                                    }));
                                default:
                                    return QuestionResult.Error("Enter either define, user or file");
                        }
                    })));
        }

    }
}
