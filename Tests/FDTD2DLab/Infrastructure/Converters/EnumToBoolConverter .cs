using System;
using System.Globalization;
using System.Windows.Data;

namespace FDTD2DLab.Infrastructure.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? parameter : Binding.DoNothing;
        }
    }
}