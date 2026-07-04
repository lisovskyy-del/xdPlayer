using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System.Linq;
using System.Threading.Tasks;
using xdPlayer.Application.Interfaces;
using xdPlayer.Infrastructure.Data;

namespace xdPlayer.App.ViewModels;

public class ProfileViewModel : ReactiveObject
{
    private readonly IStatisticsService _statisticsService;
    private readonly AppDbContext _db;

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

    // for avalonia previewer
    public ProfileViewModel()
    {
        _statisticsService = null!;
        _db = null!;
        DisplayName = "User Name";
        MemberSince = "Member since May 2024";
        TracksInLibrary = 1248;
        LikedTracksCount = 86;
        TotalPlayTimeText = "134 h";
        DaysListened = 92;
        MostPlayedGenre = "Hip Hop";
    }

    public ProfileViewModel(IStatisticsService statisticsService, AppDbContext db)
    {
        _statisticsService = statisticsService;
        _db = db;

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var profile = await _db.UserProfiles.FirstOrDefaultAsync();
        if (profile != null)
        {
            DisplayName = profile.DisplayName;
            MemberSince = $"Member since {profile.CreatedAt:MMMM yyyy}";
        }

        var overview = await _statisticsService.GetOverviewAsync();
        TracksInLibrary = overview.TracksInLibrary;
        LikedTracksCount = overview.LikedTracksCount;
        DaysListened = overview.DaysListened;
        MostPlayedGenre = overview.MostPlayedGenre ?? "—";

        var hours = overview.TotalPlayTimeSeconds / 3600;
        TotalPlayTimeText = $"{hours} h";
    }
}