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
    Task<List<TopTrackItem>> GetTopTracksAsync(StatsPeriod period);
    Task<List<DailyPlayCount>> GetPlaysPerDayAsync(StatsPeriod period);
    Task BackfillDailyStatisticsAsync();
}

public enum StatsPeriod { Days7, Days30, Days90, Year1, All }

public class TopTrackItem
{
    public int TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string? CoverImagePath { get; set; }
    public int PlayCount { get; set; }
}

public class DailyPlayCount
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}