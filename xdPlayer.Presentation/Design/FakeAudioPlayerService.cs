using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Application.Interfaces;

namespace xdPlayer.Presentation.Design;

public class FakeAudioPlayerService : IAudioPlayerService
{
    public void Play(string data) { }
    public void Pause() { }
    public void Stop() { }

    public event EventHandler? PlaybackStarted;
    public event EventHandler? PlaybackPaused;
    public event EventHandler? PlaybackFinished;
}
