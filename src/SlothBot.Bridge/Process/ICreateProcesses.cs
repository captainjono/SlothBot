using System;
using System.Threading.Tasks;

namespace SlothBot.Bridge.Process
{
    public interface ICreateProcesses
    {
        Task<long> Run(ProcessMeta meta);
    }
}
