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
    /// Логика взаимодействия для FieldView.xaml
    /// </summary>
    public partial class FieldView : UserControl
    {
        public FieldView()
        {
            InitializeComponent();
        }

        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DataContext is not FieldViewModel vm || vm.FrameCount == 0) return;
            int newFrame = (int)(e.NewValue * (vm.FrameCount - 1));
            if (newFrame != vm.CurrentFrameIndex)
                vm.CurrentFrameIndex = newFrame;
        }
    }
}
