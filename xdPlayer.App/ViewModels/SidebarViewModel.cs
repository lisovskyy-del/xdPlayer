using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.ViewModels;

public class SidebarViewModel : ReactiveObject
{
    private readonly PlaylistViewModel _playlistVm;

    public Action? LibraryRequested;
    public Action? PlaylistRequested;

    public ReactiveCommand<Unit, Unit> ShowLibraryCommand { get; }
    public ReactiveCommand<Playlist, Unit> OpenPlaylistCommand { get; }
    public ReactiveCommand<(Track track, Playlist playlist), Unit> AddToPlaylistCommand { get; }

    public SidebarViewModel(PlaylistViewModel playlistVm)
    {
        _playlistVm = playlistVm;

        CreatePlaylistCommand = playlistVm.CreatePlaylistCommand;

        ShowLibraryCommand = ReactiveCommand.Create(() =>
        {
            LibraryRequested?.Invoke();
        });

        OpenPlaylistCommand = ReactiveCommand.Create<Playlist>(playlist =>
        {
            SelectedPlaylist = playlist;
        });
    }

    public ObservableCollection<Playlist> Playlists => _playlistVm.Playlists;

    public Playlist? SelectedPlaylist
    {
        get => _playlistVm.SelectedPlaylist;
        set
        {
            _playlistVm.SelectedPlaylist = value;

            if (value != null)
                PlaylistRequested?.Invoke();

            this.RaisePropertyChanged();
        }
    }

    public string NewPlaylistName
    {
        get => _playlistVm.NewPlaylistName;
        set
        {
            _playlistVm.NewPlaylistName = value;
            this.RaisePropertyChanged();
        }
    }

    public ReactiveCommand<Unit, Unit> CreatePlaylistCommand { get; }
}