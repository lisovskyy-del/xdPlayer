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
    private readonly ProfileViewModel _profileVm;

    public string ProfileDisplayName => _profileVm.DisplayName;
    public string? ProfileAvatarPath => _profileVm.AvatarPath;

    public Action? LibraryRequested;
    public Action? PlaylistRequested;
    public Action? ProfileRequested;
    public Action? SettingsRequested;

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
    public ReactiveCommand<Unit, Unit> ShowProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleAddPlaylistCommand { get; }
    public ReactiveCommand<Unit, Unit> ConfirmAddPlaylistCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelAddPlaylistCommand { get; }

    public SidebarViewModel(PlaylistViewModel playlistVm, ProfileViewModel profileVm)
    {
        _playlistVm = playlistVm;
        _profileVm = profileVm;

        _profileVm.WhenAnyValue(x => x.DisplayName, x => x.AvatarPath)
            .Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(ProfileDisplayName));
            this.RaisePropertyChanged(nameof(ProfileAvatarPath));
        });

        ShowLibraryCommand = ReactiveCommand.Create(() =>
        {
            SelectedPlaylist = null;
            LibraryRequested?.Invoke();
        });

        ShowProfileCommand = ReactiveCommand.Create(() =>
        {
            ProfileRequested?.Invoke();
        });

        ShowSettingsCommand = ReactiveCommand.Create(() =>
        {
            SettingsRequested?.Invoke();
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