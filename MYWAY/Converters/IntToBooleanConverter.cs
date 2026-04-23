using System;
using System.Globalization;
using System.Windows.Data;

namespace MYWAY.Converters
{
    public class IntToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter != null && int.TryParse(parameter.ToString(), out var paramValue))
            {
                return intValue == paramValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null && int.TryParse(parameter.ToString(), out var paramValue))
            {
                return paramValue;
            }
            return Binding.DoNothing;
        }
    }
}
