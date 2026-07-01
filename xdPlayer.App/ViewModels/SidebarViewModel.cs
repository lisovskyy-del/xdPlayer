using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.ViewModels;

public class SidebarViewModel : ReactiveObject
{
    private readonly PlaylistViewModel _playlistVm;

    public Action? LibraryRequested;
    public Action? PlaylistRequested;

    private bool _isLibrarySelected = true;
    public bool IsLibrarySelected
    {
        get => _isLibrarySelected;
        private set => this.RaiseAndSetIfChanged(ref _isLibrarySelected, value);
    }

    private bool _isAddingPlaylist;
    public bool IsAddingPlaylist
    {
        get => _isAddingPlaylist;
        set => this.RaiseAndSetIfChanged(ref _isAddingPlaylist, value);
    }

    public ReactiveCommand<Unit, Unit> ShowLibraryCommand { get; }
    public ReactiveCommand<Playlist, Unit> OpenPlaylistCommand { get; }
    public ReactiveCommand<(Track track, Playlist playlist), Unit> AddToPlaylistCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleAddPlaylistCommand { get; }
    public ReactiveCommand<Unit, Unit> ConfirmAddPlaylistCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelAddPlaylistCommand { get; }

    public SidebarViewModel(PlaylistViewModel playlistVm)
    {
        _playlistVm = playlistVm;

        ShowLibraryCommand = ReactiveCommand.Create(() =>
        {
            SelectedPlaylist = null;
            LibraryRequested?.Invoke();
        });

        OpenPlaylistCommand = ReactiveCommand.Create<Playlist>(playlist =>
        {
            SelectedPlaylist = playlist;
        });

        ToggleAddPlaylistCommand = ReactiveCommand.Create(() =>
        {
            IsAddingPlaylist = !IsAddingPlaylist;
            if (!IsAddingPlaylist)
                NewPlaylistName = string.Empty;
        });

        ConfirmAddPlaylistCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (string.IsNullOrWhiteSpace(NewPlaylistName))
                return;

            await _playlistVm.CreatePlaylistCommand.Execute();
            IsAddingPlaylist = false;
        });

        CancelAddPlaylistCommand = ReactiveCommand.Create(() =>
        {
            IsAddingPlaylist = false;
            NewPlaylistName = string.Empty;
        });
    }

    public ObservableCollection<Playlist> Playlists => _playlistVm.Playlists;

    public Playlist? SelectedPlaylist
    {
        get => _playlistVm.SelectedPlaylist;
        set
        {
            _playlistVm.SelectedPlaylist = value;
            IsLibrarySelected = value == null;

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
}