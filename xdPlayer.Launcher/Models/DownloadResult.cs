using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Launcher.Models;

public class DownloadResult
{
    public bool Success { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string Error { get; set; } = string.Empty;
}