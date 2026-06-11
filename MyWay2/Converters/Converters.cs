using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MyWay.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Visibility v && v == Visibility.Visible;
    }

    public class BoolToInverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class DifficultyToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int diff)
            {
                return diff switch
                {
                    1 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5DC0B3")),   // green
                    2 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5DA1C0")),   // blue
                    3 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C05DBB")),   // pink
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class CompletedToStrikethroughConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? TextDecorations.Strikethrough : null!;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }



    public class StreakToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int streak)
            {
                if (streak >= 7) return new SolidColorBrush(Color.FromRgb(229, 57, 53));
                if (streak >= 3) return new SolidColorBrush(Color.FromRgb(255, 167, 38));
                return new SolidColorBrush(Color.FromRgb(120, 120, 150));
            }
            return Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }



    public class IsCompletedTodayToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b
               ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5DC0B3"))
               : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#625DC0"));
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
