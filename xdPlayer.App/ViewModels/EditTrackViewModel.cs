using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Entities;

namespace xdPlayer.App.ViewModels;

public class EditTrackViewModel : ReactiveObject
{
    private readonly ILibraryService _libraryService;
    private readonly ITagService _tagService;
    private readonly Track _track;

    public bool Confirmed { get; private set; }

    private string _title;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string? _artist;
    public string? Artist
    {
        get => _artist;
        set => this.RaiseAndSetIfChanged(ref _artist, value);
    }

    private string? _album;
    public string? Album
    {
        get => _album;
        set => this.RaiseAndSetIfChanged(ref _album, value);
    }

    private string? _genre;
    public string? Genre
    {
        get => _genre;
        set => this.RaiseAndSetIfChanged(ref _genre, value);
    }

    private int? _year;
    public int? Year
    {
        get => _year;
        set => this.RaiseAndSetIfChanged(ref _year, value);
    }

    private string? _musicBrainzId;
    public string? MusicBrainzId
    {
        get => _musicBrainzId;
        set => this.RaiseAndSetIfChanged(ref _musicBrainzId, value);
    }

    private bool _isLiked;
    public bool IsLiked
    {
        get => _isLiked;
        set => this.RaiseAndSetIfChanged(ref _isLiked, value);
    }

    private string? _coverImagePath;
    public string? CoverImagePath
    {
        get => _coverImagePath;
        set => this.RaiseAndSetIfChanged(ref _coverImagePath, value);
    }

    private string _newTagText = string.Empty;
    public string NewTagText
    {
        get => _newTagText;
        set => this.RaiseAndSetIfChanged(ref _newTagText, value);
    }

    public ObservableCollection<Tag> Tags { get; } = [];
    public ObservableCollection<Tag> AllAvailableTags { get; } = [];

    // Read-only info
    public string FilePath => _track.FilePath;
    public string Folder => System.IO.Path.GetDirectoryName(_track.FilePath) ?? string.Empty;
    public string DurationText => TimeSpan.FromSeconds(_track.DurationSeconds).ToString(@"m\:ss");
    public int PlayCount => _track.PlayCount;
    public string TotalListenedText
    {
        get
        {
            var ts = TimeSpan.FromSeconds(_track.TotalListenedSeconds);
            return ts.Hours > 0 ? $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s" : $"{ts.Minutes}m {ts.Seconds}s";
        }
    }
    public string AddedText => _track.AddedAt.ToLocalTime().ToString("d MMM yyyy, HH:mm");
    public string LastPlayedText => _track.LastPlayedAt.HasValue
        ? _track.LastPlayedAt.Value.ToLocalTime().ToString("d MMM yyyy, HH:mm")
        : "Never";

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetFromFileCommand { get; }
    public ReactiveCommand<Unit, Unit> AddTagCommand { get; }
    public ReactiveCommand<Tag, Unit> RemoveTagCommand { get; }
    public ReactiveCommand<Unit, Unit> ChangeCoverCommand { get; }

    public Action? RequestClose;
    public Action? RequestChangeCover;

    public EditTrackViewModel(Track track, ILibraryService libraryService, ITagService tagService)
    {
        _track = track;
        _libraryService = libraryService;
        _tagService = tagService;

        _title = track.Title;
        _artist = track.Artist;
        _album = track.Album;
        _genre = track.Genre;
        _year = track.Year;
        _musicBrainzId = track.MusicBrainzId;
        _isLiked = track.IsLiked;
        _coverImagePath = track.CoverImagePath;

        foreach (var tt in track.TrackTags)
            if (tt.Tag != null)
                Tags.Add(tt.Tag);

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(() => RequestClose?.Invoke());
        ResetFromFileCommand = ReactiveCommand.CreateFromTask(ResetFromFileAsync);
        AddTagCommand = ReactiveCommand.CreateFromTask(AddTagAsync);
        RemoveTagCommand = ReactiveCommand.CreateFromTask<Tag>(RemoveTagAsync);
        ChangeCoverCommand = ReactiveCommand.Create(() => RequestChangeCover?.Invoke());

        _ = LoadAllTagsAsync();
    }

    private async Task LoadAllTagsAsync()
    {
        var all = await _tagService.GetAllAsync();
        AllAvailableTags.Clear();
        foreach (var t in all)
            AllAvailableTags.Add(t);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title)) return;

        await _libraryService.UpdateTrackMetadataAsync(_track.Id, Title, Artist, Album, Genre, Year, MusicBrainzId);

        if (IsLiked != _track.IsLiked)
            await _libraryService.ToggleLikeAsync(_track.Id);

        Confirmed = true;
        RequestClose?.Invoke();
    }

    private async Task ResetFromFileAsync()
    {
        var updated = await _libraryService.ResetMetadataFromFileAsync(_track.Id);

        Title = updated.Title;
        Artist = updated.Artist;
        Album = updated.Album;
        Genre = updated.Genre;
        Year = updated.Year;
        CoverImagePath = updated.CoverImagePath;
    }

    private async Task AddTagAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTagText)) return;

        var tag = await _tagService.CreateAsync(NewTagText.Trim());
        await _tagService.AddTagToTrackAsync(_track.Id, tag.Id);

        if (!Tags.Any(t => t.Id == tag.Id))
            Tags.Add(tag);

        NewTagText = string.Empty;
    }

    private async Task RemoveTagAsync(Tag tag)
    {
        await _tagService.RemoveTagFromTrackAsync(_track.Id, tag.Id);
        var existing = Tags.FirstOrDefault(t => t.Id == tag.Id);
        if (existing != null)
            Tags.Remove(existing);
    }

    public void UpdateCoverPath(string path)
    {
        CoverImagePath = path;
    }
}