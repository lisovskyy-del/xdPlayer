using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Threading;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Application.Models;

public class ListeningSessionService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private ListeningSession? _currentSession;

    public ListeningSessionService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task OnTrackStartedAsync(int trackId)
    {
        if (trackId == 0) return;

        await _lock.WaitAsync();
        try
        {
            if (_currentSession != null)
            {
                using var closeScope = _scopeFactory.CreateScope();
                var closeUow = closeScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await EndCurrentSessionInternalAsync(closeUow, completed: false);
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
            Debug.WriteLine($"[Session] Saved, Id={_currentSession.Id}");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task OnTrackEndedAsync(bool completed)
    {
        await _lock.WaitAsync();
        try
        {
            if (_currentSession == null)
            {
                Debug.WriteLine("[Session] _currentSession is null, skipping");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await EndCurrentSessionInternalAsync(uow, completed);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task EndCurrentSessionInternalAsync(IUnitOfWork uow, bool completed)
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