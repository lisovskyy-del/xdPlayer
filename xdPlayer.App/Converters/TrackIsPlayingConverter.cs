using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using xdPlayer.App.ViewModels;

namespace xdPlayer.App.Converters;

public class TrackIsPlayingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int trackId) return Brushes.Transparent;

        var playerVm = App.Services?.GetService<PlayerViewModel>();
        if (playerVm == null) return Brushes.Transparent;

        if (trackId == playerVm.CurrentTrackId)
            return Avalonia.Application.Current?.Resources["AccentBrush"] as IBrush ?? Brushes.Transparent;

        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}