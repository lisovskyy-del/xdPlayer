using xdPlayer.Launcher.Models;

namespace xdPlayer.Launcher.Services;

public static class UpdateService
{
    public static async Task UpdateIfNeeded()
    {
        PathService.CreateDirectories();

        Logger.Info("Checking latest release...");

        var release = await GitHubService.GetLatestReleaseAsync();

        if (release is null)
        {
            Logger.Error("Unable to get latest release.");
            return;
        }

        Logger.Info("Checking player...");

        if (!File.Exists(PathService.PlayerExecutable))
        {
            Logger.Info("Player not found.");

            await InstallLatestVersion(release);
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

        await InstallLatestVersion(release);
    }

    private static async Task InstallLatestVersion(GitHubRelease release)
    {
        var asset = GitHubService.GetApplicationPackage(release);

        if (asset is null)
            throw new Exception("Release package not found.");

        Logger.Info($"Downloading {asset.Name}");

        PathService.ClearTemp();

        try
        {
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

            ArchiveService.Install();

            Logger.Info("Installation completed.");
        }
        finally
        {
            PathService.ClearTemp();
        }
    }
}