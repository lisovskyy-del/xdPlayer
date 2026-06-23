using System;
using System.Reactive;
using ReactiveUI;
using xdPlayer.Application.Interfaces;
using xdPlayer.Application.Services;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.ViewModels;

public class PlayerViewModel : ReactiveObject, IDisposable
{
    private readonly IPlaybackManager _playbackManager;
    private readonly ListeningSessionService _sessionService;

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    private string? _currentTrackTitle;
    public string? CurrentTrackTitle
    {
        get => _currentTrackTitle;
        set => this.RaiseAndSetIfChanged(ref _currentTrackTitle, value);
    }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> NextCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PreviousCommand { get; private set; }

    // for Avalonia Previewer
    public PlayerViewModel()
    {
        _playbackManager = null!;
        CurrentTrackTitle = "Design Track";
        IsPlaying = false;

        PlayCommand = ReactiveCommand.Create(() => { });
        PauseCommand = ReactiveCommand.Create(() => { });
        StopCommand = ReactiveCommand.Create(() => { });
        NextCommand = ReactiveCommand.Create(() => { });
        PreviousCommand = ReactiveCommand.Create(() => { });
    }

    public PlayerViewModel(IPlaybackManager playbackManager, ListeningSessionService sessionService)
    {
        _playbackManager = playbackManager;
        _sessionService = sessionService;

        PlayCommand = ReactiveCommand.Create(() => _playbackManager.PlayOrResume());
        PauseCommand = ReactiveCommand.Create(() => _playbackManager.Pause());
        StopCommand = ReactiveCommand.Create(() => _playbackManager.Stop());
        NextCommand = ReactiveCommand.Create(() => _playbackManager.Next());
        PreviousCommand = ReactiveCommand.Create(() => _playbackManager.Previous());

        _playbackManager.Started += OnStarted;
        _playbackManager.Paused += OnPaused;
        _playbackManager.TrackChanged += OnTrackChanged;
    }

    private void OnStarted(object? sender, EventArgs e) =>
     Avalonia.Threading.Dispatcher.UIThread.Post(() => IsPlaying = true);

    private void OnPaused(object? sender, EventArgs e) =>
        Avalonia.Threading.Dispatcher.UIThread.Post(() => IsPlaying = false);

    private void OnTrackChanged(object? sender, Track track) =>
     Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
     {
         CurrentTrackTitle = track.Title;
         await _sessionService.OnTrackStartedAsync(track.Id);
     });

    public void Dispose()
    {
        if (Avalonia.Controls.Design.IsDesignMode) return;

        _playbackManager.Started -= OnStarted;
        _playbackManager.Paused -= OnPaused;
        _playbackManager.TrackChanged -= OnTrackChanged;
    }
}