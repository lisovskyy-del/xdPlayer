using System;

namespace xdPlayer.Application.Interfaces;

public interface IDiscordPresenceService
{
    void Initialize();
    void UpdatePresence(string title, string artist, string? albumName, string? musicBrainzReleaseMbid, bool isPlaying, TimeSpan currentPosition, TimeSpan totalDuration);
    void SetPaused(string title, string artist, string? albumName, string? musicBrainzReleaseMbid);
    void ResetCache();
    void ClearPresence();
    void Shutdown();
}