using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MathCore.WPF.Converters;

namespace FDTD2DLab.Infrastructure
{
    public class RelPos
    {
        #region Attached property Binding : bool - Инициализация связи с канвой

        /// <summary>Инициализация связи с канвой</summary>
        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.RegisterAttached(
                "Binding",
                typeof(bool),
                typeof(RelPos),
                new PropertyMetadata(default(bool), OnBinding));

        /// <summary>Инициализация связи с канвой</summary>
        public static void SetBinding(DependencyObject d, bool value) => d.SetValue(BindingProperty, value);

        /// <summary>Инициализация связи с канвой</summary>
        public static bool GetBinding(DependencyObject d) => (bool)d.GetValue(BindingProperty);

        private static readonly Divide __Div2Neg = new(-2);
        private static void OnBinding(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            if ((bool)E.NewValue)
            {
                BindingOperations.SetBinding(D, LeftProperty, new Binding("(Canvas.Left)")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                });
                BindingOperations.SetBinding(D, BottomProperty, new Binding("(Canvas.Bottom)")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                });

                BindingOperations.SetBinding(D, WidthProperty, new Binding("Width") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
                BindingOperations.SetBinding(D, HeightProperty, new Binding("Height") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });

                if (D is FrameworkElement element)
                {
                    element.AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
                }
            }
            else
            {
                BindingOperations.ClearBinding(D, LeftProperty);
                BindingOperations.ClearBinding(D, BottomProperty);

                if (D is FrameworkElement element)
                {
                    element.RemoveHandler(Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMoveElement));
                    element.RemoveHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
                    element.RemoveHandler(Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
                }
            }
        }

        private const double __EdgeWidth = 4;

        private static void OnMouseDown(object Sender, MouseButtonEventArgs E)
        {
            if (Sender is not FrameworkElement element) return;
            if (VisualTreeHelper.GetParent(element) is not Canvas canvas) return;

            var point_in_canvas = E.GetPosition(canvas);
            var point_in_element = E.GetPosition(element);
            var element_width = (double)element.GetValue(FrameworkElement.WidthProperty);
            var element_height = (double)element.GetValue(FrameworkElement.HeightProperty);

            SetMouseDownPoint(element, point_in_canvas);
            Mouse.Capture(element);
            element.AddHandler(Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));

            var is_left_edge = point_in_element.X < __EdgeWidth;
            var is_right_edge = element_width - point_in_element.X < __EdgeWidth;
            var is_top_edge = point_in_element.Y < __EdgeWidth;
            var is_bottom_edge = element_height - point_in_element.Y < __EdgeWidth;

            switch ((is_left_edge, is_top_edge, is_right_edge, is_bottom_edge))
            {
                case (true, false, false, false): // левая граница
                    break;

                case (true, true, false, false): // левый верхний угол
                    break;

                case (false, true, false, false): // верхняя граница
                    break;

                case (false, true, true, false): // верхний правый угол
                    break;

                case (false, false, true, false): // правая граница
                    //StartResizing
                    break;

                case (false, false, true, true): // правый нижний угол
                    break;

                case (false, false, false, true): // нижняя граница
                    break;

                case (true, false, false, true): // правый нижний угол
                    break;

                default:
                    StartElementDragMove(element);
                    break;
            }
        }

        private static void StartElementDragMove(FrameworkElement element)
        {
            element.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMoveElement));

            var canvas_left = (double)element.GetValue(Canvas.LeftProperty);
            var canvas_bottom = (double)element.GetValue(Canvas.BottomProperty);
            SetStartPositionPoint(element, new(canvas_left, canvas_bottom));

            element.SetValue(FrameworkElement.CursorProperty, Cursors.ScrollAll);
        }

        private static void OnMouseUp(object Sender, MouseButtonEventArgs E)
        {
            if (Sender is not FrameworkElement element) return;
            if (VisualTreeHelper.GetParent(element) is not Canvas canvas) return;

            SetMouseDownPoint(element, null);
            SetStartPositionPoint(element, null);
            element.SetValue(FrameworkElement.CursorProperty, DependencyProperty.UnsetValue);

            Mouse.Capture(element, CaptureMode.None);

            element.RemoveHandler(Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMoveElement));
            element.RemoveHandler(Mouse.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
        }

        public static void OnMouseMoveElement(object Sender, MouseEventArgs E)
        {
            if (Sender is not FrameworkElement element) return;
            if (GetMouseDownPoint(element) is not { } start_point) return;
            if (GetStartPositionPoint(element) is not { } canvas_start_point) return;
            if (VisualTreeHelper.GetParent(element) is not Canvas canvas) return;

            var current_point = E.GetPosition(canvas);

            var delta = current_point.Sub(start_point);
            var current_canvas_point = new Point(canvas_start_point.X + delta.X, canvas_start_point.Y - delta.Y);

            element.SetValue(Canvas.LeftProperty, current_canvas_point.X);
            element.SetValue(Canvas.BottomProperty, current_canvas_point.Y);
        }

        #endregion

        /* ----------------------------------------------------------------------------------------------- */

        #region Attached property MouseDownPoint : Point - Точка нажатия мыши

        /// <summary>Точка нажатия мыши</summary>
        public static readonly DependencyProperty MouseDownPointProperty =
            DependencyProperty.RegisterAttached(
                "MouseDownPoint",
                typeof(Point?),
                typeof(RelPos),
                new PropertyMetadata(default(Point?)));

        /// <summary>Точка нажатия мыши</summary>
        public static void SetMouseDownPoint(DependencyObject d, Point? value) => d.SetValue(MouseDownPointProperty, value);

        /// <summary>Точка нажатия мыши</summary>
        public static Point? GetMouseDownPoint(DependencyObject d) => (Point?)d.GetValue(MouseDownPointProperty);

        #endregion

        #region Attached property StartPositionPoint : Point - Точка нажатия мыши

        /// <summary>Точка нажатия мыши</summary>
        public static readonly DependencyProperty StartPositionPointProperty =
            DependencyProperty.RegisterAttached(
                "StartPositionPoint",
                typeof(Point?),
                typeof(RelPos),
                new PropertyMetadata(default(Point?)));

        /// <summary>Точка нажатия мыши</summary>
        public static void SetStartPositionPoint(DependencyObject d, Point? value) => d.SetValue(StartPositionPointProperty, value);

        /// <summary>Точка нажатия мыши</summary>
        public static Point? GetStartPositionPoint(DependencyObject d) => (Point)d.GetValue(StartPositionPointProperty);

        #endregion

        /* ----------------------------------------------------------------------------------------------- */

        #region Attached property X : double - Физическое значение по горизонтали

        /// <summary>Физическое значение по горизонтали</summary>
        public static readonly DependencyProperty XProperty =
            DependencyProperty.RegisterAttached(
                "X",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(1d, OnXChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnXChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var x = (double)E.NewValue;
            var max_x = GetMaxX(D);
            var max_width = GetContainerWidth(D);

            var left = x * max_width / max_x;
            if (left is not double.NaN)
                SetLeft(D, left);
        }

        /// <summary>Физическое значение по горизонтали</summary>
        public static void SetX(DependencyObject d, double value) => d.SetValue(XProperty, value);

        /// <summary>Физическое значение по горизонтали</summary>
        public static double GetX(DependencyObject d) => (double)d.GetValue(XProperty);

        #endregion

        #region Attached property ValueWidth : double - Физическое значение ширины

        /// <summary>Физическое значение ширины</summary>
        public static readonly DependencyProperty ValueWidthProperty =
            DependencyProperty.RegisterAttached(
                "ValueWidth",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(default(double), OnValueWidthChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnValueWidthChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var value_width = (double)E.NewValue;
            var max_x = GetMaxX(D);
            var max_width = GetContainerWidth(D);

            var width = value_width * max_width / max_x;
            if (width is not double.NaN)
                SetWidth(D, width);
        }

        /// <summary>Физическое значение ширины</summary>
        public static void SetValueWidth(DependencyObject d, double value) => d.SetValue(ValueWidthProperty, value);

        /// <summary>Физическое значение ширины</summary>
        public static double GetValueWidth(DependencyObject d) => (double)d.GetValue(ValueWidthProperty);

        #endregion

        #region Attached property MaxX : double - Максимальное физическое значение по горизонтальной оси в контейнере

        /// <summary>Максимальное физическое значение по горизонтальной оси в контейнере</summary>
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

            var left = x * max_width / max_x;
            if (left is not double.NaN)
                SetLeft(D, left);

            var value_width = GetValueWidth(D);
            var width = value_width * max_width / max_x;
            if (width is not double.NaN)
                SetWidth(D, width);
        }

        /// <summary>Максимальное физическое значение по горизонтальной оси в контейнере</summary>
        public static void SetMaxX(DependencyObject d, double value) => d.SetValue(MaxXProperty, value);

        /// <summary>Максимальное физическое значение по горизонтальной оси в контейнере</summary>
        public static double GetMaxX(DependencyObject d) => (double)d.GetValue(MaxXProperty);

        #endregion

        #region Attached property ContainerWidth : double - Максимальная ширина визуального контейнера

        /// <summary>Максимальная ширина визуального контейнера</summary>
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

            var left = x * max_width / max_x;
            if (left is not double.NaN)
                SetLeft(D, left);

            var value_width = GetValueWidth(D);
            var width = value_width * max_width / max_x;
            if (width is not double.NaN)
                SetWidth(D, width);
        }

        /// <summary>Максимальная ширина визуального контейнера</summary>
        public static void SetContainerWidth(DependencyObject d, double value) => d.SetValue(ContainerWidthProperty, value);

        /// <summary>Максимальная ширина визуального контейнера</summary>
        public static double GetContainerWidth(DependencyObject d) => (double)d.GetValue(ContainerWidthProperty);

        #endregion

        #region Attached property Left : double - Экранное смещение по горизонтали

        /// <summary>Экранное смещение по горизонтали</summary>
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
            var max_x = GetMaxX(D);
            var max_width = GetContainerWidth(D);

            var x = left * max_x / max_width;
            if (x is not double.NaN)
                SetX(D, x);
        }

        /// <summary>Экранное смещение по горизонтали</summary>
        public static void SetLeft(DependencyObject d, double value) => d.SetValue(LeftProperty, value);

        /// <summary>Экранное смещение по горизонтали</summary>
        public static double GetLeft(DependencyObject d) => (double)d.GetValue(LeftProperty);

        #endregion

        #region Attached property Width : double - Экранное значение ширины

        /// <summary>Экранное значение ширины</summary>
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.RegisterAttached(
                "Width",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(default(double), OnWidthChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnWidthChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var width = (double)E.NewValue;
            var max_x = GetMaxX(D);
            var max_width = GetContainerWidth(D);

            var value_width = width * max_x / max_width;
            if (value_width is not double.NaN)
                SetValueWidth(D, value_width);
        }

        /// <summary>Экранное значение ширины</summary>
        public static void SetWidth(DependencyObject d, double value) => d.SetValue(WidthProperty, value);

        /// <summary>Экранное значение ширины</summary>
        public static double GetWidth(DependencyObject d) => (double)d.GetValue(WidthProperty);

        #endregion

        /* ----------------------------------------------------------------------------------------------- */

        #region Attached property Y : double - Физическое значение по вертикали

        /// <summary>Физическое значение по вертикали</summary>
        public static readonly DependencyProperty YProperty =
            DependencyProperty.RegisterAttached(
                "Y",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(default(double), OnYChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnYChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var y = (double)E.NewValue;
            var max_y = GetMaxY(D);
            var max_height = GetContainerHeight(D);

            var bottom = y * max_height / max_y;
            if (bottom is not double.NaN)
                SetBottom(D, bottom);
        }

        /// <summary>Физическое значение по вертикали</summary>
        public static void SetY(DependencyObject d, double value) => d.SetValue(YProperty, value);

        /// <summary>Физическое значение по вертикали</summary>
        public static double GetY(DependencyObject d) => (double)d.GetValue(YProperty);

        #endregion

        #region Attached property ValueHeight : double - Физическое значение высоты

        /// <summary>Физическое значение высоты</summary>
        public static readonly DependencyProperty ValueHeightProperty =
            DependencyProperty.RegisterAttached(
                "ValueHeight",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(default(double), OnValueHeightChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnValueHeightChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var value_height = (double)E.NewValue;
            var max_y = GetMaxY(D);
            var max_height = GetContainerHeight(D);

            var height = value_height * max_height / max_y;
            if (height is not double.NaN)
                SetHeight(D, height);
        }

        /// <summary>Физическое значение высоты</summary>
        public static void SetValueHeight(DependencyObject d, double value) => d.SetValue(ValueHeightProperty, value);

        /// <summary>Физическое значение высоты</summary>
        public static double GetValueHeight(DependencyObject d) => (double)d.GetValue(ValueHeightProperty);

        #endregion

        #region Attached property MaxY : double - Максимальное физическое значение по вертикальной оси в контейнере

        /// <summary>Максимальное физическое значение по вертикальной оси в контейнере</summary>
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

            var bottom = y * max_height / max_y;
            if (bottom is not double.NaN)
                SetBottom(D, bottom);

            var value_height = GetValueHeight(D);
            var height = value_height * max_height / max_y;
            if (height is not double.NaN)
                SetHeight(D, height);
        }

        /// <summary>Максимальное физическое значение по вертикальной оси в контейнере</summary>
        public static void SetMaxY(DependencyObject d, double value) => d.SetValue(MaxYProperty, value);

        /// <summary>Максимальное физическое значение по вертикальной оси в контейнере</summary>
        public static double GetMaxY(DependencyObject d) => (double)d.GetValue(MaxYProperty);

        #endregion

        #region Attached property ContainerHeight : double - Максимальная высота визуального контейнера

        /// <summary>Максимальная высота визуального контейнера</summary>
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

            var bottom = y * max_height / max_y;
            if (bottom is not double.NaN)
                SetBottom(D, bottom);

            var value_height = GetValueHeight(D);
            var height = value_height * max_height / max_y;
            if (height is not double.NaN)
                SetHeight(D, height);
        }

        /// <summary>Максимальная высота визуального контейнера</summary>
        public static void SetContainerHeight(DependencyObject d, double value) => d.SetValue(ContainerHeightProperty, value);

        /// <summary>Максимальная высота визуального контейнера</summary>
        public static double GetContainerHeight(DependencyObject d) => (double)d.GetValue(ContainerHeightProperty);

        #endregion

        #region Attached property Bottom : double - Экранное смещение по горизонтали

        /// <summary>Экранное смещение по горизонтали</summary>
        public static readonly DependencyProperty BottomProperty =
            DependencyProperty.RegisterAttached(
                "Bottom",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(default(double), OnBottomChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnBottomChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var bottom = (double)E.NewValue;
            var max_y = GetMaxY(D);
            var max_height = GetContainerHeight(D);

            var y = bottom * max_y / max_height;
            if (y is not double.NaN)
                SetY(D, y);
        }

        /// <summary>Экранное смещение по горизонтали</summary>
        public static void SetBottom(DependencyObject d, double value) => d.SetValue(BottomProperty, value);

        /// <summary>Экранное смещение по горизонтали</summary>
        public static double GetBottom(DependencyObject d) => (double)d.GetValue(BottomProperty);

        #endregion

        #region Attached property Height : double - Экранное значение ширины

        /// <summary>Экранное значение ширины</summary>
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.RegisterAttached(
                "Height",
                typeof(double),
                typeof(RelPos),
                new FrameworkPropertyMetadata(default(double), OnHeightChanged) { BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

        private static void OnHeightChanged(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var height = (double)E.NewValue;
            var max_y = GetMaxY(D);
            var max_height = GetContainerHeight(D);

            var value_height = height * max_y / max_height;
            if (value_height is not double.NaN)
                SetValueHeight(D, value_height);
        }

        /// <summary>Экранное значение ширины</summary>
        public static void SetHeight(DependencyObject d, double value) => d.SetValue(HeightProperty, value);

        /// <summary>Экранное значение ширины</summary>
        public static double GetHeight(DependencyObject d) => (double)d.GetValue(HeightProperty);

        #endregion
    }
}
