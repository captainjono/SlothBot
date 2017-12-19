using System;
using System.Collections.Generic;
using System.Linq;
using SlothBot.MessagingPipeline;

namespace SlothBot.Bridge.Process
{
    public interface IProcessCreatorRepository
    {
        bool CanAliasProcess(string username, string processName);
        bool CanUseAlias(string username, string alias);
        void AliasProcess(ProcessMeta howToRun);
        ProcessMeta GetProcessForUse(string alias);
        bool ExistsAlias(string alias);
        IEnumerable<CommandDescription> GetAliases();
    }

    public class ProcessSecurity
    {
        public string ProcessName { get; set; }
        public string Path { get; set; }
        public string[] UsernamesWhoCanAlias { get; set; }

        //todo: dont assume - for process params, make param delimiter configurable

        public ProcessSecurity()
        {
            UsernamesWhoCanAlias = new string[] { };
        }
    }

    public class SecuredProcessRepository : IProcessCreatorRepository
    {
        private readonly IDictionary<string, ProcessMeta> _aliasedProcesses;
        private readonly IDictionary<string, ProcessSecurity> _processes;

        public SecuredProcessRepository(IDictionary<string, ProcessSecurity> possibleProcesses = null, IDictionary<string, ProcessMeta> runnableProcesses = null)
        {
            _aliasedProcesses = runnableProcesses ?? new Dictionary<string, ProcessMeta>();
            _processes = possibleProcesses ?? new Dictionary<string, ProcessSecurity>();
        }

        public bool CanAliasProcess(string username, string processName)
        {
            return _processes.ContainsKey(processName) && _processes[processName].UsernamesWhoCanAlias.Contains(username);
        }

        public bool CanUseAlias(string username, string processName)
        {
            if (processName.IsNullOrWhiteSpace()) return false;
            return _aliasedProcesses.ContainsKey(processName) 
                && _aliasedProcesses.AnyItems() 
                && (_aliasedProcesses[processName].UsernamesWhoCanUse.FirstOrDefault().Equals("any", StringComparison.OrdinalIgnoreCase) 
                    || _aliasedProcesses[processName].UsernamesWhoCanUse.Contains(username, StringComparer.OrdinalIgnoreCase));
        }

        public void AliasProcess(ProcessMeta howToRun)
        {
            _aliasedProcesses.Add(howToRun.Alias, howToRun);
        }

        public ProcessMeta GetProcessForUse(string alias)
        {
            var aliasedProcess = _aliasedProcesses[alias].MakeDeepCopy();
            //give the alias its proper runtime location at execution time
            aliasedProcess.ForProcess(_processes[aliasedProcess.ProcessName].Path);

            return aliasedProcess;
        }

        public bool ExistsAlias(string alias)
        {
            return _aliasedProcesses.ContainsKey(alias);
        }

        public IEnumerable<CommandDescription> GetAliases()
        {
            return _aliasedProcesses.Values.Select(process => new CommandDescription()
            {
                Command = process.Alias,
                Description = process.Description
            });
        }
    }
}
