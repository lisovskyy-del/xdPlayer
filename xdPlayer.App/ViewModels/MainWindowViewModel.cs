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
    public ProfileViewModel Profile { get; }
    public SettingsViewModel Settings { get; }

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
        PlayerViewModel player,
        ProfileViewModel profile,
        SettingsViewModel settings)
    {
        Library = library;
        Playlist = playlist;
        Player = player;
        CurrentPage = Library;
        Sidebar = sidebar;
        Profile = profile;
        Settings = settings;

        sidebar.LibraryRequested += ShowLibrary;
        sidebar.PlaylistRequested += ShowPlaylist;
        sidebar.ProfileRequested += ShowProfile;
        sidebar.SettingsRequested += ShowSettings;

        ShowLibraryCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = Library;
        });
    }

    public async void ShowLibrary()
    {
        await Library.RebuildQueueFromLibraryAsync();
        CurrentPage = Library;
    }

    public void ShowPlaylist()
    {
        CurrentPage = Playlist;
    }

    public void ShowProfile()
    {
        CurrentPage = Profile;
    }

    public void ShowSettings()
    {
        CurrentPage = Settings;
    }
}