using Avalonia.Media;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using xdPlayer.Application.Interfaces;
using xdPlayer.Application.Models;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Playback;

namespace xdPlayer.App.ViewModels;

public class PlayerViewModel : ReactiveObject, IDisposable
{
    private readonly IPlaybackManager _playbackManager;
    private readonly ListeningSessionService _sessionService;
    private readonly ILibraryService _libraryService;
    private readonly System.Timers.Timer? _progressTimer;
    private static readonly IBrush AccentBrushColor = new SolidColorBrush(Color.Parse("White"));
    private static readonly IBrush MutedBrushColor = new SolidColorBrush(Color.Parse("#606060"));
    private RepeatMode _repeatMode = RepeatMode.None;

    private int _currentTrackId;

    private bool _hasTrack;
    public bool HasTrack
    {
        get => _hasTrack;
        set => this.RaiseAndSetIfChanged(ref _hasTrack, value);
    }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    private bool _isSeeking;
    public bool IsSeeking
    {
        get => _isSeeking;
        set => this.RaiseAndSetIfChanged(ref _isSeeking, value);
    }

    private string? _currentTrackTitle;
    public string? CurrentTrackTitle
    {
        get => _currentTrackTitle;
        set => this.RaiseAndSetIfChanged(ref _currentTrackTitle, value);
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

    public IBrush ShuffleIconBrush => IsShuffleEnabled ? AccentBrushColor : MutedBrushColor;

    private bool _isShuffleEnabled;
    public bool IsShuffleEnabled
    {
        get => _isShuffleEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _isShuffleEnabled, value);
            this.RaisePropertyChanged(nameof(ShuffleIconBrush));
        }
    }

    public IBrush RepeatIconBrush => RepeatMode != RepeatMode.None ? AccentBrushColor : MutedBrushColor;
    public bool IsRepeatOne => RepeatMode == RepeatMode.One;

    public RepeatMode RepeatMode
    {
        get => _repeatMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _repeatMode, value);
            this.RaisePropertyChanged(nameof(RepeatIconBrush));
            this.RaisePropertyChanged(nameof(IsRepeatOne));
        }
    }

    public ReactiveCommand<Unit, Unit> PlayCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> NextCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PreviousCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleLikeCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ShuffleCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RepeatCommand { get; private set; }

    // for Avalonia Previewer
    public PlayerViewModel()
    {
        _playbackManager = null!;
        _libraryService = null!;
        CurrentTrackTitle = "Design Track";
        CurrentTrackArtist = "Design Artist";
        IsPlaying = false;

        PlayCommand = ReactiveCommand.Create(() => { });
        PauseCommand = ReactiveCommand.Create(() => { });
        StopCommand = ReactiveCommand.Create(() => { });
        NextCommand = ReactiveCommand.Create(() => { });
        PreviousCommand = ReactiveCommand.Create(() => { });
        ToggleLikeCommand = ReactiveCommand.Create(() => { });
        ShuffleCommand = ReactiveCommand.Create(() => { });
        RepeatCommand = ReactiveCommand.Create(() => { });
    }

    public PlayerViewModel(IPlaybackManager playbackManager, ListeningSessionService sessionService, ILibraryService libraryService)
    {
        _playbackManager = playbackManager;
        _sessionService = sessionService;
        _libraryService = libraryService;

        PlayCommand = ReactiveCommand.CreateFromTask(() => _playbackManager.PlayOrResumeAsync());
        PauseCommand = ReactiveCommand.Create(() => _playbackManager.Pause());
        StopCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            _playbackManager.Stop();
            await _sessionService.OnTrackEndedAsync(completed: false);
        });
        NextCommand = ReactiveCommand.CreateFromTask(() => _playbackManager.NextAsync());
        PreviousCommand = ReactiveCommand.CreateFromTask(() => _playbackManager.PreviousAsync());
        ToggleLikeCommand = ReactiveCommand.CreateFromTask(ToggleLikeAsync);

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
            if (IsSeeking) return;

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                CurrentPosition = _playbackManager.CurrentPosition;
                TotalDuration = _playbackManager.TotalDuration;
            });
        };
        _progressTimer.Start();

        ShuffleCommand = ReactiveCommand.Create(() =>
        {
            _playbackManager.ToggleShuffle();
            IsShuffleEnabled = _playbackManager.PlaybackMode == PlaybackMode.Shuffle;
        });

        RepeatCommand = ReactiveCommand.Create(() =>
        {
            var next = RepeatMode switch
            {
                RepeatMode.None => RepeatMode.All,
                RepeatMode.All => RepeatMode.One,
                RepeatMode.One => RepeatMode.None,
                _ => RepeatMode.None
            };
            _playbackManager.RepeatMode = next;
            RepeatMode = next;
        });
    }

    private void OnStarted(object? sender, EventArgs e) =>
     Avalonia.Threading.Dispatcher.UIThread.Post(() => IsPlaying = true);

    private void OnPaused(object? sender, EventArgs e) =>
        Avalonia.Threading.Dispatcher.UIThread.Post(() => IsPlaying = false);

    private void OnTrackChanged(object? sender, Track track) =>
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            System.Diagnostics.Debug.WriteLine($"[Track] Changed to: {track.Title}, Id={track.Id}");
            _currentTrackId = track.Id;
            HasTrack = true;
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

    private async Task ToggleLikeAsync()
    {
        if (_currentTrackId == 0) return;

        var updated = await _libraryService.ToggleLikeAsync(_currentTrackId);
        IsLiked = updated.IsLiked;
    }

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