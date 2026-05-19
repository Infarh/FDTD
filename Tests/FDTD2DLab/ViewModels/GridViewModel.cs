using DynamicData;
using DynamicData.Binding;
using FDTD2DLab.Infrastructure.Extensions;
using FDTD2DLab.ViewModels.Material;
using FDTD2DLab.ViewModels.Probe;
using FDTD2DLab.ViewModels.Propertys;
using FDTD2DLab.ViewModels.Shapes;
using FDTD2DLab.ViewModels.Source;
using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Reactive.Disposables.Fluent;

namespace FDTD2DLab.ViewModels;

public class GridViewModel : ViewModel, IOptProperty, IDisposable
{

    private string _name = "Сетка"; 

    public string Name { get => _name; set => SetValue(ref _name, value); }


    //TODO static хранение библиотек материалов
    // Материалы
    static MaterialViewModel vacuum = new MaterialViewModel { Name = "Vacuum", Eps = 1, Mu = 1, Sigma = 0 };
    static MaterialViewModel pec = new MaterialViewModel { Name = "PEC", Eps = 1, Mu = 1, Sigma = 1e10 };
    public GridViewModel(MainWindowViewModel MainModel)
    {
        this.MainModel = MainModel;
        //Shapes.CollectionChanged += (_, e) =>
        //Shapes.OnItems().Changed(nameof(ShapeViewModel.IsSelected), OnChangedIsSelectedChanged);
        #region test
        // Добавляем тестовые объекты, чтобы они были видны сразу
        // Точечный источник
        var pointSource = new PointSourceViewModel
        {
            SourceType = typeof(PointSourceViewModel),
            Name = "Источник 1",
            SignalType = SignalType.Sine,
            X = this.Lx / 2,
            Y = this.Ly / 2,
            Frequency = 1e8
        };
        this.Sources.Add(pointSource);

        // Плоская волна (вертикальная линия)
        //var planeWave = new PlaneWaveSourceViewModel
        //{
        //    SourceType = typeof(PlaneWaveSourceViewModel),
        //    Name = "Плоская волна",
        //    Position = this.Lx / 2,
        //    Start = this.Ly / 4,
        //    End = 3 * this.Ly / 4,
        //    IsHorizontal = false
        //};
        //this.Sources.Add(planeWave);

        // Зонд
        var probe = new ProbeViewModel
        {
            ProbeType = typeof(ProbeViewModel),
            Name = $"Зонд {Probes.Count + 1}",
            X = Lx / 2,
            Y = Ly / 2,
            Component = FieldComponent.Ez,
        };
        this.Probes.Add(probe);

        #endregion

        Materials.Add(vacuum);
        BackgroundMaterial = vacuum;

        Materials.Add(pec);

        var probesStream = _probes.ToObservableChangeSet()
            .Transform(x => (IOptProperty)x);

        var sourcesStream = _sources.ToObservableChangeSet()
            .Transform(x => (IOptProperty)x);

        var shapesStream = _Shapes.ToObservableChangeSet()
            .Transform(x => (IOptProperty)x);

        // Объединяем три потока
        var mergedStream = probesStream.Merge(sourcesStream).Merge(shapesStream);

        // Применяем сортировку (например, по имени)
        // Важно: Sort применяется к IObservable<IChangeSet<IOptProperty>>
        mergedStream
            .Sort(SortExpressionComparer<IOptProperty>.Ascending(x => x.Name))
            .Bind(out var items)
            .ObserveOn(DispatcherScheduler.Current)
            .Subscribe()
            .DisposeWith(_cleanUp);

        Items = items;



        UpdateGridX();
        UpdateGridY();
    }

    private readonly CompositeDisposable _cleanUp = new();


    #region базовые свойства сетки MainModel _ignoreSelectionChange dx dy SpaceUnit Lx Ly CellCount

    [JsonIgnore]
    public MainWindowViewModel MainModel { get; }

    private bool _ignoreSelectionChange;

