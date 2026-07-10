namespace xdPlayer.Launcher.Services;

public static class PathService
{
    public static string Root =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "xdPlayer");

    public static string Current =>
        Path.Combine(Root, "Current");

    public static string Backup =>
        Path.Combine(Root, "Backup");

    public static string Temp =>
        Path.Combine(Root, "Temp");

    public static string Logs =>
        Path.Combine(Root, "Logs");

    public static string PlayerExecutable =>
        Path.Combine(Current, "xdPlayer.App.exe");

    public static string DownloadedArchive =>
        Path.Combine(Temp, "xdPlayer.zip");

    public static string ExtractedDirectory =>
        Path.Combine(Temp, "Extracted");

    public static string LauncherLog =>
        Path.Combine(Logs, "launcher.log");

    public static void CreateDirectories()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(Current);
        Directory.CreateDirectory(Backup);
        Directory.CreateDirectory(Temp);
        Directory.CreateDirectory(Logs);
    }

    public static void ClearTemp()
    {
        if (Directory.Exists(Temp))
            Directory.Delete(Temp, true);

        Directory.CreateDirectory(Temp);
    }
}