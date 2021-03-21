using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace FDTD2DLab.Infrastructure.Behaviors
{
    public class MouseMove : Behavior<FrameworkElement>
    {
        #region Position : Point - Положение указателя мыши

        /// <summary>Положение указателя мыши</summary>
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(
                nameof(Position),
                typeof(Point),
                typeof(MouseMove),
                new PropertyMetadata(default(Point)));

        /// <summary>Положение указателя мыши</summary>
        //[Category("")]
        [Description("Положение указателя мыши")]
        public Point Position { get => (Point) GetValue(PositionProperty); set => SetValue(PositionProperty, value); }

        #endregion

        #region PositionRelative : Point - Положение указателя мыши

        /// <summary>Положение указателя мыши</summary>
        public static readonly DependencyProperty PositionRelativeProperty =
            DependencyProperty.Register(
                nameof(PositionRelative),
                typeof(Point),
                typeof(MouseMove),
                new PropertyMetadata(default(Point)));

        /// <summary>Положение указателя мыши</summary>
        //[Category("")]
        [Description("Положение указателя мыши")]
        public Point PositionRelative { get => (Point) GetValue(PositionRelativeProperty); set => SetValue(PositionRelativeProperty, value); }

        #endregion

        #region MouseDownCommand : ICommand - Нажатие кнопки мыши

        /// <summary>Нажатие кнопки мыши</summary>
        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register(
                nameof(MouseDownCommand),
                typeof(ICommand),
                typeof(MouseMove),
                new PropertyMetadata(default(ICommand)));

        /// <summary>Нажатие кнопки мыши</summary>
        //[Category("")]
        [Description("Нажатие кнопки мыши")]
        public ICommand MouseDownCommand { get => (ICommand) GetValue(MouseDownCommandProperty); set => SetValue(MouseDownCommandProperty, value); }

        #endregion

        #region MouseUpCommand : ICommand - Отпускание кнопки мыши

        /// <summary>Отпускание кнопки мыши</summary>
        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.Register(
                nameof(MouseUpCommand),
                typeof(ICommand),
                typeof(MouseMove),
                new PropertyMetadata(default(ICommand)));

        /// <summary>Отпускание кнопки мыши</summary>
        //[Category("")]
        [Description("Отпускание кнопки мыши")]
        public ICommand MouseUpCommand { get => (ICommand) GetValue(MouseUpCommandProperty); set => SetValue(MouseUpCommandProperty, value); }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseDown += OnMouseDown;
            AssociatedObject.MouseUp += OnMouseUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseMove -= OnMouseMove;
        }

        private void OnMouseMove(object Sender, MouseEventArgs E)
        {
            if(Sender is not FrameworkElement element) return;
            var position = E.GetPosition(element);
            Position = position;
            var width = element.ActualWidth;
            var height = element.ActualHeight;
            if(width <= 0 || height <= 0) return;
            PositionRelative = new Point(position.X / width, position.Y / height);
        }

        private void OnMouseDown(object Sender, MouseButtonEventArgs E)
        {
            if(Sender is not IInputElement element) return;
            var point = E.GetPosition(element);
            if (MouseDownCommand?.CanExecute(point) is true)
                MouseDownCommand?.Execute(point);
        }

        private void OnMouseUp(object Sender, MouseButtonEventArgs E)
        {
            if (Sender is not IInputElement element) return;
            var point = E.GetPosition(element);
            if (MouseUpCommand?.CanExecute(point) is true)
                MouseUpCommand?.Execute(point);
        }
    }
}
