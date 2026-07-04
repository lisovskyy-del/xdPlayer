using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Playback;

namespace xdPlayer.App.ViewModels;

public class PlaylistDisplayItem : ReactiveObject
{
    public Playlist Playlist { get; }

    public int Id => Playlist.Id;
    public string Name => Playlist.Name;
    public int TrackCount => Playlist.PlaylistTracks.Count;

    private string? _coverImagePath;
    public string? CoverImagePath
    {
        get => _coverImagePath;
        set => this.RaiseAndSetIfChanged(ref _coverImagePath, value);
    }

    public PlaylistDisplayItem(Playlist playlist)
    {
        Playlist = playlist;
        _coverImagePath = playlist.CoverImagePath;
    }
}

public class PlaylistViewModel : ReactiveObject
{
    private Playlist? _selectedPlaylist;
    private readonly IPlaylistService _playlistService;
    private readonly PlaybackQueue _queue;
    private readonly IPlaybackManager _playbackManager;
    private string _newPlaylistName = string.Empty;
    private string? _selectedPlaylistCoverPath;

    public ObservableCollection<Playlist> Playlists { get; } = [];
    public ObservableCollection<Track> CurrentTracks { get; } = [];

    public Playlist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPlaylist, value);
            SelectedPlaylistCoverPath = value?.CoverImagePath;
            if (value != null)
                _ = LoadTracksAsync(value.Id);
        }
    }

    public string? SelectedPlaylistCoverPath
    {
        get => _selectedPlaylistCoverPath;
        private set => this.RaiseAndSetIfChanged(ref _selectedPlaylistCoverPath, value);
    }

    public string TracksSummary
    {
        get
        {
            var count = CurrentTracks.Count;
            var totalSeconds = CurrentTracks.Sum(t => t.DurationSeconds);

            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;

            var durationText = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
            var trackWord = count == 1 ? "track" : "tracks";

            return $"{count} {trackWord} • {durationText}";
        }
    }

    public string NewPlaylistName
    {
        get => _newPlaylistName;
        set => this.RaiseAndSetIfChanged(ref _newPlaylistName, value);
    }

    public ReactiveCommand<Unit, Unit> CreatePlaylistCommand { get; }
    public ReactiveCommand<Playlist, Unit> DeletePlaylistCommand { get; }
    public ReactiveCommand<Track, Unit> PlayTrackCommand { get; }
    public ReactiveCommand<Playlist, Unit> PlayPlaylistCommand { get; }
    public ReactiveCommand<Track, Unit> RemoveTrackCommand { get; }
    public ReactiveCommand<(int fromIndex, int toIndex), Unit> MoveTrackCommand { get; }
    public ReactiveCommand<(Track track, Playlist playlist), Unit> AddTrackToPlaylistCommand { get; }
    public ReactiveCommand<(Playlist playlist, string filePath), Unit> SetPlaylistCoverCommand { get; }

    // for avalonia previewer
    public PlaylistViewModel()
    {
        System.Diagnostics.Debug.WriteLine("[PlaylistViewModel] Empty constructor called");

        _playlistService = null!;
        _queue = null!;
        _playbackManager = null!;

        Playlists.Add(new Playlist { Id = 1, Name = "Playlist 1" });
        Playlists.Add(new Playlist { Id = 2, Name = "Playlist 2" });

        CreatePlaylistCommand = ReactiveCommand.Create(() => { });
        DeletePlaylistCommand = ReactiveCommand.Create<Playlist>(_ => { });
        PlayTrackCommand = ReactiveCommand.Create<Track>(_ => { });
        PlayPlaylistCommand = ReactiveCommand.Create<Playlist>(_ => { });
        RemoveTrackCommand = ReactiveCommand.Create<Track>(_ => { });
        MoveTrackCommand = ReactiveCommand.Create<(int, int)>(_ => { });
        AddTrackToPlaylistCommand = ReactiveCommand.Create<(Track, Playlist)>(_ => { });
        SetPlaylistCoverCommand = ReactiveCommand.Create<(Playlist, string)>(_ => { });
    }

    public PlaylistViewModel(IPlaylistService playlistService, PlaybackQueue queue, IPlaybackManager playbackManager)
    {
        System.Diagnostics.Debug.WriteLine("[PlaylistViewModel] Main constructor called");

        _playlistService = playlistService;
        _queue = queue;
        _playbackManager = playbackManager;

        CreatePlaylistCommand = ReactiveCommand.CreateFromTask(CreatePlaylistAsync);
        DeletePlaylistCommand = ReactiveCommand.CreateFromTask<Playlist>(DeletePlaylistAsync);
        PlayTrackCommand = ReactiveCommand.CreateFromTask<Track>(PlayTrackAsync);
        PlayPlaylistCommand = ReactiveCommand.CreateFromTask<Playlist>(PlayPlaylistAsync);
        RemoveTrackCommand = ReactiveCommand.CreateFromTask<Track>(RemoveTrackAsync);
        MoveTrackCommand = ReactiveCommand.CreateFromTask<(int fromIndex, int toIndex)>(MoveTrackAsync);
        AddTrackToPlaylistCommand = ReactiveCommand.CreateFromTask<(Track track, Playlist playlist)>(AddTrackToPlaylistAsync);
        SetPlaylistCoverCommand = ReactiveCommand.CreateFromTask<(Playlist playlist, string filePath)>(SetPlaylistCoverAsync);

        System.Diagnostics.Debug.WriteLine($"[VM] DeletePlaylistCommand is null: {DeletePlaylistCommand == null}");

        CurrentTracks.CollectionChanged += (_, _) =>
        this.RaisePropertyChanged(nameof(TracksSummary));
    }

    private async Task LoadPlaylistsAsync()
    {
        var playlists = await _playlistService.GetAllWithTracksAsync();
        Playlists.Clear();
        foreach (var p in playlists)
            Playlists.Add(p);
    }

    private async Task PlayPlaylistAsync(Playlist playlist)
    {
        var full = await _playlistService.GetWithTracksAsync(playlist.Id);
        if (full == null) return;

        var tracks = full.PlaylistTracks
            .OrderBy(pt => pt.Position)
            .Select(pt => pt.Track)
            .Where(t => t != null)
            .Cast<Track>()
            .ToList();

        if (tracks.Count == 0) return;

        _queue.Clear();
        foreach (var t in tracks)
            _queue.Add(t);
        _queue.SetIndex(0);
        await _playbackManager.PlayAsync();
    }

    private async Task MoveTrackAsync((int fromIndex, int toIndex) args)
    {
        if (SelectedPlaylist == null) return;
        if (args.fromIndex == args.toIndex) return;

        System.Diagnostics.Debug.WriteLine($"[VM] MoveTrackAsync called, from={args.fromIndex} to={args.toIndex}");

        if (SelectedPlaylist == null)
        {
            System.Diagnostics.Debug.WriteLine("[VM] SelectedPlaylist is null");
            return;
        }
        if (args.fromIndex == args.toIndex) return;

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var track = CurrentTracks[args.fromIndex];
            CurrentTracks.RemoveAt(args.fromIndex);
            CurrentTracks.Insert(args.toIndex, track);
        });

        await _playlistService.UpdateTrackPositionsAsync(
            SelectedPlaylist.Id,
            CurrentTracks.Select(t => t.Id).ToList());
    }

    private async Task LoadTracksAsync(int playlistId)
    {
        var playlist = await _playlistService.GetWithTracksAsync(playlistId);
        CurrentTracks.Clear();
        if (playlist == null) return;
        System.Diagnostics.Debug.WriteLine($"[Playlist] Tracks count: {playlist.PlaylistTracks.Count}");
        foreach (var pt in playlist.PlaylistTracks)
        {
            System.Diagnostics.Debug.WriteLine($"[Playlist] Track: {pt.Track?.Title}");
            if (pt.Track != null)
                CurrentTracks.Add(pt.Track);
        }
    }

    private async Task CreatePlaylistAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPlaylistName)) return;

        var playlist = await _playlistService.CreateAsync(NewPlaylistName);
        Playlists.Add(playlist);
        NewPlaylistName = string.Empty;

        var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
        await libraryVm.RefreshPlaylistsAsync();
    }

    private async Task DeletePlaylistAsync(Playlist playlist)
    {
        await _playlistService.DeleteAsync(playlist.Id);

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            CurrentTracks.Clear();
            SelectedPlaylist = null;

            var toRemove = Playlists.FirstOrDefault(p => p.Id == playlist.Id);
            if (toRemove != null)
                Playlists.Remove(toRemove);
        });

        var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
        await libraryVm.RefreshPlaylistsAsync();
    }

    private async Task PlayTrackAsync(Track track)
    {
        _queue.Clear();
        foreach (var t in CurrentTracks)
            _queue.Add(t);
        var index = _queue.Tracks.IndexOf(track);
        if (index >= 0)
            _queue.SetIndex(index);
        await _playbackManager.PlayAsync();
    }

    private async Task RemoveTrackAsync(Track track)
    {
        if (SelectedPlaylist == null) return;
        await _playlistService.RemoveTrackAsync(SelectedPlaylist.Id, track.Id);
        CurrentTracks.Remove(track);
    }

    public async Task RefreshAsync()
    {
        if (_selectedPlaylist != null)
            await LoadTracksAsync(_selectedPlaylist.Id);
    }

    private async Task AddTrackToPlaylistAsync((Track track, Playlist playlist) args)
    {
        await _playlistService.AddTrackAsync(args.playlist.Id, args.track.Id);

        if (SelectedPlaylist?.Id == args.playlist.Id)
            await LoadTracksAsync(args.playlist.Id);

        await RefreshPlaylistsAsync();
    }

    private async Task SetPlaylistCoverAsync((Playlist playlist, string filePath) args)
    {
        var updated = await _playlistService.SetCoverAsync(args.playlist.Id, args.filePath);

        updated.PlaylistTracks = args.playlist.PlaylistTracks;

        args.playlist.CoverImagePath = updated.CoverImagePath;
        args.playlist.UpdatedAt = updated.UpdatedAt;

        var index = Playlists.IndexOf(args.playlist);
        if (index >= 0)
        {
            var wasSelected = SelectedPlaylist?.Id == args.playlist.Id;

            Playlists[index] = updated;

            if (wasSelected)
                SelectedPlaylist = updated;
        }

        if (SelectedPlaylist?.Id == args.playlist.Id)
            SelectedPlaylistCoverPath = updated.CoverImagePath;
    }

    public async Task RefreshPlaylistsAsync()
    {
        await LoadPlaylistsAsync();
    }
}