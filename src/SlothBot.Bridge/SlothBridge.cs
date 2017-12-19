using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SlothBot.Bridge.Process;
using SlothBot.MessagingPipeline;

namespace SlothBot.Bridge
{
    public static class SlothBridge
    {
        public static ISlothBot Create(ISlothBridgeConfig cfg,  ISlothLog logging = null)
        {
            var processFactory = new OsProcessRunner(cfg);
            var possibleProcesses = ReadOptimisedJsonFilePersistantDictionary<string, ProcessSecurity>.From(cfg.PossibleProcessesFullName ?? "enabledProcesses.json", v => v.ProcessName);
            var aliases = ReadOptimisedJsonFilePersistantDictionary<string, ProcessMeta>.From(cfg.AliasedProcessesFullName ?? "aliases.json", v => v.Alias);

            var processRepo = new SecuredProcessRepository(possibleProcesses, aliases);

            return new SlothBot(cfg, logging, Create(processRepo, processFactory));
        }

        public static IMessageHandler[] Create(IProcessCreatorRepository processRepo, ICreateProcesses processFactory)
        {
            var processMetaCreator = new ProcessMetaRunnerCreatorHandler(processRepo);
            var processMetaRunner = new ProcessRunnerHandler(processFactory, processRepo);
            
            return new IMessageHandler[]
            {
                processMetaCreator,
                processMetaRunner
            };
        }

        public static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            NullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static T MakeDeepCopy<T>(this T context)
        {
            return context.ToJson().FromJson<T>();
        }

        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj, _jsonSettings);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
        }
    }
}
