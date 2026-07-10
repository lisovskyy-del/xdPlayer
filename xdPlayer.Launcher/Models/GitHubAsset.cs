using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace xdPlayer.Launcher.Models;

public class GitHubAsset
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; set; } = string.Empty;
}