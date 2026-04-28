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
                    1 => new SolidColorBrush(Color.FromRgb(76, 175, 80)),   // green
                    2 => new SolidColorBrush(Color.FromRgb(255, 167, 38)),  // orange
                    3 => new SolidColorBrush(Color.FromRgb(229, 57, 53)),   // red
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

    public class CompletedToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? 0.45 : 1.0;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class OverdueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b
                ? new SolidColorBrush(Color.FromRgb(229, 57, 53))
                : new SolidColorBrush(Color.FromRgb(60, 60, 80));
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

    public class MoodToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int mood)
            {
                return mood switch
                {
                    1 => new SolidColorBrush(Color.FromRgb(229, 57, 53)),
                    2 => new SolidColorBrush(Color.FromRgb(255, 167, 38)),
                    3 => new SolidColorBrush(Color.FromRgb(255, 213, 79)),
                    4 => new SolidColorBrush(Color.FromRgb(102, 187, 106)),
                    5 => new SolidColorBrush(Color.FromRgb(42, 182, 115)),
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class IsCompletedTodayToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b
                ? new SolidColorBrush(Color.FromRgb(42, 182, 115))
                : new SolidColorBrush(Color.FromRgb(50, 50, 70));
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
