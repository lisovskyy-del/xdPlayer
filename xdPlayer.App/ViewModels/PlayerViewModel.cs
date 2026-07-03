using System;
using System.Reactive;
using ReactiveUI;
using xdPlayer.Application.Interfaces;
using xdPlayer.Application.Models;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.ViewModels;

public class PlayerViewModel : ReactiveObject, IDisposable
{
    private readonly IPlaybackManager _playbackManager;
    private readonly ListeningSessionService _sessionService;
    private readonly System.Timers.Timer? _progressTimer;

    private TimeSpan _currentPosition;
    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        set => this.RaiseAndSetIfChanged(ref _currentPosition, value);
    }

    private TimeSpan _totalDuration;
    public TimeSpan TotalDuration
    {
        get => _totalDuration;
        set => this.RaiseAndSetIfChanged(ref _totalDuration, value);
    }

    private double _volume = 100;
    public double Volume
    {
        get => _volume;
        set
        {
            this.RaiseAndSetIfChanged(ref _volume, value);
            if (_playbackManager != null)
                _playbackManager.Volume = (float)(value / 100.0);
        }
    }

    private string? _currentTrackArtist;
    public string? CurrentTrackArtist
    {
        get => _currentTrackArtist;
        set => this.RaiseAndSetIfChanged(ref _currentTrackArtist, value);
    }

    private string? _currentTrackCoverPath;
    public string? CurrentTrackCoverPath
    {
        get => _currentTrackCoverPath;
        set => this.RaiseAndSetIfChanged(ref _currentTrackCoverPath, value);
    }

    private bool _isLiked;
    public bool IsLiked
    {
        get => _isLiked;
        set => this.RaiseAndSetIfChanged(ref _isLiked, value);
    }

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

        PlayCommand = ReactiveCommand.CreateFromTask(() => _playbackManager.PlayOrResumeAsync());
        PauseCommand = ReactiveCommand.Create(() => _playbackManager.Pause());
        StopCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            _playbackManager.Stop();
            await _sessionService.OnTrackEndedAsync(completed: false);
        });
        NextCommand = ReactiveCommand.CreateFromTask(() => _playbackManager.NextAsync());
        PreviousCommand = ReactiveCommand.CreateFromTask(() => _playbackManager.PreviousAsync());

        _playbackManager.Started += OnStarted;
        _playbackManager.Paused += OnPaused;

        _playbackManager.Finished += async (_, _) =>
        {
            await _sessionService.OnTrackEndedAsync(completed: true);
        };

        _playbackManager.TrackChanged += OnTrackChanged;

        _progressTimer = new System.Timers.Timer(500);
        _progressTimer.Elapsed += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                CurrentPosition = _playbackManager.CurrentPosition;
                TotalDuration = _playbackManager.TotalDuration;
            });
        };
        _progressTimer.Start();
    }

    private void OnStarted(object? sender, EventArgs e) =>
     Avalonia.Threading.Dispatcher.UIThread.Post(() => IsPlaying = true);

    private void OnPaused(object? sender, EventArgs e) =>
        Avalonia.Threading.Dispatcher.UIThread.Post(() => IsPlaying = false);

    private void OnTrackChanged(object? sender, Track track) =>
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            System.Diagnostics.Debug.WriteLine($"[Track] Changed to: {track.Title}, Id={track.Id}");
            CurrentTrackTitle = track.Title;
            CurrentTrackArtist = track.Artist;
            CurrentTrackCoverPath = track.CoverImagePath;
            IsLiked = track.IsLiked;
            try
            {
                await _sessionService.OnTrackStartedAsync(track.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Track] Exception: {ex.Message}");
            }
        });

    public void Dispose()
    {
        if (Avalonia.Controls.Design.IsDesignMode) return;

        _playbackManager.Started -= OnStarted;
        _playbackManager.Paused -= OnPaused;
        _playbackManager.TrackChanged -= OnTrackChanged;
        _progressTimer?.Stop();
        _progressTimer?.Dispose();
    }
}