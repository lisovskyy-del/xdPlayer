using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using xdPlayer.Application.Helpers;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Application.Models;

public class LibraryService : ILibraryService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMetadataReader _metadata;
    private readonly IMetadataEnrichmentService _enrichmentService;

    public LibraryService(IServiceScopeFactory scopeFactory, IMetadataReader metadata, IMetadataEnrichmentService enrichmentService)
    {
        _scopeFactory = scopeFactory;
        _metadata = metadata;
        _enrichmentService = enrichmentService;
    }

    public async Task<Track?> GetByIdAsync(int id)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await uow.Tracks.GetByIdAsync(id);
    }

    public async Task<Track> AddFileAsync(string filePath)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var all = await uow.Tracks.GetAllAsync();
        var existing = all.FirstOrDefault(t => t.FilePath == filePath);
        if (existing != null) return existing;

        var meta = _metadata.ReadMetadata(filePath);
        var track = _metadata.ReadMetadata(filePath);

        track.FilePath = filePath;
        track.AddedAt = DateTime.UtcNow;

        await uow.Tracks.AddAsync(track);
        await uow.SaveChangesAsync();

        var profiles = await uow.UserProfiles.GetAllAsync();
        var profile = profiles.FirstOrDefault();
        if (profile?.MusicBrainzEnabled == true &&
            (string.IsNullOrWhiteSpace(track.Genre) || string.IsNullOrWhiteSpace(track.MusicBrainzId)) || string.IsNullOrEmpty(track.CoverImagePath))
        {
            System.Diagnostics.Debug.WriteLine($"[Library] Enqueuing track {track.Id} for enrichment (MusicBrainzEnabled={profile.MusicBrainzEnabled})");
            _enrichmentService.EnqueueForEnrichment(track.Id);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[Library] Skipping enrichment for track {track.Id}: enabled={profile?.MusicBrainzEnabled}, genre='{track.Genre}', mbid='{track.MusicBrainzId}'");
        }

        return track;
    }

    public async Task<IEnumerable<Track>> AddFolderAsync(string folderPath)
    {
        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(f => AudioExtensions.Contains(Path.GetExtension(f).ToLower()));

        var added = new List<Track>();
        foreach (var file in files)
            added.Add(await AddFileAsync(file));
        return added;
    }

    public async Task<IEnumerable<Track>> GetAllAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await uow.Tracks.GetAllAsync();
    }

    public async Task<PagedResult<Track>> GetPagedAsync(int page, int pageSize)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var items = await uow.Tracks.GetPagedAsync(page, pageSize);
        var total = await uow.Tracks.GetTotalCountAsync();
        return new PagedResult<Track>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<Track> SetTrackCoverAsync(int trackId, string imageFilePath)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var track = await uow.Tracks.GetByIdAsync(trackId);
        if (track == null) throw new InvalidOperationException("Track not found");

        var oldCoverPath = track.CoverImagePath;

        Directory.CreateDirectory("Covers");

        var extension = Path.GetExtension(imageFilePath);
        var newCoverPath = Path.Combine("Covers", $"{Guid.NewGuid()}{extension}");

        File.Copy(imageFilePath, newCoverPath, overwrite: true);

        track.CoverImagePath = newCoverPath;

        await uow.Tracks.UpdateAsync(track);
        await uow.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(oldCoverPath) && oldCoverPath != newCoverPath && File.Exists(oldCoverPath))
        {
            try { File.Delete(oldCoverPath); }
            catch { }
        }

        return track;
    }

    public async Task<IEnumerable<Track>> SearchAsync(string query)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await uow.Tracks.SearchAsync(query);
    }

    public async Task<IEnumerable<Track>> FilterAsync(LibraryFilter filter)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var all = await uow.Tracks.GetAllAsync();
        return all.Where(t =>
            (filter.Artist == null || t.Artist == filter.Artist) &&
            (filter.Album == null || t.Album == filter.Album) &&
            (filter.Genre == null || t.Genre == filter.Genre) &&
            (filter.IsLiked == null || t.IsLiked == filter.IsLiked) &&
            (filter.MinDuration == null || t.DurationSeconds >= filter.MinDuration) &&
            (filter.MaxDuration == null || t.DurationSeconds <= filter.MaxDuration)
        );
    }

    public Task<IEnumerable<Track>> SortAsync(
        IEnumerable<Track> tracks, SortField field, bool ascending)
    {
        var sorted = field switch
        {
            SortField.Title => ascending ? tracks.OrderBy(t => t.Title) : tracks.OrderByDescending(t => t.Title),
            SortField.Artist => ascending ? tracks.OrderBy(t => t.Artist) : tracks.OrderByDescending(t => t.Artist),
            SortField.Album => ascending ? tracks.OrderBy(t => t.Album) : tracks.OrderByDescending(t => t.Album),
            SortField.Duration => ascending ? tracks.OrderBy(t => t.DurationSeconds) : tracks.OrderByDescending(t => t.DurationSeconds),
            SortField.DateAdded => ascending ? tracks.OrderBy(t => t.AddedAt) : tracks.OrderByDescending(t => t.AddedAt),
            _ => tracks
        };
        return Task.FromResult(sorted);
    }

    public async Task DeleteTrackAsync(int trackId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var track = await uow.Tracks.GetByIdAsync(trackId);
        if (track == null) return;

        var coverPath = track.CoverImagePath;

        await uow.Tracks.DeleteAsync(track);
        await uow.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(coverPath) && File.Exists(coverPath))
        {
            try { File.Delete(coverPath); }
            catch {  }
        }
    }

    private static readonly string[] AudioExtensions =
        [".mp3", ".flac", ".wav", ".ogg", ".m4a", ".aac"];

    public async Task<Track> ToggleLikeAsync(int trackId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var track = await uow.Tracks.GetByIdAsync(trackId);
        if (track == null) throw new InvalidOperationException("Track not found");

        track.IsLiked = !track.IsLiked;

        await uow.Tracks.UpdateAsync(track);
        await uow.SaveChangesAsync();

        return track;
    }

    public async Task<Track> UpdateTrackMetadataAsync(int trackId, string title, string? artist, string? album, string? genre, int? year, string? musicBrainzId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var track = await uow.Tracks.GetByIdAsync(trackId);
        if (track == null) throw new InvalidOperationException("Track not found");

        track.Title = title;
        track.Artist = artist;
        track.Album = album;
        track.Genre = genre;
        track.Year = year;
        track.MusicBrainzId = musicBrainzId;

        await uow.Tracks.UpdateAsync(track);
        await uow.SaveChangesAsync();

        try
        {
            _metadata.WriteMetadata(track.FilePath, title, artist, album, genre, year);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Library] Failed to write tags to file: {ex.Message}");
        }

        return track;
    }

    public async Task<Track> ResetMetadataFromFileAsync(int trackId)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var track = await uow.Tracks.GetByIdAsync(trackId);
        if (track == null) throw new InvalidOperationException("Track not found");

        var fresh = _metadata.ReadMetadata(track.FilePath);

        track.Title = fresh.Title;
        track.Artist = fresh.Artist;
        track.Album = fresh.Album;
        track.Genre = fresh.Genre;
        track.Year = fresh.Year;
        track.DurationSeconds = fresh.DurationSeconds;

        // Обкладинку оновлюємо лише якщо файл дійсно має нову — прибираємо стару, якщо замінюємо
        if (!string.IsNullOrWhiteSpace(fresh.CoverImagePath) && fresh.CoverImagePath != track.CoverImagePath)
        {
            var oldCover = track.CoverImagePath;
            track.CoverImagePath = fresh.CoverImagePath;

            if (!string.IsNullOrWhiteSpace(oldCover) && File.Exists(oldCover))
            {
                try { File.Delete(oldCover); } catch { }
            }
        }

        await uow.Tracks.UpdateAsync(track);
        await uow.SaveChangesAsync();

        return track;
    }
}