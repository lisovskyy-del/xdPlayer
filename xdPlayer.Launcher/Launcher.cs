using xdPlayer.Launcher.Services;
using System.Diagnostics;

namespace xdPlayer.Launcher;

public sealed class Launcher
{
    public async Task RunAsync()
    {
        try
        {
            if (!SingleInstanceService.TryAcquire())
                return;

            PrintHeader();

            Logger.Info("Launcher started.");

            await UpdateService.UpdateIfNeeded();

            Logger.Info("Starting player...");

            ProcessService.StartPlayer();

            Logger.Info("Player started.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());

            Console.WriteLine(ex.Message);
            Console.ReadKey();
        }
    }

    private static void PrintHeader()
    {
        Console.Clear();

        Console.WriteLine("=================================");
        Console.WriteLine("         xdPlayer Launcher");
        Console.WriteLine("=================================");
        Console.WriteLine();
    }
}