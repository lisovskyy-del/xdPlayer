using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Domain.Interfaces;

public interface IPlaylistRepository : IRepository<Playlist>
{
    Task<Playlist?> GetWithTracksAsync(int id);
    Task DeleteWithTracksAsync(int playlistId);
}