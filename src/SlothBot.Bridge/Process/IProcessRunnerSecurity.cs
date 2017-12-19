namespace SlothBot.Bridge.Process
{
    public interface IProcessRunnerSecurity
    {
        string WorkingDirectory { get; }
        bool IsSecure { get; }
    }
}
