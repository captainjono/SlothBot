using System.Threading.Tasks;

namespace SlothBot.Bridge.Process.Arguments
{
    public class InMemoryProcessArgument : IProcessArgument
    {
        public string Name { get; }
        public string Value { get; private set; }

        public InMemoryProcessArgument(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public Task Set(string value)
        {
            Value = value;
            return Task.CompletedTask;
        }
    }
}
