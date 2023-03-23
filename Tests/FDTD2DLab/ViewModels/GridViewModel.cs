using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using FDTD2DLab.Infrastructure.Extensions;
using FDTD2DLab.ViewModels.Shapes;

using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;

namespace FDTD2DLab.ViewModels;

public class GridViewModel : ViewModel
{
    public GridViewModel(MainWindowViewModel MainModel)
    {
        this.MainModel = MainModel;
        //Shapes.CollectionChanged += (_, e) =>
        Shapes.OnItems().Changed(nameof(ShapeViewModel.IsSelected), OnChangedIsSelectedChanged);
        UpdateGridX();
        UpdateGridY();
    }

    public MainWindowViewModel MainModel { get; }


    #region GridMousePosition : Point - Положение мыши в сетке пространства

    /// <summary>Положение мыши в сетке пространства</summary>
    private Point _GridMousePosition;

    /// <summary>Положение мыши в сетке пространства</summary>
    public Point GridMousePosition { get => _GridMousePosition; set => Set(ref _GridMousePosition, value); }

    [DependencyOn(nameof(Lx))]
    [DependencyOn(nameof(Ly))]
    [DependencyOn(nameof(GridMousePosition))]
    public Point MousePosition => new(Lx * _GridMousePosition.X, Ly * (1 - _GridMousePosition.Y));

    [DependencyOn(nameof(dx))]
    [DependencyOn(nameof(dy))]
    [DependencyOn(nameof(MousePosition))]
    public Point MousePositionCell => new(Math.Ceiling(Lx * _GridMousePosition.X / _dx) - 1, Math.Ceiling(Ly * (1 - _GridMousePosition.Y) / _dy) - 1);

    #endregion

    #region Nx : int - Размер сетки по горизонтали

    /// <summary>Размер сетки по горизонтали</summary>
    private int _Nx = 300;

    /// <summary>Размер сетки по горизонтали</summary>
    public int Nx { get => _Nx; set => SetValue(ref _Nx, value, n => n > 2).Then(UpdateGridX); }

    #endregion

    #region Ny : int - Размер сетки по вертикали

    /// <summary>Размер сетки по вертикали</summary>
    private int _Ny = 100;

    /// <summary>Размер сетки по вертикали</summary>
    public int Ny { get => _Ny; set => SetValue(ref _Ny, value, n => n > 2).Then(UpdateGridY); }

    #endregion

    #region dx : double - Шаг сетки по горизонтали

    /// <summary>Шаг сетки по горизонтали</summary>
    private double _dx = 1;

    /// <summary>Шаг сетки по горизонтали</summary>
    public double dx { get => _dx; set => SetValue(ref _dx, value, d => d > 0).Then(UpdateGridX); }

    #endregion

    #region dy : double - Шаг сетки по вертикали

    /// <summary>Шаг сетки по вертикали</summary>
    private double _dy = 1;

    /// <summary>Шаг сетки по вертикали</summary>
    public double dy { get => _dy; set => SetValue(ref _dy, value, d => d > 0).Then(UpdateGridY); }

    #endregion

    #region SpaceUnit : string - Единица измерения


    /// <summary>Единица измерения</summary>
    private string _SpaceUnit = "м";

    /// <summary>Единица измерения</summary>
    public string SpaceUnit { get => _SpaceUnit; set => Set(ref _SpaceUnit, value, v => SpaceUnits.Contains(v)); }

    public IReadOnlyCollection<string> SpaceUnits { get; } = new[] { "м", "см", "мм" };

    [DependencyOn(nameof(SpaceUnit))]
    public double SpaceUnitValue => SpaceUnit switch
    {
        "м" => 1,
        "см" => 0.01,
        "мм" => 0.001,
        null => throw new NotSupportedException("Единицы измерения пространства не заданы"),
        _ => throw new NotSupportedException($"Неизвестный тип единиц измерения {SpaceUnit}")
    };

    #endregion

    [DependencyOn(nameof(Nx))]
    [DependencyOn(nameof(dx))]
    public double Lx => _Nx * _dx;

    [DependencyOn(nameof(Ny))]
    [DependencyOn(nameof(dy))]
    public double Ly => _Ny * _dy;

    [DependencyOn(nameof(Nx))]
    [DependencyOn(nameof(Ny))]
    public int CellCount => _Nx * _Ny;

    #region GridX : IEnumerable<double> - Сетка по оси X

    /// <summary>Сетка по оси X</summary>
    private IEnumerable<double> _GridX;

    /// <summary>Сетка по оси X</summary>
    public IEnumerable<double> GridX { get => _GridX; private set => Set(ref _GridX, value); }

    private void UpdateGridX() => GridX = Enumerable.Range(1, _Nx - 1).Select(i => i * _dx).ToArray();

    #endregion

    #region GridY : IEnumerable<double> - Сетка по оси Y

    /// <summary>Сетка по оси Y</summary>
    private IEnumerable<double> _GridY;

    /// <summary>Сетка по оси Y</summary>
    public IEnumerable<double> GridY { get => _GridY; private set => Set(ref _GridY, value); }

