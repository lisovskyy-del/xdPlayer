using Microsoft.EntityFrameworkCore;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;
using xdPlayer.Infrastructure.Data;

namespace xdPlayer.Infrastructure.Repositories;

public class PlaylistRepository : Repository<Playlist>, IPlaylistRepository
{
    public PlaylistRepository(AppDbContext context) : base(context) { }

    public async Task<Playlist?> GetWithTracksAsync(int id) =>
        await _context.Playlists
            .Include(p => p.PlaylistTracks)
            .ThenInclude(pt => pt.Track)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task DeleteWithTracksAsync(int playlistId)
    {
        System.Diagnostics.Debug.WriteLine($"[Playlist] DeleteWithTracksAsync called, playlistId={playlistId}");

        var playlist = await _context.Playlists
            .Include(p => p.PlaylistTracks)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        System.Diagnostics.Debug.WriteLine($"[Playlist] Playlist found: {playlist?.Name}, tracks: {playlist?.PlaylistTracks.Count}");

        if (playlist == null) return;

        _context.Set<PlaylistTrack>().RemoveRange(playlist.PlaylistTracks);
        await _context.SaveChangesAsync();
        System.Diagnostics.Debug.WriteLine("[Playlist] PlaylistTracks removed");

        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync();
        System.Diagnostics.Debug.WriteLine("[Playlist] Playlist removed");
    }
}