using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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

    public LibraryService(IServiceScopeFactory scopeFactory, IMetadataReader metadata)
    {
        _scopeFactory = scopeFactory;
        _metadata = metadata;
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

    private static readonly string[] AudioExtensions =
        [".mp3", ".flac", ".wav", ".ogg", ".m4a", ".aac"];
}