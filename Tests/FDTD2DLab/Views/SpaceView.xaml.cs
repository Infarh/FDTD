using FDTD2DLab.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FDTD2DLab.Views
{
    /// <summary>
    /// Логика взаимодействия для SpaceView.xaml
    /// </summary>
    public partial class SpaceView : UserControl
    {
        public SpaceView(MainWindowViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // Перемещаем фокус на ближайший родительский элемент, не являющийся TextBox
                var focusedElement = Keyboard.FocusedElement as UIElement;
                focusedElement?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                // Вместо этого можно просто сбросить фокус:
                // Keyboard.ClearFocus();
                e.Handled = true;
            }
        }

        private void DrawingArea_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Снимаем фокус с любого активного элемента
            Keyboard.ClearFocus();
        }
    }
}
