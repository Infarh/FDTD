using FDTD2DLab.ViewModels;
using FDTD2DLab.ViewModels.Material;
using FDTD2DLab.ViewModels.Shapes;
using System.Windows;
using System.Windows.Input;

namespace FDTD2DLab.Infrastructure.Behaviors
{
    public static class DragDropBehavior
    {
        public static readonly DependencyProperty CanDragProperty =
            DependencyProperty.RegisterAttached("CanDrag", typeof(bool), typeof(DragDropBehavior), new PropertyMetadata(false, OnCanDragChanged));

        public static void SetCanDrag(UIElement element, bool value) => element.SetValue(CanDragProperty, value);
        public static bool GetCanDrag(UIElement element) => (bool)element.GetValue(CanDragProperty);

        private static void OnCanDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                    element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                else
                    element.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            }
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element?.DataContext is MaterialViewModel material)
            {
                DragDrop.DoDragDrop(element, material, DragDropEffects.Copy);
                e.Handled = true;
            }
        }

        public static readonly DependencyProperty CanDropProperty =
            DependencyProperty.RegisterAttached("CanDrop", typeof(bool), typeof(DragDropBehavior), new PropertyMetadata(false, OnCanDropChanged));

        public static void SetCanDrop(UIElement element, bool value) => element.SetValue(CanDropProperty, value);
        public static bool GetCanDrop(UIElement element) => (bool)element.GetValue(CanDropProperty);

        private static void OnCanDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.AllowDrop = (bool)e.NewValue;
                if ((bool)e.NewValue)
                {
                    element.Drop += OnDrop;
                    element.PreviewDragOver += OnPreviewDragOver;
                }
                else
                {
                    element.Drop -= OnDrop;
                    element.PreviewDragOver -= OnPreviewDragOver;
                }
            }
        }

        private static void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            var target = sender as FrameworkElement;
            var material = e.Data.GetData(typeof(MaterialViewModel)) as MaterialViewModel;
            if (target == null || material == null) return;

            // Если цель - фигура
            if (target.DataContext is ShapeViewModel shape)
            {
                shape.AppliedMaterial = material;
                e.Handled = true;
            }
            // Если цель - фон (Border, DataContext которого может быть MainWindowViewModel или GridViewModel)
            else if (target.DataContext is MainWindowViewModel mainVM)
            {
                mainVM.Grid.BackgroundMaterial = material;
                e.Handled = true;
            }
            else if (target.DataContext is GridViewModel grid)
            {
                grid.BackgroundMaterial = material;
                e.Handled = true;
            }
        }
    }
}

