using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Application.Interfaces;

public interface IPlaybackManager
{
    event EventHandler Started;
    event EventHandler Paused;
    event EventHandler<Track> TrackChanged;

    void Play();
    void Pause();
    void Resume();
    void PlayOrResume();
    void Stop();
    void Next();
    void Previous();
}