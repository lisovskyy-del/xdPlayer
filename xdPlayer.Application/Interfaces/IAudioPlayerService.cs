using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Application.Interfaces;

public interface IAudioPlayerService
{
    void Play(string filepath);
    void Pause();
    void Stop();

    event EventHandler? PlaybackFinished;
}