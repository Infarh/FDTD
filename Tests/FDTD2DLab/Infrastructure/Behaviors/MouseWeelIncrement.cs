using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace FDTD2DLab.Infrastructure.Behaviors;

public class MouseWheelIncrement : Behavior<FrameworkElement>
{
    #region Ratio : double - Множитель

    /// <summary>Множитель</summary>
    public static readonly DependencyProperty RatioProperty =
        DependencyProperty.Register(
            nameof(Ratio),
            typeof(double),
            typeof(MouseWheelIncrement),
            new PropertyMetadata(1d));

    /// <summary>Множитель</summary>
    //[Category("")]
    [Description("Множитель")]
    public double Ratio { get => (double)GetValue(RatioProperty); set => SetValue(RatioProperty, value); }

    #endregion

    #region Value : double - Значение

    /// <summary>Значение</summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(MouseWheelIncrement),
            new FrameworkPropertyMetadata(default(double)) { BindsTwoWayByDefault = true });

    /// <summary>Значение</summary>
    //[Category("")]
    [Description("Значение")]
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

    #endregion

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseWheel += OnMouseWheel;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseWheel -= OnMouseWheel;
    }

    private void OnMouseWheel(object Sender, MouseWheelEventArgs E) => Value += Ratio * E.Delta;
}
