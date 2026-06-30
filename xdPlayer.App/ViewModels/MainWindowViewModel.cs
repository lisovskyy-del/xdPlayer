using System.Reactive;
using ReactiveUI;

namespace xdPlayer.App.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private object? _currentPage;

    public LibraryViewModel Library { get; }
    public PlaylistViewModel Playlist { get; }
    public PlayerViewModel Player { get; }
    public SidebarViewModel Sidebar { get; }

    public ReactiveCommand<Unit, Unit> ShowLibraryCommand { get; }

    public object? CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public MainWindowViewModel(
        LibraryViewModel library,
        SidebarViewModel sidebar,
        PlaylistViewModel playlist,
        PlayerViewModel player)
    {
        Library = library;
        Playlist = playlist;
        Player = player;
        CurrentPage = Library;
        Sidebar = sidebar;

        sidebar.LibraryRequested += ShowLibrary;
        sidebar.PlaylistRequested += ShowPlaylist;

        ShowLibraryCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = Library;
        });
    }

    public void ShowLibrary()
    {
        CurrentPage = Library;
    }

    public void ShowPlaylist()
    {
        CurrentPage = Playlist;
    }
}