using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace xdPlayer.Launcher.Services;

public static class FileVerificationService
{
    public static bool IsValidZip(string path)
    {
        try
        {
            using var archive = System.IO.Compression.ZipFile.OpenRead(path);

            return archive.Entries.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidApplication(string folder)
    {
        return

            File.Exists(Path.Combine(folder, "xdPlayer.App.exe"))

            &&

            File.Exists(Path.Combine(folder, "version.json"));
    }
}