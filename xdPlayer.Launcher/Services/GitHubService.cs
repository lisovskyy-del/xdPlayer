using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using xdPlayer.Launcher.Models;

namespace xdPlayer.Launcher.Services;

public static class GitHubService
{
    private const string Owner = "lisovskyy-del";
    private const string Repository = "xdPlayer";

    public static async Task<GitHubRelease> GetLatestReleaseAsync()
    {
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Add("User-Agent", "xdPlayerLauncher");

        var url =
            $"https://api.github.com/repos/{Owner}/{Repository}/releases/latest";

        var json = await client.GetStringAsync(url);

        return JsonSerializer.Deserialize<GitHubRelease>(json)!;
    }

    public static GitHubAsset GetApplicationPackage(GitHubRelease release)
    {
        var asset = release.Assets.FirstOrDefault(asset =>
            asset.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
            asset.Name.StartsWith("xdPlayer", StringComparison.OrdinalIgnoreCase));

        if (asset is null)
            throw new Exception("Application package not found.");

        return asset;
    }
}