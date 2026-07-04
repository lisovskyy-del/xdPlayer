using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using xdPlayer.Application.Interfaces;

namespace xdPlayer.App.ViewModels;

public class ProfileViewModel : ReactiveObject
{
    private readonly IStatisticsService _statisticsService;

    private string? _avatarPath;
    public string? AvatarPath
    {
        get => _avatarPath;
        set => this.RaiseAndSetIfChanged(ref _avatarPath, value);
    }

    private string _displayName = "User";
    public string DisplayName
    {
        get => _displayName;
        set => this.RaiseAndSetIfChanged(ref _displayName, value);
    }

    private string _memberSince = string.Empty;
    public string MemberSince
    {
        get => _memberSince;
        set => this.RaiseAndSetIfChanged(ref _memberSince, value);
    }

    private int _tracksInLibrary;
    public int TracksInLibrary
    {
        get => _tracksInLibrary;
        set => this.RaiseAndSetIfChanged(ref _tracksInLibrary, value);
    }

    private int _likedTracksCount;
    public int LikedTracksCount
    {
        get => _likedTracksCount;
        set => this.RaiseAndSetIfChanged(ref _likedTracksCount, value);
    }

    private string _totalPlayTimeText = "0h";
    public string TotalPlayTimeText
    {
        get => _totalPlayTimeText;
        set => this.RaiseAndSetIfChanged(ref _totalPlayTimeText, value);
    }

    private int _daysListened;
    public int DaysListened
    {
        get => _daysListened;
        set => this.RaiseAndSetIfChanged(ref _daysListened, value);
    }

    private string _mostPlayedGenre = "—";
    public string MostPlayedGenre
    {
        get => _mostPlayedGenre;
        set => this.RaiseAndSetIfChanged(ref _mostPlayedGenre, value);
    }

    private StatsPeriod _selectedPeriod = StatsPeriod.Days30;
    public StatsPeriod SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPeriod, value);
            _ = LoadTopTracksAsync();
            _ = LoadChartAsync();
        }
    }

    public ObservableCollection<TopTrackItem> TopTracks { get; } = [];
    public ObservableCollection<DailyPlayCount> ChartData { get; } = [];

    public ReactiveCommand<string, Unit> SelectPeriodCommand { get; }

    // for avalonia previewer
    public ProfileViewModel()
    {
        _statisticsService = null!;
        DisplayName = "User Name";
        MemberSince = "Member since May 2024";
        TracksInLibrary = 1248;
        LikedTracksCount = 86;
        TotalPlayTimeText = "134 h";
        DaysListened = 92;
        MostPlayedGenre = "Hip Hop";

        SelectPeriodCommand = ReactiveCommand.Create<string>(_ => { });
    }

    public ProfileViewModel(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;

        SelectPeriodCommand = ReactiveCommand.Create<string>(periodStr =>
        {
            if (Enum.TryParse<StatsPeriod>(periodStr, out var period))
                SelectedPeriod = period;
        });

        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        var profile = await _statisticsService.GetUserProfileAsync();
        if (profile != null)
        {
            DisplayName = profile.DisplayName;
            AvatarPath = profile.AvatarPath;
            MemberSince = $"Member since {profile.CreatedAt:MMMM yyyy}";
        }

        var overview = await _statisticsService.GetOverviewAsync();
        TracksInLibrary = overview.TracksInLibrary;
        LikedTracksCount = overview.LikedTracksCount;
        DaysListened = overview.DaysListened;
        MostPlayedGenre = overview.MostPlayedGenre ?? "—";

        var hours = overview.TotalPlayTimeSeconds / 3600;
        TotalPlayTimeText = $"{hours} h";

        await LoadTopTracksAsync();
        await LoadChartAsync();
    }

    private async Task LoadTopTracksAsync()
    {
        var tracks = await _statisticsService.GetTopTracksAsync(SelectedPeriod);
        TopTracks.Clear();
        foreach (var t in tracks)
            TopTracks.Add(t);
    }

    private async Task LoadChartAsync()
    {
        var data = await _statisticsService.GetPlaysPerDayAsync(SelectedPeriod);
        ChartData.Clear();
        foreach (var d in data)
            ChartData.Add(d);
    }
}