using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Playback;

namespace xdPlayer.Application.Services;

public class PlaybackManager : IPlaybackManager // the main class that is responsible for playing audios
{
    private readonly IAudioPlayerService _player; // audio player itself

    public PlaybackQueue Queue { get; } // the queue, which can have parameters

    public event EventHandler? Started;
    public event EventHandler? Paused;
    public event EventHandler<Track>? TrackChanged;

    // init the objects
    public PlaybackManager(IAudioPlayerService player, PlaybackQueue queue)
    {
        _player = player;
        Queue = queue;

        _player.PlaybackFinished += OnPlaybackFinished;
        _player.PlaybackStarted += (s, e) => Started?.Invoke(this, EventArgs.Empty);
        _player.PlaybackPaused += (s, e) => Paused?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlaybackFinished(object? sender, EventArgs e) // simple event that activates when track is finished
    {
        var nextTrack = Queue.Next();

        if (nextTrack != null)
        {
            _player.Play(nextTrack.FilePath);
            TrackChanged?.Invoke(this, nextTrack);
        }
    }
    
    public void Play() // fetches the current index of the track and plays it 
    {
        var track = Queue.CurrentTrack;

        if (track == null)
        {
            return;
        }

        _player.Play(track.FilePath);
        TrackChanged?.Invoke(this, track);
    }

    public void Pause()
    {
        _player.Pause();
    }

    public void Resume()
    {
        _player.Resume();
    }

    public void PlayOrResume()
    {
        if (_player.IsPaused)
            _player.Resume();
        else
            Play();
    }

    public void Stop()
    {
        _player.Stop();
    }

    public void Next()
    {
        var track = Queue.Next();

        if (track != null)
        {
            _player.Play(track.FilePath);
            TrackChanged?.Invoke(this, track);
        }
    }

    public void Previous()
    {
        var track = Queue.Previous();

        if (track != null)
        {
            _player.Play(track.FilePath);
            TrackChanged?.Invoke(this, track);
        }
    }
}