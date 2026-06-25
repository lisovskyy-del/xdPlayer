using Microsoft.EntityFrameworkCore;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Infrastructure.Data;

namespace xdPlayer.Infrastructure.Repositories;

public class TrackRepository : Repository<Track>, ITrackRepository
{
    public TrackRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Track>> SearchAsync(string query) =>
        await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Title.ToLower().Contains(query.ToLower()) ||
                        (t.Artist != null && t.Artist.ToLower().Contains(query.ToLower())) ||
                        (t.Album != null && t.Album.ToLower().Contains(query.ToLower())))
            .ToListAsync();

    public async Task<IEnumerable<Track>> GetLikedAsync() =>
        await _context.Tracks
            .AsNoTracking()
            .Where(t => t.IsLiked)
            .ToListAsync();

    public async Task<IEnumerable<Track>> GetByGenreAsync(string genre) =>
        await _context.Tracks
            .AsNoTracking()
            .Where(t => t.Genre == genre)
            .ToListAsync();

    public async Task<IEnumerable<Track>> GetPagedAsync(int page, int pageSize) =>
        await _context.Tracks
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetTotalCountAsync() =>
        await _context.Tracks.CountAsync();
}