using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using TagLib;

namespace xdPlayer.Infrastructure.Services;

// simple taglib for a song to have multiple rows of data to paste it in the DB.
public class TagLibMetadataReader : IMetadataReader
{
    public Track ReadMetadata(string filePath)
    {
        var file = TagLib.File.Create(filePath);

        string? coverPath = null;

        if (file.Tag.Pictures.Length > 0)
        {
            var picture = file.Tag.Pictures[0];

            byte[] imageBytes = picture.Data.Data;

            var coversDir = Path.Combine(AppContext.BaseDirectory, "Covers");
            Directory.CreateDirectory(coversDir);

            coverPath = Path.Combine("Covers", $"{Guid.NewGuid()}.jpg");

            System.IO.File.WriteAllBytes(coverPath, imageBytes);
        }

        return new Track
        {
            FilePath = filePath,

            Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),

            Artist = file.Tag.FirstPerformer,

            Album = file.Tag.Album,

            Genre = file.Tag.FirstGenre,

            Year = file.Tag.Year > 0 ? (int)file.Tag.Year : null,

            DurationSeconds = (int)file.Properties.Duration.TotalSeconds,

            CoverImagePath = coverPath,

            AddedAt = DateTime.UtcNow,
        };
    }

    public void WriteMetadata(string filePath, string title, string? artist, string? album, string? genre, int? year)
    {
        var file = TagLib.File.Create(filePath);

        file.Tag.Title = title;
        file.Tag.Performers = artist != null ? new[] { artist } : Array.Empty<string>();
        file.Tag.Album = album;
        file.Tag.Genres = genre != null ? new[] { genre } : Array.Empty<string>();
        if (year.HasValue) file.Tag.Year = (uint)year.Value;

        file.Save();
    }
}