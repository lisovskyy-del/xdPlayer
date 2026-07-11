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
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(10)
            };

            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "xdPlayerLauncher");

            await using var stream =
                await client.GetStreamAsync(url);

            await using var file =
                File.Create(destination);

            await stream.CopyToAsync(file);

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