using Microsoft.EntityFrameworkCore;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Infrastructure.Data;

namespace xdPlayer.Infrastructure.Repositories;

public class ListeningSessionRepository : IListeningSessionRepository
{
    private readonly AppDbContext _db;

    public ListeningSessionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ListeningSession?> GetActiveSessionAsync(int trackId)
        => await _db.ListeningSessions
            .Where(s => s.TrackId == trackId && s.EndedAt == null)
            .FirstOrDefaultAsync();

    public async Task AddAsync(ListeningSession session)
        => await _db.ListeningSessions.AddAsync(session);

    public async Task SaveAsync()
        => await _db.SaveChangesAsync();

    public async Task<ListeningSession?> GetByIdAsync(long id)
    => await _db.ListeningSessions.FindAsync(id);
}