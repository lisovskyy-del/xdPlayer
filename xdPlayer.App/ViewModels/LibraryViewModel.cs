using Avalonia.Platform.Storage;
using DynamicData;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;
using xdPlayer.Domain.Playback;

namespace xdPlayer.App.ViewModels;

public class LibraryViewModel : ReactiveObject
{
    private readonly ILibraryService _libraryService;
    private readonly PlaybackQueue _queue;
    private readonly IPlaybackManager _playbackManager;
    private readonly IPlaylistService _playlistService;
    private readonly ITagService _tagService;

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchQuery, value);
            _ = SearchAsync(value);
        }
    }

    private string _newTagName = string.Empty;
    public string NewTagName
    {
        get => _newTagName;
        set => this.RaiseAndSetIfChanged(ref _newTagName, value);
    }

    public ObservableCollection<Playlist> Playlists { get; } = [];
    public ObservableCollection<Tag> AllTags { get; } = [];
    public ObservableCollection<Track> Tracks { get; } = [];

    public ReactiveCommand<Unit, Unit> AddFileCommand { get; }
    public ReactiveCommand<Unit, Unit> AddFolderCommand { get; }
    public ReactiveCommand<Track, Unit> PlayTrackCommand { get; }
    public ReactiveCommand<(Track, Playlist), Unit> AddToPlaylistCommand { get; }

    public ReactiveCommand<(Track, Tag), Unit> AddTagToTrackCommand { get; }
    public ReactiveCommand<(Track, Tag), Unit> RemoveTagFromTrackCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateTagCommand { get; }


    // for avalonia previewer
    public LibraryViewModel()
    {
        _libraryService = null!;
        _queue = null!;
        _playbackManager = null!;
        _playlistService = null!;
        _tagService = null!;

        Tracks.Add(new Track { Title = "Track 1", Artist = "Artist 1", DurationSeconds = 213 });
        Tracks.Add(new Track { Title = "Track 2", Artist = "Artist 2", DurationSeconds = 180 });
        Tracks.Add(new Track { Title = "Track 3", Artist = "Artist 3", DurationSeconds = 310 });

        AddFileCommand = ReactiveCommand.Create(() => { });
        AddFolderCommand = ReactiveCommand.Create(() => { });
        PlayTrackCommand = ReactiveCommand.Create<Track>(_ => { });
        AddToPlaylistCommand = ReactiveCommand.Create<(Track, Playlist)>(_ => { });
        AddTagToTrackCommand = ReactiveCommand.Create<(Track, Tag)>(_ => { });
        RemoveTagFromTrackCommand = ReactiveCommand.Create<(Track, Tag)>(_ => { });
        CreateTagCommand = ReactiveCommand.Create(() => { });
    }

    public LibraryViewModel(ILibraryService libraryService, PlaybackQueue queue, 
        IPlaybackManager playbackManager, IPlaylistService playlistService,
        ITagService tagService)
    {
        _libraryService = libraryService;
        _queue = queue;
        _playbackManager = playbackManager;
        _playlistService = playlistService;
        _tagService = tagService;

        AddFileCommand = ReactiveCommand.CreateFromTask(AddFileAsync);
        AddFolderCommand = ReactiveCommand.CreateFromTask(AddFolderAsync);
        PlayTrackCommand = ReactiveCommand.Create<Track>(PlayTrack);
        AddToPlaylistCommand = ReactiveCommand.CreateFromTask<(Track, Playlist)>(AddToPlaylistAsync);
        AddTagToTrackCommand = ReactiveCommand.CreateFromTask<(Track, Tag)>(AddTagToTrackAsync);
        RemoveTagFromTrackCommand = ReactiveCommand.CreateFromTask<(Track, Tag)>(RemoveTagFromTrackAsync);
        CreateTagCommand = ReactiveCommand.CreateFromTask(CreateTagAsync);

        _ = LoadTracksAsync();
        _ = LoadPlaylistsAsync();
        _ = LoadTracksAsync();
        _ = LoadPlaylistsAsync();
        _ = LoadTagsAsync();
    }

    private async Task LoadTracksAsync()
    {
        var tracks = await _libraryService.GetAllAsync();
        Tracks.Clear();
        _queue.Clear();
        foreach (var t in tracks)
        {
            Tracks.Add(t);
            _queue.Add(t);
        }
    }

    private async Task SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            await LoadTracksAsync();
            return;
        }
        var results = await _libraryService.SearchAsync(query);
        Tracks.Clear();
        foreach (var t in results)
            Tracks.Add(t);
    }

    private async Task AddFileAsync()
    {
        var topLevel = Avalonia.Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Add tracks",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("Audio files")
            {
                Patterns = ["*.mp3", "*.flac", "*.wav", "*.ogg", "*.m4a"]
            }
            ]
        });

        foreach (var file in files)
        {
            var track = await _libraryService.AddFileAsync(file.Path.LocalPath);
            if (!Tracks.Contains(track))
            {
                Tracks.Add(track);
                _queue.Add(track);
            }
        }
    }

    private async Task AddFolderAsync()
    {
        var topLevel = Avalonia.Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Add Folder",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;

        var tracks = await _libraryService.AddFolderAsync(folders[0].Path.LocalPath);
        foreach (var track in tracks)
        {
            if (!Tracks.Contains(track))
            {
                Tracks.Add(track);
                _queue.Add(track);
            }
        }
    }

    private void PlayTrack(Track track)
    {
        var index = _queue.Tracks.IndexOf(track);

        if (index >= 0)
        {
            _queue.SetIndex(index);
        }
        else
        {
            _queue.Add(track);
            _queue.SetIndex(_queue.Tracks.Count - 1);
        }

        _playbackManager.Play();
    }

    private async Task LoadPlaylistsAsync()
    {
        var playlists = await _playlistService.GetAllAsync();
        Playlists.Clear();
        foreach (var p in playlists)
            Playlists.Add(p);
    }

    private async Task AddToPlaylistAsync((Track track, Playlist playlist) args)
    {
        await _playlistService.AddTrackAsync(args.playlist.Id, args.track.Id);
    }

    public async Task RefreshPlaylistsAsync()
    {
        var playlists = await _playlistService.GetAllAsync();
        Playlists.Clear();
        foreach (var p in playlists)
            Playlists.Add(p);
    }

    private async Task LoadTagsAsync()
    {
        var tags = await _tagService.GetAllAsync();
        AllTags.Clear();
        foreach (var t in tags)
            AllTags.Add(t);
    }

    private async Task CreateTagAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTagName)) return;
        var tag = await _tagService.CreateAsync(NewTagName);
        AllTags.Add(tag);
        NewTagName = string.Empty;
    }

    private async Task AddTagToTrackAsync((Track track, Tag tag) args)
    {
        await _tagService.AddTagToTrackAsync(args.track.Id, args.tag.Id);
    }

    private async Task RemoveTagFromTrackAsync((Track track, Tag tag) args)
    {
        await _tagService.RemoveTagFromTrackAsync(args.track.Id, args.tag.Id);
    }

    public async Task RefreshTagsAsync()
    {
        var tags = await _tagService.GetAllAsync();
        AllTags.Clear();
        foreach (var t in tags)
            AllTags.Add(t);
    }
}