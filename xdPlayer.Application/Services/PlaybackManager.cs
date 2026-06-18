using System;
using System.Collections.Generic;
using System.Text;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Playback;

namespace xdPlayer.Application.Services;

public class PlaybackManager // the main class that is responsible for playing audios
{
    private readonly IAudioPlayerService _player; // audio player itself

    public PlaybackQueue Queue { get; } // the queue, which can have parameters

    private void OnPlaybackFinished(object? sender, EventArgs e) // simple event that activates when track is finished
    {
        var nextTrack = Queue.Next();

        if (nextTrack != null)
        {
            _player.Play(nextTrack.FilePath);
        }
    }

    public PlaybackManager(IAudioPlayerService player, PlaybackQueue queue) // init the objects
    {
        _player = player;
        Queue = queue;

        _player.PlaybackFinished += OnPlaybackFinished;
    }
    
    public void Play() // fetches the current index of the track and plays it 
    {
        var track = Queue.CurrentTrack;

        if (track == null)
        {
            return;
        }

        _player.Play(track.FilePath);
    }

    public void Pause()
    {
        _player.Pause();
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
        }
    }

    public void Previous()
    {
        var track = Queue.Previous();

        if (track != null)
        {
            _player.Play(track.FilePath);
        }
    }
}