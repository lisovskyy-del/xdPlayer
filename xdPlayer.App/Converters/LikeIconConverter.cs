using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace xdPlayer.App.Converters;

public class LikeIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isLiked)
            return isLiked ? "♥" : "♡";

        return "♡";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}