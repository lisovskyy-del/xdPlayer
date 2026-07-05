using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using xdPlayer.Application.Interfaces;

namespace xdPlayer.App.ViewModels;

public class SettingsViewModel : ReactiveObject
{
    private readonly IThemeService _themeService;

    private string _accentColorHex = "#1A56DB";
    public string AccentColorHex
    {
        get => _accentColorHex;
        set => this.RaiseAndSetIfChanged(ref _accentColorHex, value);
    }

    private bool _musicBrainzEnabled = true;
    public bool MusicBrainzEnabled
    {
        get => _musicBrainzEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _musicBrainzEnabled, value);
            if (!_isLoading)
                _ = _themeService.SetMusicBrainzEnabledAsync(value);
        }
    }

    private bool _isLoading = true;

    public ReactiveCommand<string, Unit> SetPresetColorCommand { get; }
    public ReactiveCommand<Unit, Unit> ApplyHexColorCommand { get; }

    // for avalonia previewer
    public SettingsViewModel()
    {
        _themeService = null!;
        SetPresetColorCommand = ReactiveCommand.Create<string>(_ => { });
        ApplyHexColorCommand = ReactiveCommand.Create(() => { });
    }

    public SettingsViewModel(IThemeService themeService)
    {
        _themeService = themeService;

        SetPresetColorCommand = ReactiveCommand.CreateFromTask<string>(async hex =>
        {
            AccentColorHex = hex;
            await _themeService.SetAccentColorAsync(hex);
        });

        ApplyHexColorCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var hex = AccentColorHex.Trim();
            if (!hex.StartsWith("#")) hex = "#" + hex;
            await _themeService.SetAccentColorAsync(hex);
        });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var settings = await _themeService.LoadSettingsAsync();
        AccentColorHex = settings.AccentColor;
        MusicBrainzEnabled = settings.MusicBrainzEnabled;
        _isLoading = false;
    }
}