using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Launcher.Models;

namespace xdPlayer.Launcher.Services;

public static class DownloadService
{
    public static async Task<DownloadResult> DownloadAsync(
        string url,
        string destination)
    {
        try
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "xdPlayerLauncher");

            var bytes = await client.GetByteArrayAsync(url);

            await File.WriteAllBytesAsync(destination, bytes);

            return new DownloadResult
            {
                Success = true,
                FilePath = destination
            };
        }
        catch (Exception ex)
        {
            return new DownloadResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}