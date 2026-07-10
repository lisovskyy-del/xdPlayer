using System.Diagnostics;

namespace xdPlayer.Launcher.Services;

public static class ProcessService
{
    public static bool IsPlayerRunning()
    {
        return Process.GetProcessesByName("xdPlayer.App").Any();
    }

    public static void KillPlayer()
    {
        foreach (var process in Process.GetProcessesByName("xdPlayer.App"))
        {
            try
            {
                process.Kill(true);
                process.WaitForExit();
            }
            catch
            {
                // Ignore
            }
        }
    }

    public static void StartPlayer()
    {
        if (!File.Exists(PathService.PlayerExecutable))
            throw new FileNotFoundException(
                "Player executable not found.",
                PathService.PlayerExecutable);

        Process.Start(new ProcessStartInfo
        {
            FileName = PathService.PlayerExecutable,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(PathService.PlayerExecutable)!
        });
    }
}