    private void UpdateGridY() => GridY = Enumerable.Range(1, _Ny - 1).Select(i => i * _dy).ToArray();

    #endregion

    private ObservableCollection<ShapeViewModel> _Shapes = new()
    {
        new RectViewModel
        {
            Width = 50,
            Height = 50,
            X = 125,
            Y = 50,
            IsSelected = true
        },
        new EllipseViewModel
        {
            Width = 60,
            Height = 20,
            X = 50,
            Y = 40,
        },
    };

    private CollectionItemsChangeTracker<ObservableCollection<ShapeViewModel>> _ShapesTracker;

    public ObservableCollection<ShapeViewModel> Shapes
    {
        get => _Shapes;
        set
        {
            if (!Set(ref _Shapes, value, out var old_shapes)) return;
            _ShapesTracker?.Dispose();
            var tracker = value.OnItems();
            tracker?.Changed(nameof(ShapeViewModel.IsSelected), OnChangedIsSelectedChanged);
            _ShapesTracker = tracker;
        }
    }

    private void OnChangedIsSelectedChanged(object Shape) => SelectedShape = (ShapeViewModel)Shape;

    #region Command SetShapeCommandCommand - Выбор элемента сетки

    /// <summary>Выбор элемента сетки</summary>
    private LambdaCommand<ShapeViewModel> _SetShapeCommandCommand;

    /// <summary>Выбор элемента сетки</summary>
    public ICommand SetShapeCommandCommand => _SetShapeCommandCommand
        ??= new(OnSetShapeCommandCommandExecuted, CanSetShapeCommandCommandExecute);

    /// <summary>Проверка возможности выполнения - Выбор элемента сетки</summary>
    private bool CanSetShapeCommandCommandExecute(ShapeViewModel Shape) => Shapes.Contains(Shape);

    /// <summary>Логика выполнения - Выбор элемента сетки</summary>
    private void OnSetShapeCommandCommandExecuted(ShapeViewModel Shape) => SelectedShape = Shape;

    #endregion

    #region Command UnSetShapeCommandCommand - Снятие выбора элемента сетки

    /// <summary>Снятие выбора элемента сетки</summary>
    private LambdaCommand<ShapeViewModel> _UnSetShapeCommandCommand;

    /// <summary>Снятие выбора элемента сетки</summary>
    public ICommand UnSetShapeCommandCommand => _UnSetShapeCommandCommand
        ??= new(OnUnSetShapeCommandCommandExecuted, CanUnSetShapeCommandCommandExecute);

    /// <summary>Проверка возможности выполнения - Снятие выбора элемента сетки</summary>
    private bool CanUnSetShapeCommandCommandExecute(ShapeViewModel Shape) => Shapes.Contains(Shape);

    /// <summary>Логика выполнения - Снятие выбора элемента сетки</summary>
    private void OnUnSetShapeCommandCommandExecuted(ShapeViewModel Shape)
    {
        if (Equals(Shape, _SelectedShape))
            SelectedShape = null;
    }

    #endregion

    #region SelectedShape : ShapeViewModel - Выбранная модель

    /// <summary>Выбранная модель</summary>
    private ShapeViewModel _SelectedShape;

    /// <summary>Выбранная модель</summary>
    public ShapeViewModel SelectedShape
    {
        get => _SelectedShape;
        set => SetValue(ref _SelectedShape, value).Then(selected => Shapes.Foreach(selected, (s, current) => s.IsSelected = Equals(s, current)));
    }

    #endregion



    #region Items : Collection - Элементы

    private ICollection<ViewModel> _Items = new ObservableCollection<ViewModel>();

    public ICollection<ViewModel> Items
    {
        get => _Items;
        set
        {
            var old_items = _Items;
            if (!Set(ref _Items, value)) return;

            if (old_items is INotifyCollectionChanged old_observable)
            {
                old_observable.CollectionChanged -= OnItemsCollectionChanged;
                foreach (var item in old_items)
                    OnItemsElementRemoved(item);
            }

            if (value is INotifyCollectionChanged new_observable)
            {
                new_observable.CollectionChanged += OnItemsCollectionChanged;
                foreach (var item in value)
                    OnItemsElementAdded(item);
            }
        }
    }

    protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is { Count: > 0 } added)
                    foreach (ViewModel item in added)
                        OnItemsElementAdded(item);
                break;
            case NotifyCollectionChangedAction.Remove:
                if(e.OldItems is { Count: > 0 } removed)
                    foreach (ViewModel item in removed)
                        OnItemsElementRemoved(item);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems is { Count: > 0 } old_items)
                    foreach (ViewModel item in old_items)
                        OnItemsElementRemoved(item);
                if (e.NewItems is { Count: > 0 } new_items)
                    foreach (ViewModel item in new_items)
                        OnItemsElementAdded(item);
                break;
        }
    }

    protected virtual void OnItemsElementAdded(ViewModel item) => item.PropertyChanged += OnItemPropertyChanged;
    protected virtual void OnItemsElementRemoved(ViewModel item) => item.PropertyChanged -= OnItemPropertyChanged;

    protected virtual void OnItemPropertyChanged(object item, PropertyChangedEventArgs e)
    {

    }

    #endregion
}
