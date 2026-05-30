using FDTD2DLab.ViewModels.Propertys;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FDTD2DLab.ViewModels.Material
{
    public class MaterialViewModel : INotifyPropertyChanged, IOptProperty
    {
        private string _name;
        private double _eps = 1.0;
        private double _mu = 1.0;
        private double _sigma = 0.0;

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public double Eps
        {
            get => _eps;
            set => SetField(ref _eps, value);
        }

        public double Mu
        {
            get => _mu;
            set => SetField(ref _mu, value);
        }

        public double Sigma
        {
            get => _sigma;
            set => SetField(ref _sigma, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

