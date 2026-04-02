using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDTD2DLab.ViewModels.Source
{
    public class PlaneWaveSourceViewModel : SourceViewModel
    {
        private double _position;
        private double _start;
        private double _end;
        private bool _isHorizontal;

        public double Position { get => _position; set => SetField(ref _position, value, OnPositionChanged); }
        public double Start { get => _start; set => SetField(ref _start, value, OnStartEndChanged); }
        public double End { get => _end; set => SetField(ref _end, value, OnStartEndChanged); }
        public bool IsHorizontal { get => _isHorizontal; set => SetField(ref _isHorizontal, value, OnIsHorizontalChanged); }

        // Свойства для привязки в RelPos
        public double X
        {
            get => IsHorizontal ? Start : Position;
            set
            {
                if (IsHorizontal) Start = value;
                else Position = value;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => IsHorizontal ? Position : Start;
            set
            {
                if (IsHorizontal) Position = value;
                else Start = value;
                OnPropertyChanged();
            }
        }

        public double ValueWidth => IsHorizontal ? End - Start : 0.05;   // толщина линии 0.05 (видимая)
        public double ValueHeight => IsHorizontal ? 0.05 : End - Start;

        private void OnStartEndChanged()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(ValueWidth));
            OnPropertyChanged(nameof(ValueHeight));
        }

        private void OnPositionChanged()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }

        private void OnIsHorizontalChanged()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(ValueWidth));
            OnPropertyChanged(nameof(ValueHeight));
        }

        public override string TypeDisplayName => "Плоская волна";

        // --- Вспомогательные методы для уведомлений ---

    }
}
