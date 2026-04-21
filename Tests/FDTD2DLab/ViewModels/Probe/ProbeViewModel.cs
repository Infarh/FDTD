using FDTD2DLab.ViewModels.Propertys;
using FDTD2DLab.ViewModels.Shapes;
using MathCore.WPF.ViewModels;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace FDTD2DLab.ViewModels.Probe
{
    public enum FieldComponent
    {
        Ex, Ey, Ez, Hx, Hy, Hz
    }
    [JsonDerivedType(typeof(ProbeViewModel))]
    public class ProbeViewModel : ViewModel, INotifyPropertyChanged, IOptProperty
    {

        [JsonIgnore]
        private Type _ProbeType;
        [JsonIgnore]
        public Type ProbeType { get => _ProbeType; set => Set(ref _ProbeType, value); }

        private string _name;
        private FieldComponent _component = FieldComponent.Ez;
        private readonly List<double> _timeValues = new();
        private readonly List<double> _fieldValues = new();
        private PlotModel _plotModel;

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        #region X : double - Положение по горизонтали

        /// <summary>Положение по горизонтали</summary>
        private double _X;

        /// <summary>Положение по горизонтали</summary>
        public double X { get => _X; set => Set(ref _X, value); }

        #endregion

        #region Y : double - Положение по вертикали

        /// <summary>Положение по вертикали</summary>
        private double _Y;

        /// <summary>Положение по вертикали</summary>
        public double Y { get => _Y; set => Set(ref _Y, value); }

        #endregion

        #region IsSelected : bool - Модель выбрана

        /// <summary>Модель выбрана</summary>
        private bool _IsSelected;

        /// <summary>Модель выбрана</summary>
        public bool IsSelected { get => _IsSelected; set => Set(ref _IsSelected, value); }

        #endregion

        //#region Width : double - Размер

        ///// <summary>Размер</summary>
        //private double _Width = 10;

        ///// <summary>Размер</summary>
        //public double Width { get => _Width; set => Set(ref _Width, value); }

        //#endregion

        //#region Height : double - Размер

        ///// <summary>Размер</summary>
        //private double _Height = 10;

        ///// <summary>Размер</summary>
        //public double Height { get => _Height; set => Set(ref _Height, value); }

        //#endregion

        public FieldComponent Component
        {
            get => _component;
            set => SetField(ref _component, value);
        }

        public IReadOnlyList<double> TimeValues => _timeValues;
        public IReadOnlyList<double> FieldValues => _fieldValues;

        public PlotModel PlotModel
        {
            get
            {
                if (_plotModel == null && _timeValues.Count > 0)
                    UpdatePlotModel();
                return _plotModel;
            }
            private set => SetField(ref _plotModel, value);
        }

        public void AddSample(double time, double value)
        {
            _timeValues.Add(time);
            _fieldValues.Add(value);
        }

        public void ClearData()
        {
            _timeValues.Clear();
            _fieldValues.Clear();
            PlotModel = null;
        }

        public void UpdatePlotModel()
        {
            var model = new PlotModel { Title = Name };
            var series = new LineSeries { Title = Component.ToString() };
            for (int i = 0; i < _timeValues.Count; i++)
                series.Points.Add(new DataPoint(_timeValues[i], _fieldValues[i]));
            model.Series.Add(series);
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, Title = "Время (с)" });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, Title = Component.ToString() });
            PlotModel = model;
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