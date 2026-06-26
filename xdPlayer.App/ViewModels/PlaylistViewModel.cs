using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Playback;

namespace xdPlayer.App.ViewModels;

public class PlaylistViewModel : ReactiveObject
{
    private readonly IPlaylistService _playlistService;
    private readonly PlaybackQueue _queue;
    private readonly IPlaybackManager _playbackManager;

    public ObservableCollection<Playlist> Playlists { get; } = [];
    public ObservableCollection<Track> CurrentTracks { get; } = [];

    private Playlist? _selectedPlaylist;
    public Playlist? SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPlaylist, value);
            if (value != null)
                _ = LoadTracksAsync(value.Id);
        }
    }

    private string _newPlaylistName = string.Empty;
    public string NewPlaylistName
    {
        get => _newPlaylistName;
        set => this.RaiseAndSetIfChanged(ref _newPlaylistName, value);
    }

    public ReactiveCommand<Unit, Unit> CreatePlaylistCommand { get; }
    public ReactiveCommand<Playlist, Unit> DeletePlaylistCommand { get; }
    public ReactiveCommand<Track, Unit> PlayTrackCommand { get; }
    public ReactiveCommand<Track, Unit> RemoveTrackCommand { get; }
    public ReactiveCommand<(int fromIndex, int toIndex), Unit> MoveTrackCommand { get; }

    // for avalonia previewer
    public PlaylistViewModel()
    {
        _playlistService = null!;
        _queue = null!;
        _playbackManager = null!;

        Playlists =
        [
            new Playlist { Id = 1, Name = "Playlist 1" },
            new Playlist { Id = 2, Name = "Playlist 2" },
        ];

        CreatePlaylistCommand = ReactiveCommand.Create(() => { });
        DeletePlaylistCommand = ReactiveCommand.Create<Playlist>(_ => { });
        PlayTrackCommand = ReactiveCommand.Create<Track>(_ => { });
        RemoveTrackCommand = ReactiveCommand.Create<Track>(_ => { });
        MoveTrackCommand = ReactiveCommand.Create<(int, int)>(_ => { });
    }

    public PlaylistViewModel(IPlaylistService playlistService, PlaybackQueue queue, IPlaybackManager playbackManager)
    {
        _playlistService = playlistService;
        _queue = queue;
        _playbackManager = playbackManager;

        CreatePlaylistCommand = ReactiveCommand.CreateFromTask(CreatePlaylistAsync);
        DeletePlaylistCommand = ReactiveCommand.CreateFromTask<Playlist>(DeletePlaylistAsync);
        PlayTrackCommand = ReactiveCommand.Create<Track>(PlayTrack);
        RemoveTrackCommand = ReactiveCommand.CreateFromTask<Track>(RemoveTrackAsync);
        MoveTrackCommand = ReactiveCommand.Create<(int, int)>(_ => { });

        _ = LoadPlaylistsAsync();
    }

    private async Task LoadPlaylistsAsync()
    {
        var playlists = await _playlistService.GetAllAsync();
        Playlists.Clear();
        foreach (var p in playlists)
            Playlists.Add(p);
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

        for (int i = 0; i < CurrentTracks.Count; i++)
            await _playlistService.UpdateTrackPositionAsync(SelectedPlaylist.Id, CurrentTracks[i].Id, i);
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
        Playlists.Remove(playlist);
        if (SelectedPlaylist?.Id == playlist.Id)
        {
            SelectedPlaylist = null;
            CurrentTracks.Clear();
        }

        var libraryVm = App.Services.GetRequiredService<LibraryViewModel>();
        await libraryVm.RefreshPlaylistsAsync();
    }

    private void PlayTrack(Track track)
    {
        _queue.Clear();
        foreach (var t in CurrentTracks)
            _queue.Add(t);
        var index = _queue.Tracks.IndexOf(track);
        if (index >= 0)
            _queue.SetIndex(index);
        _playbackManager.Play();
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
}