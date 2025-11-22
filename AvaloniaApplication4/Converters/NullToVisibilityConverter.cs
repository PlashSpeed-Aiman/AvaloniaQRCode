using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaApplication4.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public static readonly NullToVisibilityConverter IsNull = new() { Invert = false };
        public static readonly NullToVisibilityConverter IsNotNull = new() { Invert = true };

        public bool Invert { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isNull = value == null;
            return Invert ? !isNull : isNull;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
