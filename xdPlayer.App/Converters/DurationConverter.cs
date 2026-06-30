using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace xdPlayer.App.Converters;

public class DurationConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int seconds)
            return "0:00";

        var time = TimeSpan.FromSeconds(seconds);

        if (time.TotalHours >= 1)
            return time.ToString(@"h\:mm\:ss");

        return time.ToString(@"m\:ss");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}