    #region GridMousePosition : Point - Положение мыши в сетке пространства
    [JsonIgnore]
    /// <summary>Положение мыши в сетке пространства</summary>
    private Point _GridMousePosition;
    [JsonIgnore]
    /// <summary>Положение мыши в сетке пространства</summary>
    public Point GridMousePosition { get => _GridMousePosition; set => Set(ref _GridMousePosition, value); }
    [JsonIgnore]
    [DependencyOn(nameof(Lx))]
    [DependencyOn(nameof(Ly))]
    [DependencyOn(nameof(GridMousePosition))]
    public Point MousePosition => new(Lx * _GridMousePosition.X, Ly * (1 - _GridMousePosition.Y));
    [JsonIgnore]
    [DependencyOn(nameof(dx))]
    [DependencyOn(nameof(dy))]
    [DependencyOn(nameof(MousePosition))]
    public Point MousePositionCell => new(Math.Ceiling(Lx * _GridMousePosition.X / _dx) - 1, Math.Ceiling(Ly * (1 - _GridMousePosition.Y) / _dy) - 1);

    #endregion

    #region Nx : int - Размер сетки по горизонтали

    /// <summary>Размер сетки по горизонтали</summary>
    private int _Nx = 400;

    /// <summary>Размер сетки по горизонтали</summary>
    public int Nx { get => _Nx; set => SetValue(ref _Nx, value, n => n > 2).Then(UpdateGridX); }

    #endregion

    #region Ny : int - Размер сетки по вертикали

    /// <summary>Размер сетки по вертикали</summary>
    private int _Ny = 400;

    /// <summary>Размер сетки по вертикали</summary>
    public int Ny { get => _Ny; set => SetValue(ref _Ny, value, n => n > 2).Then(UpdateGridY); }

    #endregion

    #region dx : double - Шаг сетки по горизонтали

    /// <summary>Шаг сетки по горизонтали</summary>
    private double _dx = 0.2;

    /// <summary>Шаг сетки по горизонтали</summary>
    public double dx { get => _dx; set => SetValue(ref _dx, value, d => d > 0).Then(UpdateGridX); }

    #endregion

    #region dy : double - Шаг сетки по вертикали

    /// <summary>Шаг сетки по вертикали</summary>
    private double _dy = 0.2;

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
    [JsonIgnore]
    /// <summary>Сетка по оси X</summary>
    private IEnumerable<double> _GridX;
    [JsonIgnore]
    /// <summary>Сетка по оси X</summary>
    public IEnumerable<double> GridX { get => _GridX; private set => Set(ref _GridX, value); }

    private void UpdateGridX() => GridX = Enumerable.Range(1, _Nx - 1).Select(i => i * _dx).ToArray();

    #endregion

    #region GridY : IEnumerable<double> - Сетка по оси Y
    [JsonIgnore]
    /// <summary>Сетка по оси Y</summary>
    private IEnumerable<double> _GridY;
    [JsonIgnore]
    /// <summary>Сетка по оси Y</summary>
    public IEnumerable<double> GridY { get => _GridY; private set => Set(ref _GridY, value); }

    private void UpdateGridY() => GridY = Enumerable.Range(1, _Ny - 1).Select(i => i * _dy).ToArray();

    #endregion

    #endregion

    private ObservableCollection<ShapeViewModel> _Shapes = new()
    {
        new RectViewModel
        {
            Width = 50,
            Height = 50,
            X = 180,
            Y = 50,
            IsSelected = false,
            ShapeName = "прямоугольник",
            ShapeType = typeof(RectViewModel),
            AppliedMaterial = vacuum
        },

        new EllipseViewModel
        {
            test = 15,
            Width = 60,
            Height = 20,
            X = 50,
            Y = 70,
            IsSelected = false,
            ShapeName = "элипс",
            ShapeType = typeof(EllipseViewModel),
            AppliedMaterial = vacuum
        },
    };

    //private CollectionItemsChangeTracker<ObservableCollection<ShapeViewModel>> _ShapesTracker;

