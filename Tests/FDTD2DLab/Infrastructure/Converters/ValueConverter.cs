using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FDTD2DLab.Infrastructure.Converters;

public abstract class ValueConverter : Freezable, IValueConverter
{
    #region Parameter : object - Параметр

    /// <summary>Параметр</summary>
    public static readonly DependencyProperty ParameterProperty =
        DependencyProperty.Register(
            nameof(Parameter),
            typeof(object),
            typeof(ValueConverter),
            new PropertyMetadata(default(object)));

    /// <summary>Параметр</summary>
    //[Category("")]
    [Description("Параметр")]
    public object Parameter { get => (object)GetValue(ParameterProperty); set => SetValue(ParameterProperty, value); }

    #endregion

    protected override Freezable CreateInstanceCore() => ((Freezable)Activator.CreateInstance(GetType()))!;

    object IValueConverter.Convert(object v, Type t, object p, CultureInfo c) => Convert(v, p);

    object IValueConverter.ConvertBack(object v, Type t, object p, CultureInfo c) => ConvertBack(v, p);

    protected abstract object Convert(object value, object parameter);
    protected virtual object ConvertBack(object value, object parameter) => throw new NotSupportedException();
}
