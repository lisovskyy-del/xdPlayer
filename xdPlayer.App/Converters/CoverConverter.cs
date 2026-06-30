using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace xdPlayer.App.Converters;

public class CoverConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path &&
            !string.IsNullOrWhiteSpace(path) &&
            File.Exists(path))
        {
            try
            {
                return new Bitmap(path);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}