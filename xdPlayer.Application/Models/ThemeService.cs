using Avalonia;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using xdPlayer.Application.Interfaces;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Application.Models;

public class ThemeService : IThemeService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ThemeService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ApplyAccentColor(string hexColor)
    {
        if (Avalonia.Application.Current == null) return;

        try
        {
            var color = Color.Parse(hexColor);
            Avalonia.Application.Current.Resources["AccentBrush"] = new SolidColorBrush(color);
        }
        catch
        {
            // ignore
        }
    }

    public async Task SetAccentColorAsync(string hexColor)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var profiles = await uow.UserProfiles.GetAllAsync();
        var profile = profiles.FirstOrDefault();
        if (profile == null) return;

        profile.AccentColor = hexColor;
        await uow.UserProfiles.UpdateAsync(profile);
        await uow.SaveChangesAsync();

        ApplyAccentColor(hexColor);
    }

    public async Task SetMusicBrainzEnabledAsync(bool enabled)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var profiles = await uow.UserProfiles.GetAllAsync();
        var profile = profiles.FirstOrDefault();
        if (profile == null) return;

        profile.MusicBrainzEnabled = enabled;
        await uow.UserProfiles.UpdateAsync(profile);
        await uow.SaveChangesAsync();
    }

    public async Task<(string AccentColor, bool MusicBrainzEnabled)> LoadSettingsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var profiles = await uow.UserProfiles.GetAllAsync();
        var profile = profiles.FirstOrDefault();

        return profile != null
            ? (profile.AccentColor, profile.MusicBrainzEnabled)
            : ("#1A56DB", true);
    }
}