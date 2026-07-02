using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Application.Interfaces;

public interface IPlaylistService
{
    Task<IEnumerable<Playlist>> GetAllAsync();
    Task<Playlist?> GetWithTracksAsync(int id);

    Task<Playlist> CreateAsync(string name, string? description = null);

    Task<Playlist> SetCoverAsync(int playlistId, string imageFilePath);

    Task RenameAsync(int playlistId, string newName);
    Task AddTrackAsync(int playlistId, int trackId);
    Task RemoveTrackAsync(int playlistId, int trackId);
    Task UpdateTrackPositionsAsync(int playlistId, IReadOnlyList<int> orderedTrackIds);

    Task DeleteAsync(int playlistId);

    Task<IEnumerable<Playlist>> GetAllWithTracksAsync();
}