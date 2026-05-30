using System;
using System.Globalization;
using System.Windows.Data;

namespace FDTD2DLab.Infrastructure.Converters
{
    public class PercentOfContainerConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double width && values[1] is double height)
            {
                double percent = parameter is double p ? p : 0.05; // 5% по умолчанию
                return Math.Min(width, height) * percent;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
