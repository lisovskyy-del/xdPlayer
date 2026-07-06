using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace xdPlayer.App.Converters;

public class TrackPlayingMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not int trackId || values[1] is not int currentId)
            return Brushes.Transparent;

        if (trackId == currentId && trackId != 0)
            return Avalonia.Application.Current?.Resources["AccentBrush"] as IBrush ?? Brushes.Transparent;

        return Brushes.Transparent;
    }
}