using xdPlayer.Application.Helpers;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Application.Interfaces;

public interface ILibraryService
{
    Task<Track?> GetByIdAsync(int id);
    Task<Track?> GetByIdWithTagsAsync(int id);

    Task<Track> AddFileAsync(string filePath);
    Task<IEnumerable<Track>> AddFolderAsync(string folderPath);

    Task<IEnumerable<Track>> GetAllAsync();
    Task<PagedResult<Track>> GetPagedAsync(int page, int pageSize);

    Task<Track> SetTrackCoverAsync(int trackId, string imageFilePath);

    Task<IEnumerable<Track>> SearchAsync(string query);
    Task<IEnumerable<Track>> FilterAsync(LibraryFilter filter);

    Task<IEnumerable<Track>> SortAsync(IEnumerable<Track> tracks, SortField field, bool ascending);

    Task DeleteTrackAsync(int trackId);

    Task<Track> ToggleLikeAsync(int trackId);

    Task<Track> UpdateTrackMetadataAsync(int trackId, string title, string? artist, string? album, string? genre, int? year, string? musicBrainzId);
    Task<Track> ResetMetadataFromFileAsync(int trackId);
}