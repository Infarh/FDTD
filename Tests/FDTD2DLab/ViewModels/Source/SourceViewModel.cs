using FDTD2DLab.ViewModels.Propertys;
using MathCore.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FDTD2DLab.ViewModels.Source
{
    public enum SignalType
    {
        Gaussian,   // гауссов импульс: Exp(-((t-t0)/tau)^2)
        Sine        // синусоида: sin(2πf t + φ)
    }

    public abstract class SourceViewModel : ViewModel, INotifyPropertyChanged, IOptProperty
    {
        private string _name = "Источник";
        private SignalType _signalType = SignalType.Gaussian;
        private double _amplitude = 1.0;
        private double _t0 = 0.0;          // задержка (с)
        private double _tau = 1e-9;         // длительность импульса (с) для Gaussian
        private double _frequency = 1e9;    // частота (Гц) для Sine
        private double _phase = 0.0;         // фаза (рад) для Sine

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public SignalType SignalType
        {
            get => _signalType;
            set => SetField(ref _signalType, value);
        }

        public double Amplitude
        {
            get => _amplitude;
            set => SetField(ref _amplitude, value);
        }

        public double T0
        {
            get => _t0;
            set => SetField(ref _t0, value);
        }

        public double Tau
        {
            get => _tau;
            set => SetField(ref _tau, value);
        }

        public double Frequency
        {
            get => _frequency;
            set => SetField(ref _frequency, value);
        }

        public double Phase
        {
            get => _phase;
            set => SetField(ref _phase, value);
        }

        // Функция времени, возвращающая значение сигнала
        public Func<double, double> GetSignalFunction()
        {
            switch (SignalType)
            {
                case SignalType.Gaussian:
                    return t => Amplitude * Math.Exp(-Math.Pow((t - T0) / Tau, 2));
                case SignalType.Sine:
                    return t => Amplitude * Math.Sin(2 * Math.PI * Frequency * t + Phase);
                default:
                    return t => 0;
            }
        }

        public abstract string TypeDisplayName { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class PointSourceViewModel : SourceViewModel
    {
        private double _x;
        private double _y;

        public double X
        {
            get => _x;
            set => SetField(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetField(ref _y, value);
        }

        public override string TypeDisplayName => "Точечный источник";
    }

    public class PlaneWaveSourceViewModel : SourceViewModel
    {
        private double _position;      // координата линии (например, x = const)
        private double _start;         // начало линии по другой оси
        private double _end;           // конец линии
        private bool _isHorizontal;    // true – горизонтальная линия (постоянная Y), false – вертикальная (постоянная X)

        public double Position
        {
            get => _position;
            set => SetField(ref _position, value);
        }

        public double Start
        {
            get => _start;
            set => SetField(ref _start, value);
        }

        public double End
        {
            get => _end;
            set => SetField(ref _end, value);
        }

        public bool IsHorizontal
        {
            get => _isHorizontal;
            set => SetField(ref _isHorizontal, value);
        }

        public override string TypeDisplayName => "Плоская волна";
    }
}