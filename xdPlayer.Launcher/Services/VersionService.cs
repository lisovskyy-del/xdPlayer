using System.Diagnostics;

namespace xdPlayer.Launcher.Services;

public static class VersionService
{
    public static Version GetInstalledVersion()
    {
        if (!File.Exists(PathService.PlayerExecutable))
            return new Version(0, 0, 0);

        var versionString = FileVersionInfo
            .GetVersionInfo(PathService.PlayerExecutable)
            .FileVersion;

        return Parse(versionString);
    }

    public static Version GetReleaseVersion(string tagName)
    {
        return Parse(tagName);
    }

    public static bool IsUpdateAvailable(string tagName)
    {
        return GetReleaseVersion(tagName) > GetInstalledVersion();
    }

    private static Version Parse(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return new Version(0, 0, 0);

        version = version.Trim();

        if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            version = version[1..];

        if (Version.TryParse(version, out var parsed))
            return parsed;

        return new Version(0, 0, 0);
    }
}