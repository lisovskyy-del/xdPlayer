using Microsoft.Extensions.DependencyInjection;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Application.Models;

public class StatisticsService : IStatisticsService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public StatisticsService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private static DateTime GetPeriodStart(StatsPeriod period) => period switch
    {
        StatsPeriod.Days7 => DateTime.UtcNow.AddDays(-7),
        StatsPeriod.Days30 => DateTime.UtcNow.AddDays(-30),
        StatsPeriod.Days90 => DateTime.UtcNow.AddDays(-90),
        StatsPeriod.Year1 => DateTime.UtcNow.AddYears(-1),
        _ => DateTime.MinValue
    };

    public async Task<List<TopTrackItem>> GetTopTracksAsync(StatsPeriod period)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var from = GetPeriodStart(period);
        var sessions = await uow.ListeningSessions.GetSessionsSinceAsync(from);

        return sessions
            .GroupBy(s => s.TrackId)
            .Select(g => new TopTrackItem
            {
                TrackId = g.Key,
                Title = g.First().Track.Title,
                Artist = g.First().Track.Artist,
                CoverImagePath = g.First().Track.CoverImagePath,
                PlayCount = g.Count()
            })
            .OrderByDescending(t => t.PlayCount)
            .Take(10)
            .ToList();
    }

    public async Task<List<DailyPlayCount>> GetPlaysPerDayAsync(StatsPeriod period)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var from = period == StatsPeriod.All
            ? DateTime.UtcNow.AddDays(-30)
            : GetPeriodStart(period);

        var sessions = await uow.ListeningSessions.GetSessionsSinceAsync(from);

        var grouped = sessions
            .GroupBy(s => DateOnly.FromDateTime(s.StartedAt.Date))
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new List<DailyPlayCount>();
        var startDate = DateOnly.FromDateTime(from.Date);
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            result.Add(new DailyPlayCount
            {
                Date = date,
                Count = grouped.TryGetValue(date, out var count) ? count : 0
            });
        }

        return result;
    }

    public async Task<ProfileOverview> GetOverviewAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var allTracks = (await uow.Tracks.GetAllAsync()).ToList();
        var likedTracks = allTracks.Where(t => t.IsLiked).ToList();

        var totalPlayTimeSeconds = (int)allTracks.Sum(t => t.TotalListenedSeconds);

        var allSessions = await uow.ListeningSessions.GetSessionsSinceAsync(DateTime.MinValue);
        var daysListened = allSessions
            .Select(s => s.StartedAt.Date)
            .Distinct()
            .Count();

        var mostPlayedGenre = allTracks
            .Where(t => !string.IsNullOrWhiteSpace(t.Genre))
            .GroupBy(t => t.Genre)
            .OrderByDescending(g => g.Sum(t => t.PlayCount))
            .Select(g => g.Key)
            .FirstOrDefault();

        return new ProfileOverview
        {
            TracksInLibrary = allTracks.Count,
            LikedTracksCount = likedTracks.Count,
            TotalPlayTimeSeconds = totalPlayTimeSeconds,
            DaysListened = daysListened,
            MostPlayedGenre = mostPlayedGenre ?? "—"
        };
    }

    public async Task<UserProfile?> GetUserProfileAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var profiles = await uow.UserProfiles.GetAllAsync();
        return profiles.FirstOrDefault();
    }
}