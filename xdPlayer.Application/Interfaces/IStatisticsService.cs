using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Application.Interfaces;

public class ProfileOverview
{
    public int TracksInLibrary { get; set; }
    public int LikedTracksCount { get; set; }
    public int TotalPlayTimeSeconds { get; set; }
    public int DaysListened { get; set; }
    public string? MostPlayedGenre { get; set; }
}

public interface IStatisticsService
{
    Task<ProfileOverview> GetOverviewAsync();
    Task<UserProfile?> GetUserProfileAsync();
}