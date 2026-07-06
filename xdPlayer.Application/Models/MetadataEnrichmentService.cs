using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Application.Models;

public class MetadataEnrichmentService : IMetadataEnrichmentService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentQueue<int> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _worker;

    public event EventHandler<int>? TrackEnriched;

    public MetadataEnrichmentService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _worker = Task.Run(ProcessQueueAsync);
    }

    public void EnqueueForEnrichment(int trackId)
    {
        _queue.Enqueue(trackId);
        _signal.Release();
    }

    private async Task ProcessQueueAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await _signal.WaitAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!_queue.TryDequeue(out var trackId))
                continue;

            try
            {
                await EnrichTrackAsync(trackId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Enrichment] Failed for track {trackId}: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(1.1), _cts.Token).ContinueWith(_ => { });
        }
    }

    private async Task EnrichTrackAsync(int trackId)
    {
        System.Diagnostics.Debug.WriteLine($"[Enrichment] Starting for track {trackId}");

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var musicBrainz = scope.ServiceProvider.GetRequiredService<IMusicBrainzClient>();

        var track = await uow.Tracks.GetByIdAsync(trackId);
        if (track == null)
        {
            System.Diagnostics.Debug.WriteLine($"[Enrichment] Track {trackId} not found");
            return;
        }

        if (!string.IsNullOrWhiteSpace(track.Genre) &&
            !string.IsNullOrWhiteSpace(track.MusicBrainzId) &&
            !string.IsNullOrWhiteSpace(track.CoverImagePath))
        {
            System.Diagnostics.Debug.WriteLine($"[Enrichment] Track {trackId} already has all data, skipping");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[Enrichment] Searching MusicBrainz for '{track.Title}' by '{track.Artist}'");

        var result = await musicBrainz.SearchRecordingAsync(track.Title, track.Artist);

        if (result == null)
        {
            System.Diagnostics.Debug.WriteLine($"[Enrichment] No MusicBrainz result found for track {trackId}");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[Enrichment] Found: mbid={result.MusicBrainzId}, genre={result.Genre}, album={result.Album}");

        var changed = false;

        if (string.IsNullOrWhiteSpace(track.MusicBrainzId) && result.MusicBrainzId != null)
        {
            track.MusicBrainzId = result.MusicBrainzId;
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(track.Genre) && result.Genre != null)
        {
            track.Genre = result.Genre;
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(track.Album) && result.Album != null)
        {
            track.Album = result.Album;
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(track.CoverImagePath) && result.ReleaseMbid != null)
        {
            await Task.Delay(TimeSpan.FromSeconds(1.1));

            var coverUrl = await musicBrainz.GetCoverArtUrlAsync(result.ReleaseMbid);
            if (coverUrl != null)
            {
                var savedPath = await DownloadCoverAsync(coverUrl);
                if (savedPath != null)
                {
                    track.CoverImagePath = savedPath;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            await uow.Tracks.UpdateAsync(track);
            await uow.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"[Enrichment] Track {trackId} enriched: genre={track.Genre}, album={track.Album}, cover={track.CoverImagePath}");

            TrackEnriched?.Invoke(this, trackId);
        }
    }

    private static readonly HttpClient _downloadClient = new();

    private async Task<string?> DownloadCoverAsync(string url)
    {
        try
        {
            var bytes = await _downloadClient.GetByteArrayAsync(url);

            var coversDir = Path.Combine(AppContext.BaseDirectory, "Covers");
            Directory.CreateDirectory(coversDir);

            var path = Path.Combine(coversDir, $"{Guid.NewGuid()}.jpg");
            await File.WriteAllBytesAsync(path, bytes);

            return path;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Enrichment] DownloadCoverAsync exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _signal.Release();
        try { _worker.Wait(TimeSpan.FromSeconds(2)); } catch { }
        _cts.Dispose();
        _signal.Dispose();
    }
}