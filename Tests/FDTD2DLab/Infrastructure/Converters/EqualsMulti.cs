using System;
using System.Globalization;
using System.Windows.Data;

namespace FDTD2DLab.Infrastructure.Converters;

public class EqualsMulti : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null) return null;
        if (values.Length == 1) return true;
        var last = values[0];
        for(var i = 1; i < values.Length; i++)
            if (!Equals(last, values[i]))
                return false;
        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
