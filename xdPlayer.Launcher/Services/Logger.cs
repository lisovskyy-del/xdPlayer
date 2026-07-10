using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Updater.Services;

namespace xdPlayer.Launcher.Services;

public static class Logger
{
    public static void Info(string message)
    {
        Write("INFO", message);
    }

    public static void Error(string message)
    {
        Write("ERROR", message);
    }

    private static void Write(string level, string message)
    {
        PathService.CreateDirectories();

        var line =
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

        Console.WriteLine(line);

        File.AppendAllText(
            PathService.LauncherLog,
            line + Environment.NewLine);
    }
}