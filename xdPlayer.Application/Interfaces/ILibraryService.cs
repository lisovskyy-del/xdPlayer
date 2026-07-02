using xdPlayer.Application.Helpers;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Application.Interfaces;

public interface ILibraryService
{
    Task<Track> AddFileAsync(string filePath);
    Task<IEnumerable<Track>> AddFolderAsync(string folderPath);

    Task<IEnumerable<Track>> GetAllAsync();
    Task<PagedResult<Track>> GetPagedAsync(int page, int pageSize);

    Task<Track> SetTrackCoverAsync(int trackId, string imageFilePath);

    Task<IEnumerable<Track>> SearchAsync(string query);
    Task<IEnumerable<Track>> FilterAsync(LibraryFilter filter);

    Task<IEnumerable<Track>> SortAsync(IEnumerable<Track> tracks, SortField field, bool ascending);

    Task DeleteTrackAsync(int trackId);
}