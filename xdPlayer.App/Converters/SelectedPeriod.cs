using System;
using System.Globalization;
using Avalonia.Data.Converters;
using xdPlayer.Application.Interfaces;

namespace xdPlayer.App.Converters;

public class PeriodEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is StatsPeriod period && parameter is string paramStr &&
            Enum.TryParse<StatsPeriod>(paramStr, out var target))
        {
            return period == target;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}