using System;
using System.Threading.Tasks;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Playback;

namespace xdPlayer.Application.Models;

public class PlaybackManager : IPlaybackManager
{
    private readonly IAudioPlayerService _player;

    public PlaybackQueue Queue { get; }

    public event EventHandler? Started;
    public event EventHandler? Paused;
    public event EventHandler? Finished;
    public event EventHandler<Track>? TrackChanged;

    public float Volume
    {
        get => _player.Volume;
        set => _player.Volume = value;
    }

    public TimeSpan CurrentPosition => _player.CurrentPosition;
    public TimeSpan TotalDuration => _player.TotalDuration;

    public PlaybackManager(IAudioPlayerService player, PlaybackQueue queue)
    {
        _player = player;
        Queue = queue;

        _player.PlaybackFinished += OnPlaybackFinished;
        _player.PlaybackStarted += (s, e) => Started?.Invoke(this, EventArgs.Empty);
        _player.PlaybackPaused += (s, e) => Paused?.Invoke(this, EventArgs.Empty);
    }

    private async void OnPlaybackFinished(object? sender, EventArgs e)
    {
        Console.WriteLine("[PlaybackManager] OnPlaybackFinished called");

        Finished?.Invoke(this, EventArgs.Empty);

        var nextTrack = Queue.Next();
        if (nextTrack != null)
        {
            await _player.PlayAsync(nextTrack.FilePath);
            TrackChanged?.Invoke(this, nextTrack);
        }
    }

    public async Task PlayAsync()
    {
        var track = Queue.CurrentTrack;
        if (track == null) return;

        await _player.PlayAsync(track.FilePath);
        TrackChanged?.Invoke(this, track);
    }

    public void Pause() => _player.Pause();

    public void Resume() => _player.Resume();

    public async Task PlayOrResumeAsync()
    {
        if (_player.IsPaused)
            _player.Resume();
        else
            await PlayAsync();
    }

    public void Stop() => _player.Stop();

    public async Task NextAsync()
    {
        var track = Queue.Next();
        if (track != null)
        {
            await _player.PlayAsync(track.FilePath);
            TrackChanged?.Invoke(this, track);
        }
    }

    public async Task PreviousAsync()
    {
        var track = Queue.Previous();
        if (track != null)
        {
            await _player.PlayAsync(track.FilePath);
            TrackChanged?.Invoke(this, track);
        }
    }

    public void Seek(TimeSpan position) => _player.Seek(position);
}