using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FDTD2DLab.ViewModels
{
    public class SimulationSettingsViewModel : ViewModel
    {
        private readonly MainWindowViewModel _main;

        public SimulationSettingsViewModel(MainWindowViewModel main)
        {
            _main = main;
            var grid = main.Grid;

            // Заполняем информацию о сетке
            GridInfo = $"{grid.Nx} × {grid.Ny}";
            GridInfoDx = grid.dx;
            GridInfoDy = grid.dy;
            GridInfoDt = grid.dt;
            MaxStableDt = grid.MaxDt;
            GridInfoLength = $"{grid.Lx:F2} × {grid.Ly:F2}";

            // Рекомендуемое время: чтобы волна прошла примерно 10 длин области (Lx / c)
            double c = 299792458;
            double recommendedTime = 10 * grid.Lx / c;
            RecommendedTime = recommendedTime;

            // По умолчанию время моделирования = 1000 * dt
            SimulationTime = 1000 * grid.dt;
            SleepDelayMs = 30; // задержка по умолчанию

            OkCommand = new LambdaCommand(() =>
            {
                if (Validate())
                {
                    // Закрываем окно с успехом
                    if (System.Windows.Application.Current.Windows.OfType<Window>()
                            .FirstOrDefault(w => w.DataContext == this) is Window window)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, введите корректное время моделирования (положительное число).",
                                    "Некорректный ввод", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            });

            CalculateDefaultCommand = new LambdaCommand(() =>
            {
                SimulationTime = RecommendedTime;
            });
        }

        // --- Свойства для отображения сетки ---
        private string _gridInfo;
        public string GridInfo { get => _gridInfo; set => Set(ref _gridInfo, value); }

        private double _gridInfoDx;
        public double GridInfoDx { get => _gridInfoDx; set => Set(ref _gridInfoDx, value); }

        private double _gridInfoDy;
        public double GridInfoDy { get => _gridInfoDy; set => Set(ref _gridInfoDy, value); }

        private double _gridInfoDt;
        public double GridInfoDt { get => _gridInfoDt; set => Set(ref _gridInfoDt, value); }

        private double _maxStableDt;
        public double MaxStableDt { get => _maxStableDt; set => Set(ref _maxStableDt, value); }

        private string _gridInfoLength;
        public string GridInfoLength { get => _gridInfoLength; set => Set(ref _gridInfoLength, value); }

        // --- Настройки моделирования ---
        private double _simulationTime;
        public double SimulationTime
        {
            get => _simulationTime;
            set
            {
                if (Set(ref _simulationTime, value))
                {
                    OnPropertyChanged(nameof(TimeStepsInfo));
                }
            }
        }

        private int _sleepDelayMs = 30;
        public int SleepDelayMs { get => _sleepDelayMs; set => Set(ref _sleepDelayMs, value); }

        private double _recommendedTime;
        public double RecommendedTime { get => _recommendedTime; set => Set(ref _recommendedTime, value); }

        // Вычисляемое количество шагов
        public string TimeStepsInfo
        {
            get
            {
                if (_main?.Grid == null || _simulationTime <= 0) return "—";
                int steps = (int)(_simulationTime / _main.Grid.dt);
                return steps.ToString("N0");
            }
        }

        public ICommand OkCommand { get; }
        public ICommand CalculateDefaultCommand { get; }

        public bool Validate() => _simulationTime > 0;
    }
}