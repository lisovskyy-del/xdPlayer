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