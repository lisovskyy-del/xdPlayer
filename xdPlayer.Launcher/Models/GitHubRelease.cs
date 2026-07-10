using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace xdPlayer.Launcher.Models;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = string.Empty;

    [JsonPropertyName("assets")]
    public List<GitHubAsset> Assets { get; set; } = [];
}