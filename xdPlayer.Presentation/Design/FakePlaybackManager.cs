using System;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Presentation.Design;

public class FakePlaybackManager : IPlaybackManager
{
    public event EventHandler? Started { add { } remove { } }
    public event EventHandler? Paused { add { } remove { } }
    public event EventHandler<Track>? TrackChanged { add { } remove { } }

    public void Play() { }
    public void Pause() { }
    public void Stop() { }
    public void Next() { }
    public void Previous() { }
}