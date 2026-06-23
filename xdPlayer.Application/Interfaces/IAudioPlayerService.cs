using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Application.Interfaces;

public interface IAudioPlayerService
{
    bool IsPaused { get; }

    void Play(string filepath);
    void Pause();
    void Resume();
    void Stop();

    event EventHandler? PlaybackStarted;
    event EventHandler? PlaybackPaused;
    event EventHandler? PlaybackFinished;
}