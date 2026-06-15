using xdPlayer.Domain.Interfaces;
using xdPlayer.Infrastructure.Data;

namespace xdPlayer.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ITrackRepository Tracks { get; }
    public IPlaylistRepository Playlists { get; }
    public ITagRepository Tags { get; }

    public UnitOfWork(AppDbContext context,
        ITrackRepository tracks,
        IPlaylistRepository playlists,
        ITagRepository tags)
    {
        _context = context;
        Tracks = tracks;
        Playlists = playlists;
        Tags = tags;
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public void Dispose() =>
        _context.Dispose();
}