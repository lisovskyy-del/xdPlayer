using System;
using DiscordRPC;
using RPCConsoleLogger = DiscordRPC.Logging.ConsoleLogger;
using RPCLogLevel = DiscordRPC.Logging.LogLevel;
using xdPlayer.Application.Interfaces;

namespace xdPlayer.Infrastructure.Services;

public class DiscordPresenceService : IDiscordPresenceService, IDisposable
{
    private DiscordRpcClient? _client;
    private const string CLIENT_ID = "1549376393186381875";
    private const string DEFAULT_LOGO_KEY = "logo";

    private string? _lastPresenceKey;

    public void Initialize()
    {
        if (_client != null && !_client.IsDisposed) return;

        _client = new DiscordRpcClient(CLIENT_ID)
        {
            Logger = new RPCConsoleLogger { Level = RPCLogLevel.Warning }
        };

        _client.Initialize();
    }

    public void UpdatePresence(string title, string artist, string? albumName, string? musicBrainzReleaseMbid, bool isPlaying, TimeSpan currentPosition, TimeSpan totalDuration)
    {
        if (_client == null || _client.IsDisposed) return;

        if (!isPlaying)
        {
            SetPaused(title, artist, albumName, musicBrainzReleaseMbid);
            return;
        }

        string presenceKey = $"{title}_{artist}_{musicBrainzReleaseMbid}_PLAYING";
        if (_lastPresenceKey == presenceKey) return;
        _lastPresenceKey = presenceKey;

        Timestamps? timestamps = null;
        if (totalDuration > TimeSpan.Zero)
        {
            var startUtc = DateTime.UtcNow.Subtract(currentPosition);
            var endUtc = startUtc.Add(totalDuration);

            timestamps = new Timestamps
            {
                Start = startUtc,
                End = endUtc
            };
        }

        string largeImage = GetCoverUrl(musicBrainzReleaseMbid);

        _client.SetPresence(new RichPresence
        {
            Details = string.IsNullOrWhiteSpace(title) ? "Listening to music" : title,
            State = string.IsNullOrWhiteSpace(artist) ? "Unknown Artist" : artist,
            Timestamps = timestamps,
            Type = ActivityType.Listening,
            Assets = new Assets
            {
                LargeImageKey = largeImage,
                LargeImageText = string.IsNullOrWhiteSpace(albumName) ? "xdPlayer" : albumName,
                SmallImageKey = "play_icon",
                SmallImageText = "Playing"
            }
        });
    }

    public void SetPaused(string title, string artist, string? albumName, string? musicBrainzReleaseMbid)
    {
        if (_client == null || _client.IsDisposed) return;

        string presenceKey = $"{title}_{artist}_PAUSED";
        if (_lastPresenceKey == presenceKey) return;
        _lastPresenceKey = presenceKey;

        _client.ClearPresence();
    }

    public void ResetCache()
    {
        _lastPresenceKey = null;
    }

    private string GetCoverUrl(string? releaseMbid)
    {
        if (!string.IsNullOrWhiteSpace(releaseMbid) && Guid.TryParse(releaseMbid, out _))
        {
            return $"https://coverartarchive.org/release/{releaseMbid}/front-250";
        }

        return DEFAULT_LOGO_KEY;
    }

    public void ClearPresence()
    {
        _lastPresenceKey = null;
        _client?.ClearPresence();
    }

    public void Shutdown()
    {
        _client?.Dispose();
        _client = null;
    }

    public void Dispose() => Shutdown();
}