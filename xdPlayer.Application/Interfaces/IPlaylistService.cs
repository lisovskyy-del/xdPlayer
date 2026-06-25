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

    Task RenameAsync(int playlistId, string newName);
    Task AddTrackAsync(int playlistId, int trackId);
    Task RemoveTrackAsync(int playlistId, int trackId);

    Task DeleteAsync(int playlistId);
}