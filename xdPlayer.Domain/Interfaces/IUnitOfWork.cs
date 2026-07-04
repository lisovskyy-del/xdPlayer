using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Domain.Interfaces;

public interface IUnitOfWork
{
    ITrackRepository Tracks { get; }
    IPlaylistRepository Playlists { get; }
    ITagRepository Tags { get; }
    IListeningSessionRepository ListeningSessions { get; }
    IRepository<UserProfile> UserProfiles { get; }
    Task<int> SaveChangesAsync();

    IDailyStatisticsRepository DailyStatistics { get; }
}