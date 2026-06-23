using xdPlayer.Domain.Interfaces;
using xdPlayer.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public ITrackRepository Tracks { get; }
    public IPlaylistRepository Playlists { get; }
    public ITagRepository Tags { get; }
    public IListeningSessionRepository ListeningSessions { get; }

    public UnitOfWork(
        AppDbContext db,
        ITrackRepository tracks,
        IPlaylistRepository playlists,
        ITagRepository tags,
        IListeningSessionRepository listeningSessions)
    {
        _db = db;
        Tracks = tracks;
        Playlists = playlists;
        Tags = tags;
        ListeningSessions = listeningSessions;
    }

    public async Task<int> SaveChangesAsync()
        => await _db.SaveChangesAsync();
}