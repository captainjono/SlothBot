using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SlothBot.Bridge.Process.Arguments;

namespace SlothBot.Bridge.Process
{
    public class OsProcessRunner : ICreateProcesses
    {
        private readonly IProcessRunnerSecurity _security;
        public readonly string DefaultSecurWorkingDirectory = Path.Combine(AppContext.BaseDirectory, "RunnableProcesses");

        public OsProcessRunner(IProcessRunnerSecurity security)
        {
            _security = security;
        }

        public Task<long> Run(ProcessMeta meta)
        {
            var processResult = new TaskCompletionSource<long>();
            var filename = Path.GetFileName(meta.ProcessName);
            var workingDirectory = Path.GetDirectoryName(meta.ProcessName) ??
                                   DefaultSecurWorkingDirectory;

            var process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false, //cant redirect stdout if this is set
                    FileName = StopTravesalInjectionAttacks(filename, _security.IsSecure, null),
                    WorkingDirectory = StopTravesalInjectionAttacks(workingDirectory, _security.IsSecure, _security.WorkingDirectory.IfIsNullOrWhiteSpace(DefaultSecurWorkingDirectory)),
                    Arguments = meta.Arguments.Values.ToCmdLineArguments()
                }
            };
            process.OutputDataReceived += OnStandardOutput;
            process.ErrorDataReceived += OnStandardError;
            process.EnableRaisingEvents = true;
            process.Exited += OnProcessExited;

            try
            {
                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
            }
            catch (Exception e)
            {
                if (!_security.IsSecure)
                {
                    meta.OnError(e.Message);
                }
                processResult.SetResult(1);
            }

            var cancel = new CancellationToken();
            cancel.Register(() =>
            {
                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception e)
                    {
                        meta.OnError($"Could not terminte process {meta.Alias} because {e}");
                    }
                }
            });

            return processResult.Task;
            
            void OnStandardOutput(object sender, DataReceivedEventArgs e)
            {
                meta.OnInfo(e.Data);
            }

            void OnStandardError(object sender, DataReceivedEventArgs e)
            {
                meta.OnError(e.Data);
            }

            void OnProcessExited(object sender, EventArgs e)
            {
                if (process.ExitCode == 0)
                {
                    meta.OnInfo("Finished");

                }
                else
                {
                    meta.OnError($"The process terminted unusually with {process.ExitCode}");
                }

                processResult.SetResult(process.ExitCode);
            }
        }


        private string StopTravesalInjectionAttacks(string fileOrPath, bool securityIsSecure, string secureValue)
        {
            if (securityIsSecure && secureValue != null) return secureValue;

            return fileOrPath.Replace("%", "").Replace("..", "");
        }
    }
    
    public static class ProcessExentions
    {
        public static string ToCmdLineArguments(this IEnumerable<IProcessArgument> arguments)
        {
            return String.Join(" ", arguments.Select(ToShellArgument)).Trim();

            string ToShellArgument(IProcessArgument arg)
            {
                if (string.IsNullOrWhiteSpace(arg.Name))
                {
                    return Escape(arg.Value);
                }

                return $"-{arg.Name} {Escape(arg.Value)}";
            }

            string Escape(string argument)
            {
                if (argument.IsNullOrWhiteSpace()) return null;
                if (argument.Contains(" "))
                {
                    argument = $"\"{argument}\"";
                }

                if (argument.Contains("\\"))
                {
                    argument = argument.Replace("\\", "\\\\");
                }

                return argument;
            }
        }
    }
}
