using System;
using System.Collections.Generic;
using System.Text;

namespace xdPlayer.Application.Interfaces;

public interface IThemeService
{
    Task SetAccentColorAsync(string hexColor);
    Task SetMusicBrainzEnabledAsync(bool enabled);
    Task<(string AccentColor, bool MusicBrainzEnabled)> LoadSettingsAsync();
    void ApplyAccentColor(string hexColor);
}