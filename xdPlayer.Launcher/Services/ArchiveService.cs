using System.IO.Compression;


namespace xdPlayer.Launcher.Services;

public static class ArchiveService
{
    public static void Install()
    {
        foreach (var file in Directory.GetFiles(
                     PathService.ExtractedDirectory,
                     "*",
                     SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(
                PathService.ExtractedDirectory,
                file);

            if (relativePath.Equals("xdPlayer.exe",
                StringComparison.OrdinalIgnoreCase))
                continue;

            var destination = Path.Combine(
                PathService.Root,
                relativePath);

            Directory.CreateDirectory(
                Path.GetDirectoryName(destination)!);

            File.Copy(file, destination, true);
        }
    }

    public static bool Verify(string archivePath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(archivePath);
            return archive.Entries.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public static void Extract(string archivePath)
    {
        if (Directory.Exists(PathService.ExtractedDirectory))
            Directory.Delete(PathService.ExtractedDirectory, true);

        Directory.CreateDirectory(PathService.ExtractedDirectory);

        ZipFile.ExtractToDirectory(
            archivePath,
            PathService.ExtractedDirectory,
            true);
    }

    public static bool Validate()
    {
        return File.Exists(
            Path.Combine(
                PathService.ExtractedDirectory,
                "xdPlayer.App.exe"));
    }

    public static void Cleanup()
    {
        if (Directory.Exists(PathService.ExtractedDirectory))
            Directory.Delete(PathService.ExtractedDirectory, true);

        if (File.Exists(PathService.DownloadedArchive))
            File.Delete(PathService.DownloadedArchive);
    }
}