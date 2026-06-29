using xdPlayer.App.ViewModels;
using ReactiveUI;

namespace xdPlayer.App.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private bool _showLibrary = true;

    public LibraryViewModel Library { get; }
    public PlaylistViewModel Playlist { get; }
    public PlayerViewModel Player { get; }

    public bool ShowLibrary
    {
        get => _showLibrary;
        set => this.RaiseAndSetIfChanged(ref _showLibrary, value);
    }

    public bool ShowPlaylist => !ShowLibrary;

    public MainWindowViewModel(
    LibraryViewModel library,
    PlaylistViewModel playlist,
    PlayerViewModel player)
    {
        Library = library;
        Playlist = playlist;
        Player = player;

        Playlist.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PlaylistViewModel.SelectedPlaylist))
            {
                ShowLibrary = Playlist.SelectedPlaylist == null;
                this.RaisePropertyChanged(nameof(ShowPlaylist));
            }
        };
    }
}