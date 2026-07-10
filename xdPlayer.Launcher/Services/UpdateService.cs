namespace xdPlayer.Launcher.Services;

public static class UpdateService
{
    public static async Task UpdateIfNeeded()
    {
        PathService.CreateDirectories();

        Logger.Info("Checking installation...");

        if (!InstallationService.IsInstalled())
        {
            Logger.Info("Player is not installed.");
            await InstallLatestVersion();
            return;
        }

        Logger.Info("Checking latest release...");

        var release = await GitHubService.GetLatestReleaseAsync();

        if (release is null)
        {
            Logger.Error("Unable to get latest release.");
            return;
        }

        Logger.Info($"Installed version: {VersionService.GetInstalledVersion()}");
        Logger.Info($"Latest version: {VersionService.GetReleaseVersion(release.TagName)}");

        if (!VersionService.IsUpdateAvailable(release.TagName))
        {
            Logger.Info("Already up to date.");
            return;
        }

        Logger.Info("Update available.");

        ProcessService.KillPlayer();

        await InstallLatestVersion();
    }

    private static async Task InstallLatestVersion()
    {
        var release = await GitHubService.GetLatestReleaseAsync();

        if (release is null)
            throw new Exception("Unable to get latest release.");

        var asset = GitHubService.GetApplicationPackage(release);

        if (asset is null)
            throw new Exception("Release package not found.");

        Logger.Info($"Downloading {asset.Name}");

        PathService.ClearTemp();

        var result = await DownloadService.DownloadAsync(
            asset.DownloadUrl,
            PathService.DownloadedArchive);

        if (!result.Success)
            throw new Exception(result.Error);

        Logger.Info("Download complete.");

        if (!ArchiveService.Verify(PathService.DownloadedArchive))
            throw new Exception("Downloaded archive is invalid.");

        Logger.Info("Extracting archive...");

        ArchiveService.Extract(PathService.DownloadedArchive);

        if (!ArchiveService.Validate())
            throw new Exception("Archive validation failed.");

        Logger.Info("Installing...");

        InstallationService.Install(PathService.ExtractedDirectory);

        PathService.ClearTemp();

        Logger.Info("Installation completed.");
    }
}