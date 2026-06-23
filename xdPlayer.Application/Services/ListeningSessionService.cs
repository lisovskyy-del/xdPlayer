using Microsoft.Extensions.DependencyInjection;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Application.Services;

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

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if (_currentSession != null)
            await EndCurrentSessionAsync(uow, completed: false);

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
        if (_currentSession == null) return;

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await EndCurrentSessionAsync(uow, completed);
    }

    private async Task EndCurrentSessionAsync(IUnitOfWork uow, bool completed)
    {
        if (_currentSession == null) return;

        _currentSession.EndedAt = DateTime.UtcNow;
        _currentSession.ListenedSeconds = (int)(DateTime.UtcNow - _currentSession.StartedAt).TotalSeconds;
        _currentSession.CompletedFully = completed;

        _currentSession = null;
        await uow.SaveChangesAsync();
    }
}