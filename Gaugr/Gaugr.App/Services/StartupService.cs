using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;

namespace Gaugr.App.Services;

/// <summary>
/// Starts Gaugr at sign-in through a scheduled task. A plain Run registry entry cannot
/// launch an app that requires administrator rights, so a highest-privilege task is used.
/// </summary>
public sealed class StartupService
{
    private const string TaskName = "Gaugr";

    public bool IsEnabled() => RunSchtasks($"/Query /TN \"{TaskName}\"") == 0;

    public bool Enable()
    {
        var exe = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exe)) return false;

        var xmlPath = Path.Combine(Path.GetTempPath(), "gaugr-startup-task.xml");
        try
        {
            File.WriteAllText(xmlPath, BuildTaskXml(exe), Encoding.Unicode);
            return RunSchtasks($"/Create /TN \"{TaskName}\" /XML \"{xmlPath}\" /F") == 0;
        }
        catch { return false; }
        finally
        {
            try { File.Delete(xmlPath); } catch { }
        }
    }

    public bool Disable() => !IsEnabled() || RunSchtasks($"/Delete /TN \"{TaskName}\" /F") == 0;

    // The XML form allows battery use and removes the default 72-hour limit, which the
    // command-line switches of schtasks cannot do.
    private static string BuildTaskXml(string exe)
    {
        var user = SecurityElement.Escape($"{Environment.UserDomainName}\\{Environment.UserName}");
        var command = SecurityElement.Escape(exe);
        var folder = SecurityElement.Escape(Path.GetDirectoryName(exe) ?? "");

        return $"""
            <?xml version="1.0" encoding="UTF-16"?>
            <Task version="1.4" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
              <Triggers>
                <LogonTrigger>
                  <Enabled>true</Enabled>
                  <UserId>{user}</UserId>
                </LogonTrigger>
              </Triggers>
              <Principals>
                <Principal id="Author">
                  <UserId>{user}</UserId>
                  <LogonType>InteractiveToken</LogonType>
                  <RunLevel>HighestAvailable</RunLevel>
                </Principal>
              </Principals>
              <Settings>
                <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
                <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
                <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
                <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
                <Enabled>true</Enabled>
              </Settings>
              <Actions Context="Author">
                <Exec>
                  <Command>{command}</Command>
                  <Arguments>--minimized</Arguments>
                  <WorkingDirectory>{folder}</WorkingDirectory>
                </Exec>
              </Actions>
            </Task>
            """;
    }

    private static int RunSchtasks(string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("schtasks.exe", arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (process is null) return -1;

            process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode;
        }
        catch { return -1; }
    }
}
