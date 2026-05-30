using FDTD2DLab.ViewModels;
using FDTD2DLab.ViewModels.Probe;
using System.Windows.Controls;
using System.Windows.Input;

namespace FDTD2DLab;

public partial class MainWindow
{
    public MainWindow() => InitializeComponent();

    private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {

    }

    private void ProbesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if ((sender as ListBox)?.SelectedItem is ProbeViewModel probe)
        {
            (DataContext as MainWindowViewModel)?.OpenProbeTabCommand.Execute(probe);
        }
    }

}
