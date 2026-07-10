using System;
using xdPlayer.Launcher.Models;

namespace xdPlayer.Launcher.Services;

public class Launcher
{
    public async Task Run()
    {
        PrintHeader();

        Logger.Info("Launcher started.");

        await UpdateService.UpdateIfNeeded();

        ProcessService.StartPlayer();
    }

    private void PrintHeader()
    {
        Console.WriteLine("=================================");
        Console.WriteLine("        xdPlayer Launcher");
        Console.WriteLine("=================================");
        Console.WriteLine();
    }
}