using System.Windows;
using FDTD2DLab.ViewModels;

namespace FDTD2DLab.Views
{
    public partial class SimulationSettingsDialog : Window
    {
        public SimulationSettingsDialog(MainWindowViewModel mainViewModel)
        {
            InitializeComponent();
            DataContext = new SimulationSettingsViewModel(mainViewModel);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SimulationSettingsViewModel vm && vm.Validate())
                DialogResult = true;
            else
                MessageBox.Show("Пожалуйста, введите корректное время моделирования (положительное число).",
                                "Некорректный ввод", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}