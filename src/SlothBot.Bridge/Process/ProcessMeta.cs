using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using SlothBot.Bridge.Process.Arguments;

namespace SlothBot.Bridge.Process
{
    public class ProcessMeta
    {
        public Dictionary<string, IProcessArgument> Arguments { get; set; }
        public string ArgDelimiter { get; set; }
        
        public string Alias { get; set; }
        public string ProcessName { get; set; }
        public TimeSpan ExecutionTimeout { get; set; }
        public string[] UsernamesWhoCanUse { get; set; }
        
        [JsonIgnore]
        public Action<string> OnInfo { get; private set; }
        [JsonIgnore]
        public Action<string> OnError { get; private set; }
        public string Description { get; set; }

        public ProcessMeta()
        {
            
        }

        public ProcessMeta(string @alias)
        {
            Alias = alias;

            UsernamesWhoCanUse = new string[] {};
            
            Arguments = new Dictionary<string, IProcessArgument>();
            OnInfo = info => Debug.WriteLine($"[{Alias}][Info] {info}");
            OnError = e => Debug.WriteLine($"[{Alias}][Error] {e}");
        }

        public static ProcessMeta CreateAlias(string alias)
        {
            return new ProcessMeta(alias);
        }

        public ProcessMeta ForProcess(string location)
        {
            ProcessName = location;

            return this;
        }

        public ProcessMeta WithArgument(string name, string value)
        {
            Arguments.Add(NonNullKey(name), new InMemoryProcessArgument(name, value));

            return this;
        }

        private string NonNullKey(string name)
        {
            return !string.IsNullOrWhiteSpace(name) ? name : $"{{noarg{Arguments.Count}}}";
        }

        public ProcessMeta WithArgumentFromFile(string filename, string name)
        {
            Arguments.Add(name, new FileContentsProcessArgument(filename, name));

            return this;
        }

        public ProcessMeta WithUserSpecifiedArgument(string description, string name)
        {
            Arguments.Add(name, new UserSpecifiedProcessArgument(description, name));

            return this;
        }

        public ProcessMeta LogWith(Action<string> info, Action<string> errors)
        {
            OnInfo = info;
            OnError = errors;
            return this;
        }

        public ProcessMeta GiveUsersPermission(params string[] username)
        {
            UsernamesWhoCanUse = UsernamesWhoCanUse.Concat(username).ToArray();

            return this;
        }

        public void WithDescription(string whatProcessDoes)
        {
            Description = whatProcessDoes;
        }
    }
}
