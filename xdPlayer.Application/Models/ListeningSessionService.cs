using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Application.Models;

public class ListeningSessionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private ListeningSession? _currentSession;

    public ListeningSessionService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task OnTrackStartedAsync(int trackId)
    {
        Console.WriteLine($"[Session] OnTrackStartedAsync called, trackId={trackId}");

        if (trackId == 0) return;

        if (_currentSession != null)
        {
            using var closeScope = _scopeFactory.CreateScope();
            var closeUow = closeScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await EndCurrentSessionAsync(closeUow, completed: false);
        }

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        _currentSession = new ListeningSession
        {
            TrackId = trackId,
            StartedAt = DateTime.UtcNow,
        };

        await uow.ListeningSessions.AddAsync(_currentSession);
        await uow.SaveChangesAsync();
        Console.WriteLine($"[Session] Saved, Id={_currentSession.Id}");
    }

    public async Task OnTrackEndedAsync(bool completed)
    {
        Debug.WriteLine($"[Session] OnTrackEndedAsync called, completed={completed}");
        if (_currentSession == null)
        {
            Debug.WriteLine("[Session] _currentSession is null, skipping");
            return;
        }
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await EndCurrentSessionAsync(uow, completed);
    }

    private async Task EndCurrentSessionAsync(IUnitOfWork uow, bool completed)
    {
        if (_currentSession == null) return;

        var session = await uow.ListeningSessions.GetByIdAsync(_currentSession.Id);
        if (session == null) return;

        session.EndedAt = DateTime.UtcNow;
        session.ListenedSeconds = (int)(DateTime.UtcNow - session.StartedAt).TotalSeconds;
        session.CompletedFully = completed;

        await uow.ListeningSessions.UpdateAsync(session);

        var track = await uow.Tracks.GetByIdAsync(session.TrackId);
        if (track != null)
        {
            track.PlayCount += 1;
            track.TotalListenedSeconds += session.ListenedSeconds;
            track.LastPlayedAt = DateTime.UtcNow;
            await uow.Tracks.UpdateAsync(track);
        }

        await uow.SaveChangesAsync();
        _currentSession = null;
    }
}