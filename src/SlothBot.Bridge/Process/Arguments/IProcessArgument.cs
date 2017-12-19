using System.Threading.Tasks;

namespace SlothBot.Bridge.Process.Arguments
{
    public interface IProcessArgument
    {
        string Name { get; }
        string Value { get; }
        Task Set(string value);
    }
}
