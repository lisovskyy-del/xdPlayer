using System;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using xdPlayer.Application.Interfaces;
using xdPlayer.Application.Services;
using xdPlayer.Domain.Entities;

namespace xdPlayer.Presentation.ViewModels;

// middle point between Playback logic and Avalonia UI
public class PlayerViewModel : ReactiveObject, IDisposable
{
    private readonly IAudioPlayerService _audio;
    private readonly IPlaybackManager _playbackManager;

    [Reactive] public bool IsPlaying { get; set; }
    [Reactive] public string? CurrentTrackTitle { get; set; }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PauseCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> StopCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousCommand { get; private set; } = null!;

    private void OnStarted(object? sender, EventArgs e) => IsPlaying = true;
    private void OnPaused(object? sender, EventArgs e) => IsPlaying = false;

    private void OnTrackChanged(object? sender, Track track) =>
        CurrentTrackTitle = track.Title;

    public PlayerViewModel(IPlaybackManager playbackManager, IAudioPlayerService audio)
    {
        _audio = audio;
        _playbackManager = playbackManager;

        InitCommands();

        if (Avalonia.Controls.Design.IsDesignMode)
        {
            return;
        }

        InitRuntime();
    }

    private void InitCommands()
    {
        PlayCommand = ReactiveCommand.Create(() => _playbackManager.Play());
        PauseCommand = ReactiveCommand.Create(() => _playbackManager.Pause());
        StopCommand = ReactiveCommand.Create(() => _playbackManager.Stop());
        NextCommand = ReactiveCommand.Create(() => _playbackManager.Next());
        PreviousCommand = ReactiveCommand.Create(() => _playbackManager.Previous());
    }

    private void InitRuntime()
    {
        _playbackManager.Started += OnStarted;
        _playbackManager.Paused += OnPaused;
        _playbackManager.TrackChanged += OnTrackChanged;
    }

    public void Dispose()
    {
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            return;
        }

        _playbackManager.Started -= OnStarted;
        _playbackManager.Paused -= OnPaused;
        _playbackManager.TrackChanged -= OnTrackChanged;
    }
}