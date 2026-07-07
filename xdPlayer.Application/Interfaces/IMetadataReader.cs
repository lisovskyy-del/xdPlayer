using xdPlayer.Domain.Entities;

namespace xdPlayer.Application.Interfaces;

public interface IMetadataReader
{
    Track ReadMetadata(string filePath);
    void WriteMetadata(string filePath, string title, string? artist, string? album, string? genre, int? year);
}