using System;
using System.Threading.Tasks;

namespace xdPlayer.Application.Interfaces;

public interface IAudioPlayerService
{
    bool IsPaused { get; }
    TimeSpan CurrentPosition { get; }
    TimeSpan TotalDuration { get; }
    float Volume { get; set; }

    Task PlayAsync(string filepath);
    void Pause();
    void Resume();
    void Stop();
    void Seek(TimeSpan position);

    event EventHandler? PlaybackStarted;
    event EventHandler? PlaybackPaused;
    event EventHandler? PlaybackFinished;
}