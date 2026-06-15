using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Domain.Interfaces;

public interface IUnitOfWork
{
    ITrackRepository Tracks { get; }
    IPlaylistRepository Playlists { get; }
    ITagRepository Tags { get; }
    Task<int> SaveChangesAsync();
}