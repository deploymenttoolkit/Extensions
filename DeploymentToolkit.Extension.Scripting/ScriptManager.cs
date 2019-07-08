using DeploymentToolkit.Extension.Scripts.Actions;
using DeploymentToolkit.Extension.Scripts.Modals;
using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;

namespace DeploymentToolkit.Extension.Scripts
{
    internal static class ScriptManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        internal static bool StartScript(ScriptExecute script)
        {
            _logger.Trace($"StartScript({script.ScriptType}, {script.LaunchType}, {script.ScriptPath}, {script.WaitForExit})");

            switch (script.ScriptType)
            {
                case ScriptType.Batch:
                    return StartBatchScript(script);

                case ScriptType.PowerShell:
                    return StartPowerShellScript(script);

                case ScriptType.VBS:
                    return StartVBSScript(script);

                default:
                    return false;
            }
        }

        private static bool StartBatchScript(ScriptExecute script)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{script.ScriptPath}\"",

                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = script.LaunchType == LaunchType.External
            };

            return StartProcess(script, startInfo);
        }

        private static bool StartPowerShellScript(ScriptExecute script)
        {
            if (script.LaunchType == LaunchType.External)
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass & \"{script.ScriptPath}\"",

                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                };

                return StartProcess(script, startInfo);
            }
            else
            {
                if (!File.Exists(script.ScriptPath))
                    return false;

                try
                {
                    var scriptText = File.ReadAllText(script.ScriptPath);
                    using (var powershellInstance = PowerShell.Create())
                    {
                        powershellInstance.AddScript(scriptText);
                        powershellInstance.Invoke();
                    }
                    _logger.Info("Successfully invoked PowerShell script");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to execute PowerShell script");
                    return false;
                }
            }
        }

        private static bool StartVBSScript(ScriptExecute script)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "cscript",
                Arguments = $"/B /NoLogo \"{script.ScriptPath}\"",

                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = script.LaunchType == LaunchType.External
            };

            return StartProcess(script, startInfo);
        }

        private static bool StartProcess(ScriptExecute script, ProcessStartInfo startInfo)
        {
            try
            {
                _logger.Trace($"Final Commandline: {startInfo.FileName} {startInfo.Arguments}");

                var process = Process.Start(startInfo);

                _logger.Debug($"Started process with pid {process.Id} in session {process.SessionId}");

                if (script.WaitForExit)
                {
                    process.WaitForExit();
                    _logger.Info($"Process exited with ExitCode {process.ExitCode}");
                }
                else
                {
                    _logger.Info($"Not waiting for exit of {process.Id}");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error while trying to execute {script.ScriptType} script");
                return false;
            }
        }
    }
}