    public ObservableCollection<ShapeViewModel> Shapes
    {
        get => _Shapes;
        set
        {
            if (!Set(ref _Shapes, value, out var old_shapes)) return;
            SetValue(ref _Shapes, value);
            //_ShapesTracker?.Dispose();
            //var tracker = value.OnItems();
            //tracker?.Changed(nameof(ShapeViewModel.IsSelected), OnChangedIsSelectedChanged);
            //_ShapesTracker = tracker;
        }
        
        
    }

    //private void OnChangedIsSelectedChanged(object Shape) => SelectedShape = (ShapeViewModel)Shape;

    //private ObservableCollection<IOptProperty> _Items = new();

    //public ReadOnlyObservableCollection<IOptProperty> Items
    //{
    //    get => _Items;
    //    set
    //    {
    //        if(!Set(ref _Items, value, out var old_items)) return;
    //        SetValue(ref _Items, value);
    //    }
    //}
    public ReadOnlyObservableCollection<IOptProperty> Items { get; }

    /*------------------------------------------------------------------------------------*/

    #region Выборы элементов сетки

    #region Command SetShapeCommandCommand - Выбор фигуры элемента сетки

    /// <summary>Выбор элемента сетки</summary>
    private LambdaCommand<ShapeViewModel> _SetShapeCommandCommand;

    /// <summary>Выбор элемента сетки</summary>
    public ICommand SetShapeCommandCommand => _SetShapeCommandCommand
        ??= new(OnSetShapeCommandCommandExecuted, CanSetShapeCommandCommandExecute);

    /// <summary>Проверка возможности выполнения - Выбор элемента сетки</summary>
    private bool CanSetShapeCommandCommandExecute(ShapeViewModel Shape) => Shapes.Contains(Shape);

    /// <summary>Логика выполнения - Выбор элемента сетки</summary>
    private void OnSetShapeCommandCommandExecuted(ShapeViewModel Shape) 
    { 
        //SelectedShape = Shape;  
        //SelectedMaterial = Shape.AppliedMaterial; 
        SelectedProperty = Shape; 
    }

    #endregion

