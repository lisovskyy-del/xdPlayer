using System;
using System.Threading.Tasks;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Application.Interfaces;

public interface IPlaybackManager
{
    event EventHandler Started;
    event EventHandler Paused;
    event EventHandler? Finished;
    event EventHandler<Track> TrackChanged;

    float Volume { get; set; }
    TimeSpan CurrentPosition { get; }
    TimeSpan TotalDuration { get; }

    Task PlayAsync();
    void Pause();
    void Resume();
    Task PlayOrResumeAsync();
    void Stop();
    Task NextAsync();
    Task PreviousAsync();
    void Seek(TimeSpan position);
}