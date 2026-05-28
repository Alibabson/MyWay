using System;
using System.Globalization;
using System.Windows.Data;

namespace MyWay.Converters
{
    /// <summary>
    /// Przelicza (progress 0..1, containerWidth) → szerokość paska postępu w pikselach.
    /// </summary>
    public class ProgressWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2
                && values[0] is double progress
                && values[1] is double containerWidth)
            {
                return Math.Max(0, Math.Min(1, progress) * containerWidth);
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
