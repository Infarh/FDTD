using System;
using System.Globalization;
using System.Windows.Data;

namespace FDTD2DLab.Infrastructure.Converters
{
    public class SimulatingOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSimulating && isSimulating)
                return 0.3;   // полупрозрачность при моделировании
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
