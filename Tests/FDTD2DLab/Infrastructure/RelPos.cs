using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FDTD2DLab.Infrastructure
{
    public class RelPos
    {
        #region Attached property CanvasBinding : bool - Инициализация связи
        /// <summary>Инициализация связи</summary>
        public static readonly DependencyProperty CanvasBindingProperty =
            DependencyProperty.RegisterAttached(
                "CanvasBinding",
                typeof(bool),
                typeof(RelPos),
                new PropertyMetadata(default(bool), OnCanvasBinding));

        private static void OnCanvasBinding(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            if ((bool)E.NewValue)
            {
                BindingOperations.SetBinding(D, LeftProperty, new Binding("(Canvas.Left)") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
                BindingOperations.SetBinding(D, BottomProperty, new Binding("(Canvas.Bottom)") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
            }
            else
            {
                BindingOperations.ClearBinding(D, LeftProperty);
                BindingOperations.ClearBinding(D, BottomProperty);
            }
        }

        /// <summary>Инициализация связи</summary>
        public static void SetCanvasBinding(DependencyObject d, bool value) => d.SetValue(CanvasBindingProperty, value);

        /// <summary>Инициализация связи</summary>
        public static bool GetCanvasBinding(DependencyObject d) => (bool)d.GetValue(CanvasBindingProperty);

        #endregion

        #region Attached property X : double - Значение по горизонтали

        /// <summary>Значение по горизонтали</summary>
        public static readonly DependencyProperty XProperty =
            DependencyProperty.RegisterAttached(
                "X",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(1d, OnXChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnXChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var x = (double)E.NewValue;
            if (double.IsNaN(x)) return;
            var max_x = GetMaxX(D);
            var max_width = GetContainerWidth(D);
            if(max_width == 0) return;
            var left = x * max_width / max_x;
            SetLeft(D, left);
        }

        /// <summary>Значение по горизонтали</summary>
        public static void SetX(DependencyObject d, double value) => d.SetValue(XProperty, value);

        /// <summary>Значение по горизонтали</summary>
        public static double GetX(DependencyObject d) => (double)d.GetValue(XProperty);

        #endregion

        #region Attached property MaxX : double - Максимальное значение по горизонтали

        /// <summary>Максимальное значение по горизонтали</summary>
        public static readonly DependencyProperty MaxXProperty =
            DependencyProperty.RegisterAttached(
                "MaxX",
                typeof(double),
                typeof(RelPos),
                new PropertyMetadata(1d, OnMaxXChanged));

        private static void OnMaxXChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var x = GetX(D);
            var max_x = (double)E.NewValue;
            var max_width = GetContainerWidth(D);
            if(max_width == 0) return;
            var left = x * max_width / max_x;
            SetLeft(D, left);
        }

        /// <summary>Максимальное значение по горизонтали</summary>
        public static void SetMaxX(DependencyObject d, double value) => d.SetValue(MaxXProperty, value);

        /// <summary>Максимальное значение по горизонтали</summary>
        public static double GetMaxX(DependencyObject d) => (double)d.GetValue(MaxXProperty);

        #endregion

        #region Attached property ContainerWidth : double - Максимальная ширина контейнера

        /// <summary>Максимальная ширина контейнера</summary>
        public static readonly DependencyProperty ContainerWidthProperty =
            DependencyProperty.RegisterAttached(
                "ContainerWidth",
                typeof(double),
                typeof(RelPos),
                new PropertyMetadata(default(double), OnContainerWidthChanged));

        private static void OnContainerWidthChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var x = GetX(D);
            var max_x = GetMaxX(D);
            var max_width = (double)E.NewValue;
            if(max_width == 0) return;
            var left = x * max_width / max_x;
            SetLeft(D, left);
        }

        /// <summary>Максимальная ширина контейнера</summary>
        public static void SetContainerWidth(DependencyObject d, double value) => d.SetValue(ContainerWidthProperty, value);

        /// <summary>Максимальная ширина контейнера</summary>
        public static double GetContainerWidth(DependencyObject d) => (double)d.GetValue(ContainerWidthProperty);

        #endregion

        #region Attached property Left : double - Смещение по горизонтали

        /// <summary>Смещение по горизонтали</summary>
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.RegisterAttached(
                "Left",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(default(double), OnLeftChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }
                );

        private static void OnLeftChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var left = (double)E.NewValue;
            if(double.IsNaN(left)) return;
            var max_x = GetMaxX(D);
            var max_width = GetContainerWidth(D);
            if(max_width == 0) return;
            var x = left * max_x / max_width;
            SetX(D, x);
        }

        /// <summary>Смещение по горизонтали</summary>
        public static void SetLeft(DependencyObject d, double value) => d.SetValue(LeftProperty, value);

        /// <summary>Смещение по горизонтали</summary>
        public static double GetLeft(DependencyObject d) => (double)d.GetValue(LeftProperty);

        #endregion

        #region Attached property Y : double - Значение по вертикали

        /// <summary>Значение по вертикали</summary>
        public static readonly DependencyProperty YProperty =
            DependencyProperty.RegisterAttached(
                "Y",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(default(double), OnYChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnYChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var y = (double)E.NewValue;
            if (double.IsNaN(y)) return;
            var max_y = GetMaxY(D);
            var max_height = GetContainerHeight(D);
            if(max_height == 0) return;
            var bottom = y * max_height / max_y;
            SetBottom(D, bottom);
        }

        /// <summary>Значение по вертикали</summary>
        public static void SetY(DependencyObject d, double value) => d.SetValue(YProperty, value);

        /// <summary>Значение по вертикали</summary>
        public static double GetY(DependencyObject d) => (double)d.GetValue(YProperty);

        #endregion

        #region Attached property MaxY : double - Максимальное значение по горизонтали

        /// <summary>Максимальное значение по горизонтали</summary>
        public static readonly DependencyProperty MaxYProperty =
            DependencyProperty.RegisterAttached(
                "MaxY",
                typeof(double),
                typeof(RelPos),
                new PropertyMetadata(1d, OnMaxYChanged));

        private static void OnMaxYChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var y = GetY(D);
            var max_y = (double)E.NewValue;
            var max_height = GetContainerHeight(D);
            if(max_height == 0) return;
            var bottom = y * max_height / max_y;
            SetBottom(D, bottom);
        }

        /// <summary>Максимальное значение по горизонтали</summary>
        public static void SetMaxY(DependencyObject d, double value) => d.SetValue(MaxYProperty, value);

        /// <summary>Максимальное значение по горизонтали</summary>
        public static double GetMaxY(DependencyObject d) => (double)d.GetValue(MaxYProperty);

        #endregion

        #region Attached property ContainerHeight : double - Максимальная высота контейнера

        /// <summary>Максимальная высота контейнера</summary>
        public static readonly DependencyProperty ContainerHeightProperty =
            DependencyProperty.RegisterAttached(
                "ContainerHeight",
                typeof(double),
                typeof(RelPos),
                new PropertyMetadata(default(double), OnContainerHeightChanged));

        private static void OnContainerHeightChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var y = GetY(D);
            var max_y = GetMaxY(D);
            var max_height = (double)E.NewValue;
            if(max_height == 0) return;
            var bottom = y * max_height / max_y;
            SetBottom(D, bottom);
        }

        /// <summary>Максимальная высота контейнера</summary>
        public static void SetContainerHeight(DependencyObject d, double value) => d.SetValue(ContainerHeightProperty, value);

        /// <summary>Максимальная высота контейнера</summary>
        public static double GetContainerHeight(DependencyObject d) => (double)d.GetValue(ContainerHeightProperty);

        #endregion

        #region Attached property Bottom : double - Смещение по горизонтали

        /// <summary>Смещение по горизонтали</summary>
        public static readonly DependencyProperty BottomProperty =
            DependencyProperty.RegisterAttached(
                "Bottom",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(default(double), OnBottomChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnBottomChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var bottom = (double)E.NewValue;
            if(double.IsNaN(bottom)) return;
            var max_y = GetMaxY(D);
            var max_height = GetContainerHeight(D);
            if(max_height == 0) return;
            var y = bottom * max_y / max_height;
            SetY(D, y);
        }

        /// <summary>Смещение по горизонтали</summary>
        public static void SetBottom(DependencyObject d, double value) => d.SetValue(BottomProperty, value);

        /// <summary>Смещение по горизонтали</summary>
        public static double GetBottom(DependencyObject d) => (double)d.GetValue(BottomProperty);

        #endregion
    }
}
