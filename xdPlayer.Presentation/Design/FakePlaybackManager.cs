using System;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Presentation.Design;

public class FakePlaybackManager : IPlaybackManager
{
    public event EventHandler? Started;
    public event EventHandler? Paused;
    public event EventHandler<Track>? TrackChanged;

    public void Play()
    {
        Started?.Invoke(this, EventArgs.Empty);
        TrackChanged?.Invoke(this, new Track { Title = "Preview Track" });
    }

    public void Pause()
    {
        Paused?.Invoke(this, EventArgs.Empty);
    }

    public void Stop() { }
    public void Next() { }
    public void Previous() { }
}