    #region Command UnSetShapeCommandCommand - Снятие выбора фигуры элемента сетки

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
        if (Equals(Shape, _SelectedShape)) { 
            //SelectedShape = null;
            //SelectedMaterial = null;
            //SelectedProperty = null;
        }

    }

    #endregion

    #region Command SetSourceCommandCommand - Выбор источника 

    /// <summary>Выбор элемента сетки</summary>
    private LambdaCommand<SourceViewModel> _SetSourceCommandCommand;

    /// <summary>Выбор элемента сетки</summary>
    public ICommand SetSourceCommandCommand => _SetSourceCommandCommand
        ??= new(OnSetSourceCommandCommandExecuted, CanSetSourceCommandCommandExecute);

    /// <summary>Проверка возможности выполнения - Выбор элемента сетки</summary>
    private bool CanSetSourceCommandCommandExecute(SourceViewModel source) => Sources.Contains(source);

    /// <summary>Логика выполнения - Выбор элемента сетки</summary>
    private void OnSetSourceCommandCommandExecuted(SourceViewModel source)
    {
        //SelectedSource = source;
        SelectedProperty = source;
    }

    #endregion

    #region Command UnSetSourceCommandCommand - Снятие выбора источника

    /// <summary>Снятие выбора элемента сетки</summary>
    private LambdaCommand<SourceViewModel> _UnSetSourceCommandCommand;

    /// <summary>Снятие выбора элемента сетки</summary>
    public ICommand UnSetSourceCommandCommand => _UnSetSourceCommandCommand
        ??= new(OnUnSetSourceCommandCommandExecuted, CanUnSetSourceCommandCommandExecute);

    /// <summary>Проверка возможности выполнения - Снятие выбора элемента сетки</summary>
    private bool CanUnSetSourceCommandCommandExecute(SourceViewModel source) => Sources.Contains(source);

    /// <summary>Логика выполнения - Снятие выбора элемента сетки</summary>
    private void OnUnSetSourceCommandCommandExecuted(SourceViewModel source)
    {
        if (Equals(source, _selectedSource)) { 
            //SelectedSource = null;
            //SelectedProperty = null;
        }
    }

    #endregion

    #region Command SetProbeCommandCommand - Выбор зонда 

    /// <summary>Выбор элемента сетки</summary>
    private LambdaCommand<ProbeViewModel> _SetProbeCommandCommand;

    /// <summary>Выбор элемента сетки</summary>
    public ICommand SetProbeCommandCommand => _SetProbeCommandCommand
        ??= new(OnSetProbeCommandCommandExecuted, CanSetProbeCommandCommandExecute);

    /// <summary>Проверка возможности выполнения - Выбор элемента сетки</summary>
    private bool CanSetProbeCommandCommandExecute(ProbeViewModel probe) => Probes.Contains(probe);

    /// <summary>Логика выполнения - Выбор элемента сетки</summary>
    private void OnSetProbeCommandCommandExecuted(ProbeViewModel probe)
    {
        //SelectedProbe = probe;
        SelectedProperty = probe;
    }

    #endregion

    #region Command UnSetProbeCommandCommand - Снятие выбора источника

    /// <summary>Снятие выбора элемента сетки</summary>
    private LambdaCommand<ProbeViewModel> _UnSetProbeCommandCommand;

    /// <summary>Снятие выбора элемента сетки</summary>
    public ICommand UnSetProbeCommandCommand => _UnSetProbeCommandCommand
        ??= new(OnUnSetProbeCommandCommandExecuted, CanUnSetProbeCommandCommandExecute);

    /// <summary>Проверка возможности выполнения - Снятие выбора элемента сетки</summary>
    private bool CanUnSetProbeCommandCommandExecute(ProbeViewModel probe) => Probes.Contains(probe);

    /// <summary>Логика выполнения - Снятие выбора элемента сетки</summary>
    private void OnUnSetProbeCommandCommandExecuted(ProbeViewModel probe)
    {
        if (Equals(probe, _selectedProbe)) { 
            //SelectedProbe = null;
            //SelectedProperty = null;
        }
    }

    #endregion

    #endregion

    /*----------------------------------------------------------------------------------------------*/

    #region Command AddRectShapeToGrid - добавление прямоугольника на grid

    private LambdaCommand _AddRectShapeToGrid;

    /// <summary>Сохранить как</summary>
    public ICommand AddRectShapeToGrid => _AddRectShapeToGrid ??= new(OnAddRectShapeToGridExecute, CanAddRectShapeToGridExecute);

    /// <summary>Проверка возможности выполнения - Сохранить как</summary>
    private bool CanAddRectShapeToGridExecute() => true;

    /// <summary>Логика выполнения - Сохранить как</summary>
    private void OnAddRectShapeToGridExecute()
    {
        var item = new RectViewModel
        {
            ShapeType = typeof(RectViewModel),
            X = 125,
            Y = 50,
            AppliedMaterial = pec,
            IsSelected = false
        };
        Shapes.Add(item);
    }

    #endregion

    #region Command AddRectShapeToGrid - добавление элипса на grid

    private LambdaCommand _AddEllipseShapeToGrid;

    /// <summary>Сохранить как</summary>
    public ICommand AddEllipseShapeToGrid => _AddEllipseShapeToGrid ??= new(OnAddEllipseShapeToGridExecute, CanAddEllipseShapeToGridExecute);

    /// <summary>Проверка возможности выполнения - Сохранить как</summary>
    private bool CanAddEllipseShapeToGridExecute() => true;

    /// <summary>Логика выполнения - Сохранить как</summary>
    private void OnAddEllipseShapeToGridExecute()
    {
        var item = new EllipseViewModel
        {
            ShapeType = typeof(EllipseViewModel),
            X = 50,
            Y = 40,
            IsSelected = false,
            Angle = 0,
            AppliedMaterial = pec
        };


        Shapes.Add(item);
    }

    #endregion

    #region SelectedShape : ShapeViewModel - Выбранная модель

    /// <summary>Выбранная модель</summary>
    private ShapeViewModel _SelectedShape;

    /// <summary>Выбранная модель</summary>
    public ShapeViewModel SelectedShape
    {
        get => _SelectedShape;
        set
        {
            
            // Отписываемся от предыдущей фигуры
            if (_SelectedShape != null) { 
                _SelectedShape.PropertyChanged -= OnShapePropertyChanged;
            }

            if (Set(ref _SelectedShape, value))
            {
                // Подписываемся на новую фигуру
                if (value != null)
                    value.PropertyChanged += OnShapePropertyChanged;

                if (value != null)
                {
                    SelectedProperty = value;
                    SelectedMaterial = value.AppliedMaterial;
                    SelectedSource = null;
                }
                // Если value == null, ничего не делаем, так как SelectedProperty мог быть установлен из другого места

            }
        }
    }

    private void OnShapePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShapeViewModel.AppliedMaterial))
        {
            var shape = (ShapeViewModel)sender;
            if (shape == SelectedShape)
            {
                _ignoreSelectionChange = true;
                SelectedMaterial = shape.AppliedMaterial;
                _ignoreSelectionChange = false;
            }
        }
    }

    #endregion

    #region Selected Property

    /// <summary>Выбранная модель</summary>
    private IOptProperty _SelectedProperty;

    /// <summary>Выбранная модель</summary>
    public IOptProperty SelectedProperty
    {
        get => _SelectedProperty;
        set
        {
            if (Set(ref _SelectedProperty, value))
            {

                // Синхронизируем вспомогательные свойства в зависимости от типа
                if (value is ShapeViewModel shape)
                {
                    SelectedShape = shape;
                    SelectedMaterial = shape.AppliedMaterial;
                    SelectedSource = null;
                    SelectedProbe = null;
                    //shape.IsSelected = true;
                }
                else if (value is MaterialViewModel material)
                {
                    SelectedMaterial = material;
                    SelectedShape = null;
                    SelectedSource = null;
                    SelectedProbe = null;
                }
                else if (value is SourceViewModel source)
                {
                    SelectedSource = source;
                    SelectedShape = null;
                    SelectedMaterial = null;
                    SelectedProbe = null;
                }
                else if (value is ProbeViewModel probe)
                {
                    SelectedProbe = probe;
                    SelectedShape = null;
                    SelectedMaterial = null;
                    SelectedSource = null;
                }
                else
                {
                    SelectedShape = null;
                    SelectedMaterial = null;
                    SelectedSource = null;
                }

            }
        }
    }

    #endregion

    #region dt : double - Шаг по времени (секунды)

    private double _dt = 424e-12; // значение по умолчанию 1 нс

    /// <summary>Шаг по времени (секунды)</summary>
    public double dt
    {
        get => _dt;
        set => SetValue(ref _dt, value, d => d > 0); // только положительные значения
    }

    #endregion

    #region MaxDt : double - Максимально допустимый шаг по времени (расчётный)

    [DependencyOn(nameof(dx))]
    [DependencyOn(nameof(dy))]
    [DependencyOn(nameof(Nx))]
    [DependencyOn(nameof(Ny))]
    public double MaxDt
    {
        get
        {
            // Формула Куранта для вакуума (ε=1, μ=1)
            double c = 299792458; // 299792458 м/с
            double inv_dx2 = 1.0 / (_dx * _dx);
            double inv_dy2 = 1.0 / (_dy * _dy);
            return 1.0 / (c * Math.Sqrt(inv_dx2 + inv_dy2));
        }
    }

    #endregion

    #region Sources

    private ObservableCollection<SourceViewModel> _sources = new();
    public ObservableCollection<SourceViewModel> Sources
    {
        get => _sources;
        set => Set(ref _sources, value);
    }

    #endregion

    #region Command AddPointSource - Добавить точечный источник

    private LambdaCommand _addPointSourceCommand;
    public ICommand AddPointSourceCommand => _addPointSourceCommand ??= new(OnAddPointSource);

    private void OnAddPointSource()
    {
        var source = new PointSourceViewModel
        {
            SourceType = typeof(PointSourceViewModel),
            Name = $"Точечный {Sources.Count + 1}",
            SignalType = SignalType.Sine,
            X = Lx / 2,
            Y = Ly / 2,
            Amplitude = 1.0,
            Tau = 0,
            T0 = 1e-9,
            Frequency = 1e-3
        };
        Sources.Add(source);
        SelectedSource = source;
    }

    #endregion

    #region Command AddPlaneWaveSource - Добавить источник плоской волны

    private LambdaCommand _addPlaneWaveSourceCommand;
    public ICommand AddPlaneWaveSourceCommand => _addPlaneWaveSourceCommand ??= new(OnAddPlaneWaveSource);

    private void OnAddPlaneWaveSource()
    {
        var source = new PlaneWaveSourceViewModel
        {
            SourceType= typeof(PlaneWaveSourceViewModel),
            Name = $"Плоская волна {Sources.Count + 1}",
            Position = Lx / 2,
            Start = 0,
            End = Ly,
            IsHorizontal = false,
            Amplitude = 1.0,
            Tau = 1e-9,
            T0 = 1e-9
        };
        Sources.Add(source);
        SelectedSource = source;
    }

    #endregion

    #region SelectedSource : SourceViewModel - Выбранный источник

    private SourceViewModel _selectedSource;
    public SourceViewModel SelectedSource
    {
        get => _selectedSource;
        set
        {
            if (Set(ref _selectedSource, value))
            {

                if (value != null)
                {
                    SelectedProperty = value;
                    SelectedShape = null;
                    SelectedMaterial = null;
                }

            }
        }
    }

    #endregion

    #region DeletStructureCommand

    private LambdaCommand _deleteStructureCommand;
    public ICommand DeletStructureCommand => _deleteStructureCommand ??= new(OnDeleteStructureCommand, CanDeleteStructureCommand);

    private void OnDeleteStructureCommand()
    {
        if (SelectedProperty == null) return;

        // Удаление фигуры
        if (SelectedProperty is ShapeViewModel shape && Shapes.Contains(shape))
        {
            Shapes.Remove(shape);
            SelectedProperty = null;
        }
        // Удаление источника
        else if (SelectedProperty is SourceViewModel source && Sources.Contains(source))
        {
            Sources.Remove(source);
            SelectedProperty = null;
        }
        else if (SelectedProperty is ProbeViewModel probe && Probes.Contains(probe))
        {
            Probes.Remove(probe);
            SelectedProperty = null;
        }
    }

    private bool CanDeleteStructureCommand() => SelectedProperty.IsNotNull();


    #endregion

    #region Material

    private ObservableCollection<MaterialViewModel> _materials = new();
    public ObservableCollection<MaterialViewModel> Materials
    {
        get => _materials;
        set => Set(ref _materials, value);
    }

    private MaterialViewModel _selectedMaterial;
    public MaterialViewModel SelectedMaterial
    {
        get => _selectedMaterial;
        set
        {
            if (Set(ref _selectedMaterial, value))
            {

            }
        }
    }

    private MaterialViewModel _backgroundMaterial;
    public MaterialViewModel BackgroundMaterial
    {
        get => _backgroundMaterial;
        set => Set(ref _backgroundMaterial, value);
    }

    #endregion

    #region AddMaterialCommand

    private LambdaCommand _addMaterialCommand;
    public ICommand AddMaterialCommand => _addMaterialCommand ??= new(OnAddMaterial);

    private void OnAddMaterial()
    {
        var material = new MaterialViewModel
        {
            Name = $"Материал {Materials.Count + 1}",
            Eps = 1.0,
            Mu = 1.0,
            Sigma = 0.0
        };
        Materials.Add(material);
        SelectedMaterial = material; // автоматически отобразит свойства в правой панели
        SelectedProperty = material; 
    }

    #endregion

    #region Probes

    #region Probe SelectProbe

    private ObservableCollection<ProbeViewModel> _probes = new();
    public ObservableCollection<ProbeViewModel> Probes
    {
        get => _probes;
        set => Set(ref _probes, value);
    }

    private ProbeViewModel _selectedProbe;
    public ProbeViewModel SelectedProbe
    {
        get => _selectedProbe;
        set
        {
            if (Set(ref _selectedProbe, value))
            {
                if (_ignoreSelectionChange) return;
                _ignoreSelectionChange = true;

                if (value != null)
                {
                    SelectedProperty = value;
                    SelectedShape = null;
                    SelectedMaterial = null;
                    SelectedSource = null;
                }

                _ignoreSelectionChange = false;
            }
        }
    }

    #endregion

    #region AddProbeCommand - Команда добавления зонда


    private LambdaCommand _addProbeCommand;
    public ICommand AddProbeCommand => _addProbeCommand ??= new(OnAddProbe);

    private void OnAddProbe()
    {
        var probe = new ProbeViewModel
        {
            ProbeType = typeof(ProbeViewModel),
            Name = $"Зонд {Probes.Count + 1}",
            X = Lx / 2,
            Y = Ly / 2,
            Component = FieldComponent.Ez,
        };
        Probes.Add(probe);
        SelectedProbe = probe;
        SelectedProperty = probe;
    }

    // Команда выбора зонда на канвасе
    private LambdaCommand<ProbeViewModel> _selectProbeCommand;
    public ICommand SelectProbeCommand => _selectProbeCommand ??= new(OnSelectProbe);

    private void OnSelectProbe(ProbeViewModel probe)
    {
        SelectedProperty = probe;
        SelectedShape = null;
        SelectedMaterial = null;
    }

    #endregion

    #endregion

    #region Boundares


    private BoundaryType _boundaryLeft = BoundaryType.ABC;
    public BoundaryType BoundaryLeft
    {
        get => _boundaryLeft;
        set => Set(ref _boundaryLeft, value);
    }

    private BoundaryType _boundaryRight = BoundaryType.ABC;
    public BoundaryType BoundaryRight
    {
        get => _boundaryRight;
        set => Set(ref _boundaryRight, value);
    }

    private BoundaryType _boundaryTop = BoundaryType.ABC;
    public BoundaryType BoundaryTop
    {
        get => _boundaryTop;
        set => Set(ref _boundaryTop, value);
    }

    private BoundaryType _boundaryBottom = BoundaryType.ABC;
    public BoundaryType BoundaryBottom
    {
        get => _boundaryBottom;
        set => Set(ref _boundaryBottom, value);
    }




    #endregion

    #region Boundaries PML

    private bool _usePml;
    public bool UsePml
    {
        get => _usePml;
        set => Set(ref _usePml, value);
    }

    private int _pmlThickness = 20;
    public int PmlThickness
    {
        get => _pmlThickness;
        set => Set(ref _pmlThickness, value);
    }

    private double _pmlSigmaMax = 0.0015;
    public double PmlSigmaMax
    {
        get => _pmlSigmaMax;
        set => Set(ref _pmlSigmaMax, value);
    }

    private double _pmlProfilePower = 4;
    public double PmlProfilePower
    {
        get => _pmlProfilePower;
        set => Set(ref _pmlProfilePower, value);
    }

    #endregion

    public void Dispose()
    {
        _cleanUp.Dispose();
    }

    private LambdaCommand<ProbeViewModel> _testingCommand;
    public ICommand TestingCommand => _testingCommand ??= new(OnTesting);

    private void OnTesting(ProbeViewModel probe)
    {
        System.Console.WriteLine("test");
    }


}

public enum BoundaryType
{
    None,        // без граничных условий (свободное пространство, но численно неустойчиво)
    ABC,         // Absorbing Boundary Condition (Mur)
    PEC,         // Perfect Electric Conductor (Ez = 0)
    PMC,         // Perfect Magnetic Conductor (Hz = 0)
    PML          // Perfectly Matched Layer (требует дополнительных параметров)
}


