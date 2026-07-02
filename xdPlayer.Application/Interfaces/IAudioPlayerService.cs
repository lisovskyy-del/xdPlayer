using System;
using System.Threading.Tasks;

namespace xdPlayer.Application.Interfaces;

public interface IAudioPlayerService
{
    bool IsPaused { get; }

    Task PlayAsync(string filepath);
    void Pause();
    void Resume();
    void Stop();

    event EventHandler? PlaybackStarted;
    event EventHandler? PlaybackPaused;
    event EventHandler? PlaybackFinished;
}