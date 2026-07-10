namespace xdPlayer.Launcher.Services;

public static class InstallationService
{
    public static bool IsInstalled()
    {
        return File.Exists(PathService.PlayerExecutable);
    }

    public static void Install(string sourceDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException(sourceDirectory);

        PathService.CreateDirectories();

        BackupCurrent();

        try
        {
            FileService.DeleteDirectory(PathService.Current);

            FileService.CopyDirectory(
                sourceDirectory,
                PathService.Current);

            FileService.DeleteDirectory(PathService.Backup);
        }
        catch
        {
            RestoreBackup();
            throw;
        }
    }

    private static void BackupCurrent()
    {
        if (!Directory.Exists(PathService.Current))
            return;

        FileService.DeleteDirectory(PathService.Backup);

        FileService.CopyDirectory(
            PathService.Current,
            PathService.Backup);
    }

    private static void RestoreBackup()
    {
        if (!Directory.Exists(PathService.Backup))
            return;

        FileService.DeleteDirectory(PathService.Current);

        FileService.CopyDirectory(
            PathService.Backup,
            PathService.Current);
    }
}