namespace FDTD2DLab.Infrastructure.Converters
{
    public class Div : ValueConverter
    {
        protected override object Convert(object value, object parameter)
        {
            if (value is null || (Parameter ?? parameter) is not { } p) return value;
            var v = System.Convert.ToDouble(value);
            var x = System.Convert.ToDouble(p);
            return v / x;
        }

        protected override object ConvertBack(object value, object parameter)
        {
            if (value is null || (Parameter ?? parameter) is not { } p) return value;
            var v = System.Convert.ToDouble(value);
            var x = System.Convert.ToDouble(p);
            return v * x;
        }
    }
}