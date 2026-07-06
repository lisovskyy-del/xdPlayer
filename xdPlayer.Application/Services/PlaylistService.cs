using Microsoft.Extensions.DependencyInjection;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Application.Services;

public class PlaylistService : IPlaylistService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PlaylistService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<IEnumerable<Playlist>> GetAllAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await uow.Playlists.GetAllAsync();
    }

    public async Task<Playlist?> GetWithTracksAsync(int id)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await uow.Playlists.GetWithTracksAsync(id);
    }

    public async Task<Playlist> CreateAsync(string name, string? description = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var playlist = new Playlist
        {
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await uow.Playlists.AddAsync(playlist);
        await uow.SaveChangesAsync();
        return playlist;
    }

    public async Task<Playlist> SetCoverAsync(int playlistId, string imageFilePath)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var playlist = await uow.Playlists.GetByIdAsync(playlistId);
        if (playlist == null) throw new InvalidOperationException("Playlist not found");

        var oldCoverPath = playlist.CoverImagePath;

        var coversDir = Path.Combine(AppContext.BaseDirectory, "PlaylistCovers");
        Directory.CreateDirectory(coversDir);

        var extension = Path.GetExtension(imageFilePath);
        var newCoverPath = Path.Combine(coversDir, $"{Guid.NewGuid()}{extension}");

        File.Copy(imageFilePath, newCoverPath, overwrite: true);

        playlist.CoverImagePath = newCoverPath;
        playlist.UpdatedAt = DateTime.UtcNow;

        await uow.Playlists.UpdateAsync(playlist);
        await uow.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(oldCoverPath) && oldCoverPath != newCoverPath && File.Exists(oldCoverPath))
        {
            try { File.Delete(oldCoverPath); }
            catch { }
        }

        return playlist;
    }

    public async Task RenameAsync(int playlistId, string newName)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var playlist = await uow.Playlists.GetByIdAsync(playlistId);
        if (playlist == null) return;

        playlist.Name = newName;
        playlist.UpdatedAt = DateTime.UtcNow;

        await uow.Playlists.UpdateAsync(playlist);
        await uow.SaveChangesAsync();
    }

    public async Task AddTrackAsync(int playlistId, int trackId)
    {
        System.Diagnostics.Debug.WriteLine($"[Playlist] AddTrackAsync called, playlistId={playlistId}, trackId={trackId}");

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var playlist = await uow.Playlists.GetWithTracksAsync(playlistId);
        System.Diagnostics.Debug.WriteLine($"[Playlist] Playlist found: {playlist?.Name}");

        if (playlist == null) return;

        if (playlist.PlaylistTracks.Any(pt => pt.TrackId == trackId))
        {
            System.Diagnostics.Debug.WriteLine("[Playlist] Track already exists");
            return;
        }

        var nextPosition = playlist.PlaylistTracks.Count > 0
            ? playlist.PlaylistTracks.Max(pt => pt.Position) + 1
            : 0;

        playlist.PlaylistTracks.Add(new PlaylistTrack
        {
            PlaylistId = playlistId,
            TrackId = trackId,
            Position = nextPosition,
        });

        playlist.UpdatedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync();
        System.Diagnostics.Debug.WriteLine("[Playlist] Track saved");
    }

    public async Task UpdateTrackPositionsAsync(int playlistId, IReadOnlyList<int> orderedTrackIds)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var playlist = await uow.Playlists.GetWithTracksAsync(playlistId);
        if (playlist == null) return;

        for (int i = 0; i < orderedTrackIds.Count; i++)
        {
            var trackId = orderedTrackIds[i];
            var playlistTrack = playlist.PlaylistTracks.FirstOrDefault(pt => pt.TrackId == trackId);
            if (playlistTrack != null)
                playlistTrack.Position = i;
        }

        playlist.UpdatedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync();
    }

    public async Task RemoveTrackAsync(int playlistId, int trackId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var playlist = await uow.Playlists.GetWithTracksAsync(playlistId);
        if (playlist == null) return;

        var playlistTrack = playlist.PlaylistTracks
            .FirstOrDefault(pt => pt.TrackId == trackId);

        if (playlistTrack == null) return;

        playlist.PlaylistTracks.Remove(playlistTrack);
        playlist.UpdatedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int playlistId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var playlist = await uow.Playlists.GetByIdAsync(playlistId);
        var coverPath = playlist?.CoverImagePath;

        await uow.Playlists.DeleteWithTracksAsync(playlistId);

        if (!string.IsNullOrWhiteSpace(coverPath) && File.Exists(coverPath))
        {
            try { File.Delete(coverPath); }
            catch { }
        }
    }

    public async Task<IEnumerable<Playlist>> GetAllWithTracksAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await uow.Playlists.GetAllWithTracksAsync();
    }
}