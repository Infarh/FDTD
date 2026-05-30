using DynamicData;
using FDTD.Space2D;
using FDTD.Space2D.Boundaries;
using FDTD.Space2D.Boundaries.ABC;
using FDTD.Space2D.Boundaries.PEC;
using FDTD.Space2D.Boundaries.PMC;
using FDTD.Space2D.Sources;
using FDTD2DLab.Infrastructure;
using FDTD2DLab.Infrastructure.Serialization;
using FDTD2DLab.Services.Interfaces;
using FDTD2DLab.ViewModels.Material;
using FDTD2DLab.ViewModels.Probe;
using FDTD2DLab.ViewModels.Propertys;
using FDTD2DLab.ViewModels.Shapes;
using FDTD2DLab.ViewModels.Source;
using FDTD2DLab.Views;
using MathCore.ViewModels;
using MathCore.WPF.Commands;
using MathCore.WPF.Converters;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace FDTD2DLab.ViewModels;

public class MainWindowViewModel : ViewModel
{

    private string _projectFolder;

    public MainWindowViewModel(IUserDialog UserDialog, IComputer Computer)
    {
        Grid = new(this);

        _UserDialog = UserDialog;
        _Computer = Computer;

        
        OpenProbeTabCommand = new LambdaCommand<ProbeViewModel>(OpenProbeTab);

        Tabs = new ObservableCollection<TabItemViewModel>
        {
            new TabItemViewModel
            {
                Header = "Пространство",
                Content = new Views.SpaceView(this),
                CanClose = false
            },
            new TabItemViewModel
            {
                Header = "Поле",
                Content = new Views.FieldView { DataContext = FieldVM },
                CanClose = false
            }
        };
        //SelectedTab = Tabs[0];

        UpdateTitle();
    }

    private readonly IUserDialog _UserDialog;
    private readonly IComputer _Computer;

    //public ObservableCollection<FileInfo> RecentFiles { get; } = new();

    #region Grid : GridViewModel - Счётная область
    /// <summary>Счётная область</summary>
    private GridViewModel _Grid;
    /// <summary>Счётная область</summary>
    public GridViewModel Grid { get => _Grid; set => Set(ref _Grid, value); }

    #endregion

    #region Title : string - Заголовок

    /// <summary>Заголовок</summary>
    private string _Title = "FDTD";

    /// <summary>Заголовок</summary>
    public string Title { get => _Title; private set => Set(ref _Title, value); }

    private void UpdateTitle() => Title = $"FDTD+ v0.0a {ProjectName}" +
    (_ProjectFile != null ? $" [{_ProjectFile.Name}]" : "");

    #endregion

    #region Status : string - Статус

    /// <summary>Статус</summary>
    private string _Status;

    /// <summary>Статус</summary>
    public string Status { get => _Status; set => Set(ref _Status, value); }

    #endregion

    #region Progress

    private double _progressValue;
    public double ProgressValue { get => _progressValue; set => Set(ref _progressValue, value); }

    private double _progressMaximum = 100;
    public double ProgressMaximum { get => _progressMaximum; set => Set(ref _progressMaximum, value); }

    #endregion

    #region ProjectName : string - Название проекта

    /// <summary>Название проекта</summary>
    private string _ProjectName = "Новый проект";

    /// <summary>Название проекта</summary>
    public string ProjectName { get => _ProjectName; set => SetValue(ref _ProjectName, value).Then(UpdateTitle); }

    #endregion

    #region ProjectFile : FileInfo - Файл проекта

    /// <summary>Файл проекта</summary>
    private FileInfo _ProjectFile;

    /// <summary>Файл проекта</summary>
    public FileInfo ProjectFile { get => _ProjectFile; set => SetValue(ref _ProjectFile, value).Then(UpdateTitle); }

    #endregion

    #region GIF поле

    private FieldViewModel _fieldViewModel = new();
    public FieldViewModel FieldVM { get => _fieldViewModel; set => Set(ref _fieldViewModel, value); }

    private List<Solver2DFrame> _recordedFrames = new();

    private readonly object _recordedFramesLock = new();

    private static double[,] CloneArray(double[,] source)
    {
        int width = source.GetLength(0);
        int height = source.GetLength(1);
        var copy = new double[width, height];
        Buffer.BlockCopy(source, 0, copy, 0, Buffer.ByteLength(source));
        return copy;
    }

    private string SaveLastSimulationToGif()
    {
        List<Solver2DFrame> frames;
        lock (_recordedFramesLock)
        {
            if (_recordedFrames.Count == 0) return null;
            frames = new List<Solver2DFrame>(_recordedFrames);
        }

        string dir = _projectFolder ?? Path.GetTempPath();
        string gifPath = Path.Combine(dir, $"simulation_{DateTime.Now:yyyyMMddHHmmss}.gif");

        // Используем существующий метод SaveFramesToGif (модифицированный, чтобы принимать List и путь)
        SaveFramesToGif(frames, gifPath);
        return gifPath;
    }

    #endregion

    #region Command ExitCommand - Выход из приложения

    /// <summary>Выход из приложения</summary>
    private Command _ExitCommand;

    /// <summary>Выход из приложения</summary>
    public ICommand ExitCommand => _ExitCommand ??= Command.New(() => Application.Current.MainWindow!.Close());

    #endregion

    #region Command CreateCommand - Создать

    /// <summary>Создать</summary>
    private LambdaCommand _CreateCommand;

    /// <summary>Создать</summary>
    public ICommand CreateCommand => _CreateCommand ??= new(OnCreateCommandExecuted);

    /// <summary>Логика выполнения - Создать</summary>
    private void OnCreateCommandExecuted()
    {
        if (_UserDialog.GetString("Название проекта", "Назовите проект", "Новый проект") is not { } project_name) return;
        Set(ref _ProjectName, project_name);
        Set(ref _ProjectFile, null);
        _projectFolder = null;
        UpdateTitle();

        // Сброс всех данных
        Grid.Shapes.Clear();
        Grid.Sources.Clear();
        Grid.Materials.Clear();
        Grid.Probes.Clear();
        Grid.BackgroundMaterial = Grid.Materials.FirstOrDefault() ?? new MaterialViewModel { Name = "Vacuum", Eps = 1, Mu = 1, Sigma = 0 };
        Grid.Nx = 100;
        Grid.Ny = 100;
        Grid.dx = 1;
        Grid.dy = 1;
        Grid.SpaceUnit = "м";
        Grid.dt = 1e-9;

        Status = "Создан новый проект";

        // Добавляем тестовые объекты, чтобы они были видны сразу
        // Точечный источник
        var pointSource = new PointSourceViewModel
        {
            Name = "Источник 1",
            X = Grid.Lx / 2,
            Y = Grid.Ly / 2,
            Frequency = 1e4
        };
        Grid.Sources.Add(pointSource);

        // Плоская волна (вертикальная линия)
        var planeWave = new PlaneWaveSourceViewModel
        {
            Name = "Плоская волна",
            Position = Grid.Lx / 2,
            Start = Grid.Ly / 4,
            End = 3 * Grid.Ly / 4,
            IsHorizontal = false
        };
        Grid.Sources.Add(planeWave);

        // Зонд
        var probe = new ProbeViewModel
        {
            Name = "Зонд 1",
            X = Grid.Lx / 2,
            Y = Grid.Ly / 2,
            Component = FieldComponent.Ez
        };
        Grid.Probes.Add(probe);
    }

    #endregion

    #region Command OpenCommand - Открыть

    /// <summary>Открыть</summary>
    private LambdaCommand<FileInfo> _OpenCommand;

    /// <summary>Открыть</summary>
    public ICommand OpenCommand => _OpenCommand ??= new(OnOpenCommandExecuted, CanOpenCommandExecute);

    /// <summary>Проверка возможности выполнения - Открыть</summary>
    private bool CanOpenCommandExecute(FileInfo file) => true;

    /// <summary>Логика выполнения - Открыть</summary>
    private void OnOpenCommandExecuted(FileInfo file)
    {
        var fileInfo = _UserDialog.OpenFile(
            "Открыть проект",
            "Файлы проекта FDTD2D (*.fdtd2d)|*.fdtd2d|Все файлы (*.*)|*.*");
        if (fileInfo == null) return;

        LoadProject(fileInfo.FullName);
    }

    #endregion

    #region Command SaveCommand - Сохранить

    private ProjectData _projectData;

    /// <summary>Сохранить</summary>
    private LambdaCommand _SaveCommand;

    /// <summary>Сохранить</summary>
    public ICommand SaveCommand => _SaveCommand ??= new(OnSaveCommandExecuted, CanSaveCommandExecute);

    /// <summary>Проверка возможности выполнения - Сохранить</summary>
    private bool CanSaveCommandExecute() => _projectFolder != null;

    /// <summary>Логика выполнения - Сохранить</summary>
    private void OnSaveCommandExecuted()
    {
        if (_ProjectFile == null || !_ProjectFile.Exists)
        {
            OnSaveAsCommandExecuted(null);
            return;
        }
        SaveProject(_ProjectFile.FullName);
        Status = $"Проект сохранён: {_ProjectFile.Name}";
    }

    private FileInfo GetnewProjectFile(FileInfo Default) => Default ?? _UserDialog.SaveFile(
        "Сохранить проект как...",
        "Файлы проекта (*.fdtdproj)|*.fdtdproj|Xml-файлы (*.xml)|*.xml|Json-файлы (*.json)|*.json|Все файлы (*.*)|*.*",
        ProjectFile?.FullName);

    private void SaveProject(string projectFilePath)
    {
        if (_projectData == null)
            _projectData = new ProjectData { ProjectName = ProjectName };

        string baseDir = Path.GetDirectoryName(projectFilePath) ?? ".";

        // Задаём относительные имена файлов компонентов
        _projectData.GridFile = "grid.gmfdtd";
        _projectData.MaterialsFile = "materials.mmfdtd";
        _projectData.SourcesFile = "source.smfdtd";
        _projectData.ProbesFile = "probes.pmfdtd";

        // Сохраняем компоненты
        SaveGrid(Path.Combine(baseDir, _projectData.GridFile));
        SaveMaterials(Path.Combine(baseDir, _projectData.MaterialsFile));
        SaveSources(Path.Combine(baseDir, _projectData.SourcesFile));
        SaveProbes(Path.Combine(baseDir, _projectData.ProbesFile));

        // Сохраняем сам .fdtd2d
        var json = JsonSerializer.Serialize(_projectData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(projectFilePath, json);
    }

    //private string SaveComponent(string defaultFileName, string baseDir, Func<string, string> serializer)
    //{
    //    // Если файл уже существует вне базовой папки, оставляем его путь,
    //    // иначе сохраняем в baseDir с именем по умолчанию.
    //    string currentPath = GetCurrentComponentPath(defaultFileName);
    //    if (!string.IsNullOrEmpty(currentPath) && !Path.GetRelativePath(baseDir, currentPath).StartsWith("."))
    //    {
    //        // Файл где-то вне папки проекта — не перезаписываем, путь остаётся абсолютным
    //        serializer(currentPath);
    //        return currentPath;
    //    }
    //    else
    //    {
    //        string newPath = Path.Combine(baseDir, defaultFileName);
    //        serializer(newPath);
    //        return defaultFileName; // относительный путь в .fdtd2d
    //    }
    //}

    // Вспомогательные методы для сериализации компонентов в строку JSON и записи в файл
    private string SerializeGrid(string path)
    {
        var gridData = CreateGridData();
        var json = JsonSerializer.Serialize(gridData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
        return path;
    }


    /// <summary>
    /// Аналогично для материалов, источников, зондов...
    /// </summary>
    /// <returns></returns>
    private GridData CreateGridData()
    {
        return new GridData
        {
            Nx = Grid.Nx,
            Ny = Grid.Ny,
            Dx = Grid.dx,
            Dy = Grid.dy,
            Dt = Grid.dt,
            SpaceUnit = Grid.SpaceUnit,
            BackgroundMaterialName = Grid.BackgroundMaterial?.Name,
            Shapes = Grid.Shapes.Select(shape => new ShapeData
            {
                Type = shape is RectViewModel ? "Rect" : "Ellipse",
                X = shape.X,
                Y = shape.Y,
                Width = shape.Width,
                Height = shape.Height,
                Angle = shape.Angle,
                MaterialName = shape.AppliedMaterial?.Name,
                Eps = shape.AppliedMaterial.Eps,
                Mu = shape.AppliedMaterial.Mu,
                Sigma = shape.AppliedMaterial.Sigma
            }).ToList()
        };
    }

    private void LoadProject(string projectFilePath)
    {
        string json = File.ReadAllText(projectFilePath);
        _projectData = JsonSerializer.Deserialize<ProjectData>(json)
                       ?? throw new InvalidOperationException("Неверный формат файла проекта.");

        string baseDir = Path.GetDirectoryName(projectFilePath) ?? ".";

        LoadMaterials(Path.Combine(baseDir, _projectData.MaterialsFile));
        // Загружаем компоненты, пути могут быть относительными
        LoadGrid(Path.Combine(baseDir, _projectData.GridFile));
        LoadSources(Path.Combine(baseDir, _projectData.SourcesFile));
        LoadProbes(Path.Combine(baseDir, _projectData.ProbesFile));

        // В конце LoadProject
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            // Заставляем все привязанные элементы перемериться
            var spaceView = Tabs.OfType<TabItemViewModel>()
                               .Select(t => t.Content)
                               .OfType<Views.SpaceView>()
                               .FirstOrDefault();
            spaceView?.UpdateLayout();
            // Дополнительно сообщаем системе о необходимости обновления привязок
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }, System.Windows.Threading.DispatcherPriority.Loaded);

        Application.Current.MainWindow?.UpdateLayout();

        UpdateTitle();
    }

    // Сохраняет сетку и фигуры в указанный JSON-файл
    private void SaveGrid(string filePath)
    {
        var gridData = new GridData
        {
            Nx = Grid.Nx,
            Ny = Grid.Ny,
            Dx = Grid.dx,
            Dy = Grid.dy,
            Dt = Grid.dt,
            SpaceUnit = Grid.SpaceUnit,
            BackgroundMaterialName = Grid.BackgroundMaterial?.Name,
            Shapes = Grid.Shapes.Select(shape => new ShapeData
            {
                Type = shape is RectViewModel ? "Rect" : "Ellipse",
                X = shape.X,
                Y = shape.Y,
                Width = shape.Width,
                Height = shape.Height,
                Angle = shape.Angle,
                MaterialName = shape.AppliedMaterial?.Name,
                Eps = shape.AppliedMaterial.Eps,
                Mu = shape.AppliedMaterial.Mu,
                Sigma = shape.AppliedMaterial.Sigma
            }).ToList()
        };
        var json = JsonSerializer.Serialize(gridData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    // Сохраняет пользовательские материалы в JSON-файл
    private void SaveMaterials(string filePath)
    {
        var materialsToSave = Grid.Materials
            .Where(m => m.Name != "Vacuum" && m.Name != "PEC")
            .Select(m => new MaterialData
            {
                Name = m.Name,
                Eps = m.Eps,
                Mu = m.Mu,
                Sigma = m.Sigma
            })
            .ToList();
        var json = JsonSerializer.Serialize(materialsToSave, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    // Сохраняет источники в JSON-файл
    private void SaveSources(string filePath)
    {
        var sourcesData = Grid.Sources.Select(src => new SourceData
        {
            Type = src is PointSourceViewModel ? "Point" : "PlaneWave",
            X = (src as PointSourceViewModel)?.X ?? 0,
            Y = (src as PointSourceViewModel)?.Y ?? 0,
            Position = (src as PlaneWaveSourceViewModel)?.Position ?? 0,
            Start = (src as PlaneWaveSourceViewModel)?.Start ?? 0,
            End = (src as PlaneWaveSourceViewModel)?.End ?? 0,
            IsHorizontal = (src as PlaneWaveSourceViewModel)?.IsHorizontal ?? false,
            Name = src.Name,
            SignalType = src.SignalType,
            Amplitude = src.Amplitude,
            T0 = src.T0,
            Tau = src.Tau,
            Frequency = src.Frequency,
            Phase = src.Phase
        }).ToList();
        var json = JsonSerializer.Serialize(sourcesData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    // Сохраняет зонды в JSON-файл
    private void SaveProbes(string filePath)
    {
        var probesData = Grid.Probes.Select(p => new ProbeData
        {
            Name = p.Name,
            X = p.X,
            Y = p.Y,
            Component = p.Component
        }).ToList();
        var json = JsonSerializer.Serialize(probesData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }


    // Загружает сетку и фигуры из указанного JSON-файла
    private void LoadGrid(string filePath)
    {
        if (!File.Exists(filePath)) return;
        var json = File.ReadAllText(filePath);
        var gridData = JsonSerializer.Deserialize<GridData>(json);
        if (gridData == null) return;

        Grid.Nx = gridData.Nx;
        Grid.Ny = gridData.Ny;
        Grid.dx = gridData.Dx;
        Grid.dy = gridData.Dy;
        Grid.dt = gridData.Dt;
        Grid.SpaceUnit = gridData.SpaceUnit;

        Grid.BackgroundMaterial = Grid.Materials.FirstOrDefault(m => m.Name == gridData.BackgroundMaterialName)
                                  ?? Grid.Materials.FirstOrDefault();

        Grid.Shapes.Clear();
        foreach (var shapeData in gridData.Shapes)
        {
            ShapeViewModel shape = shapeData.Type switch
            {
                "Rect" => new RectViewModel(),
                "Ellipse" => new EllipseViewModel(),
                _ => null
            };
            if (shape == null) continue;

            shape.X = shapeData.X;
            shape.Y = shapeData.Y;
            shape.Width = shapeData.Width;
            shape.Height = shapeData.Height;
            shape.Angle = shapeData.Angle;
            // добавляем
            shape.ShapeType = shape.GetType();

            if (!string.IsNullOrEmpty(shapeData.MaterialName))
            {
                var material = Grid.Materials.FirstOrDefault(m => m.Name == shapeData.MaterialName);
                if (material != null)
                    shape.AppliedMaterial = material;
                else
                {
                    shape.AppliedMaterial.Eps = shapeData.Eps;
                    shape.AppliedMaterial.Mu = shapeData.Mu;
                    shape.AppliedMaterial.Sigma = shapeData.Sigma;
                }
            }
            else
            {
                shape.AppliedMaterial.Eps = shapeData.Eps;
                shape.AppliedMaterial.Mu = shapeData.Mu;
                shape.AppliedMaterial.Sigma = shapeData.Sigma;
            }
            shape.ShapeType = shape.GetType();
            Grid.Shapes.Add(shape);
        }
    }

    // Загружает материалы из указанного JSON-файла
    private void LoadMaterials(string filePath)
    {
        if (!File.Exists(filePath)) return;
        var json = File.ReadAllText(filePath);
        var materialsData = JsonSerializer.Deserialize<List<MaterialData>>(json);
        if (materialsData == null) return;

        // Оставляем стандартные Vacuum и PEC, остальные заменяем
        var toRemove = Grid.Materials.Where(m => m.Name != "Vacuum" && m.Name != "PEC").ToList();
        foreach (var m in toRemove) Grid.Materials.Remove(m);

        foreach (var mData in materialsData)
        {
            Grid.Materials.Add(new MaterialViewModel
            {
                Name = mData.Name,
                Eps = mData.Eps,
                Mu = mData.Mu,
                Sigma = mData.Sigma
            });
        }
    }

    // Загружает источники из указанного JSON-файла
    private void LoadSources(string filePath)
    {
        if (!File.Exists(filePath)) return;
        var json = File.ReadAllText(filePath);
        var sourcesData = JsonSerializer.Deserialize<List<SourceData>>(json);
        if (sourcesData == null) return;

        Grid.Sources.Clear();
        foreach (var srcData in sourcesData)
        {
            SourceViewModel source = srcData.Type switch
            {
                "Point" => new PointSourceViewModel(),
                "PlaneWave" => new PlaneWaveSourceViewModel(),
                _ => null
            };

            if (source == null) continue;


            if (source is PointSourceViewModel point)
            {
                Grid.Sources.Add(new PointSourceViewModel
                {
                    SourceType = source.GetType(),
                    Name = srcData.Name,
                    SignalType = srcData.SignalType,
                    Amplitude = srcData.Amplitude,
                    T0 = srcData.T0,
                    Tau = srcData.Tau,
                    Frequency = srcData.Frequency,
                    Phase = srcData.Phase,
                    X = srcData.X,
                    Y = srcData.Y
                });
            }
            else if (source is PlaneWaveSourceViewModel plane)
            {
                Grid.Sources.Add(new PlaneWaveSourceViewModel
                {
                    SourceType = source.GetType(),
                    Name = srcData.Name,
                    SignalType = srcData.SignalType,
                    Amplitude = srcData.Amplitude,
                    T0 = srcData.T0,
                    Tau = srcData.Tau,
                    Frequency = srcData.Frequency,
                    Phase = srcData.Phase,
                    X = srcData.X,
                    Y = srcData.Y,
                    Position = srcData.Position,
                    Start = srcData.Start,
                    End = srcData.End,
                    IsHorizontal = srcData.IsHorizontal,
                }
            );

            }

        }
    }

    // Загружает зонды из указанного JSON-файла
    private void LoadProbes(string filePath)
    {
        if (!File.Exists(filePath)) return;
        var json = File.ReadAllText(filePath);
        var probesData = JsonSerializer.Deserialize<List<ProbeData>>(json);
        if (probesData == null) return;

        Grid.Probes.Clear();
        foreach (var pData in probesData)
        {
            Grid.Probes.Add(new ProbeViewModel
            {
                Name = pData.Name,
                X = pData.X,
                Y = pData.Y,
                Component = pData.Component,
                ProbeType = typeof(ProbeViewModel)
            });
        }
    }

    #endregion






    #region Command SaveAsCommand - Сохранить как

    /// <summary>Сохранить как</summary>
    private LambdaCommand<FileInfo> _SaveAsCommand;

    /// <summary>Сохранить как</summary>
    public ICommand SaveAsCommand => _SaveAsCommand ??= new(OnSaveAsCommandExecuted, CanSaveAsCommandExecute);

    /// <summary>Проверка возможности выполнения - Сохранить как</summary>
    private bool CanSaveAsCommandExecute(FileInfo file) => true;

    /// <summary>Логика выполнения - Сохранить как</summary>
    private void OnSaveAsCommandExecuted(FileInfo file)
    {
        var path = _UserDialog.SaveFile(
            "Сохранить проект как...",
            "Файлы проекта FDTD2D (*.fdtd2d)|*.fdtd2d|Все файлы (*.*)|*.*",
            _ProjectFile?.FullName ?? "Новый проект.fdtd2d");
        if (path == null) return;

        string fullPath = path.FullName;
        if (!fullPath.EndsWith(".fdtd2d", StringComparison.OrdinalIgnoreCase))
            fullPath += ".fdtd2d";

        _ProjectFile = new FileInfo(fullPath);
        _projectFolder = _ProjectFile.DirectoryName;

        SaveProject(fullPath);
        ProjectName = Path.GetFileNameWithoutExtension(fullPath);
        Status = $"Проект сохранён: {_ProjectFile.Name}";
        UpdateTitle();
    }

    #endregion

    #region

    private void SaveGridViewModelToJson(string filePath, GridViewModel SaveGrid)
    {
        string jsonString = JsonSerializer.Serialize(SaveGrid, new JsonSerializerOptions()
        {   
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver() 
        });
        File.WriteAllText(filePath, jsonString);
    }

    private GridViewModel LoadGridViewModelFromJson(string filePath)
    {
        string jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<GridViewModel>(jsonString);
    }

    #endregion

    #region Simulation

    private CancellationTokenSource _simulationCts;
    private FdtdSimulationService _simulationService;

    /// <summary>Текущий кадр для отображения в реальном времени</summary>
    private WriteableBitmap _fieldBitmap;
    public WriteableBitmap FieldBitmap
    {
        get => _fieldBitmap;
        private set => Set(ref _fieldBitmap, value);
    }

    /// <summary>Флаг, показывающий, идёт ли сейчас расчёт</summary>
    private bool _isSimulating;
    public bool IsSimulating
    {
        get => _isSimulating;
        private set => Set(ref _isSimulating, value);
    }

    private double _simulationTotalTime = 1e-6;   // значение по умолчанию
    private int _sleepDelayMs = 30;

    public double SimulationTotalTime
    {
        get => _simulationTotalTime;
        set => Set(ref _simulationTotalTime, value);
    }

    public int SleepDelayMs
    {
        get => _sleepDelayMs;
        set => Set(ref _sleepDelayMs, value);
    }

    #endregion

    #region Commands Calculate

    // Быстрый запуск (без окна)
    private LambdaCommand _quickStartCommand;
    public ICommand QuickStartCommand => _quickStartCommand ??= new(QuickStart, CanStartSimulation);
    private async void QuickStart()
    {
        // Параметры по умолчанию: 1000 шагов, задержка 30 мс
        SimulationTotalTime = 1000 * Grid.dt;
        SleepDelayMs = 30;
        await StartSimulationAsync(isRealTime: true, saveGif: false);
    }

    private LambdaCommand _startGifRecordingCommand;
    public ICommand StartGifRecordingCommand => _startGifRecordingCommand ??= new(StartGifRecording, CanStartSimulation);

    private LambdaCommand _startRealTimeCommand;
    public ICommand StartRealTimeCommand => _startRealTimeCommand ??= new(StartRealTimeDialog, CanStartSimulation);

    private async void StartRealTimeDialog()
    {
        var dialog = new Views.SimulationSettingsDialog(this)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        if (dialog.ShowDialog() == true && dialog.DataContext is SimulationSettingsViewModel settings)
        {
            SimulationTotalTime = settings.SimulationTime;
            SleepDelayMs = settings.SleepDelayMs;
            await StartSimulationAsync(isRealTime: true, saveGif: settings.SaveGif);
        }
    }

    private LambdaCommand _stopSimulationCommand;
    public ICommand StopSimulationCommand => _stopSimulationCommand ??= new(StopSimulation, () => IsSimulating);

    private void StopSimulation()
    {
        _simulationCts?.Cancel();
    }


    private bool CanStartSimulation() => !IsSimulating && Grid != null;

    private async void StartGifRecording()
    {
        //if (!CanStartSimulation()) return;
        //SimulationTotalTime = 1e-6; // можно другое
        await StartSimulationAsync(isRealTime: false, saveGif: true);
    }

    private async void StartRealTime()
    {
        await StartSimulationAsync(isRealTime: true);
    }

    private async Task StartSimulationAsync(bool isRealTime, bool saveGif = false)
    {
        if (IsSimulating) return;

        var (solver, materialArrays) = PrepareSolverFromGrid();
        _simulationService = new FdtdSimulationService(solver, materialArrays, Grid.dt);
        _simulationCts = new CancellationTokenSource();
        IsSimulating = true;

        try
        {
            if (isRealTime)
            {
                // Запускаем реальное время с записью кадров, если saveGif == true
                await Task.Run(() => RunRealTimeSimulation(_simulationCts.Token, recordFrames: saveGif));
                if (saveGif)
                {
                    string gifPath = await Task.Run(() => SaveLastSimulationToGif());
                    if (!string.IsNullOrEmpty(gifPath))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            FieldVM.LoadFromFile(gifPath);
                            // Переключаем вкладку на "Поле"
                            var fieldTab = Tabs.FirstOrDefault(t => t.Header == "Поле");
                            if (fieldTab != null) SelectedTab = fieldTab;
                        });
                    }
                }
            }
            else
            {
                string gifPath = await Task.Run(() => RunGifSimulation(_simulationCts.Token));
                if (!string.IsNullOrEmpty(gifPath))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FieldVM.LoadFromFile(gifPath);
                        var fieldTab = Tabs.FirstOrDefault(t => t.Header == "Поле");
                        if (fieldTab != null) SelectedTab = fieldTab;
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            Status = "Моделирование прервано";
        }
        finally
        {
            IsSimulating = false;
            _simulationCts?.Dispose();
            _simulationCts = null;
            _simulationService = null;
        }
    }

    private (Solver2D solver, MaterialArrays arrays) PrepareSolverFromGrid()
    {
        // Преобразовать GridViewModel в Solver2D
        var solver = new Solver2D(Grid.Nx, Grid.Ny, Grid.dx, Grid.dy);

        // Создать массивы материалов на основе фигур из Grid.Shapes
        var eps = new double[Grid.Nx, Grid.Ny];
        var mu = new double[Grid.Nx, Grid.Ny];
        var sigma = new double[Grid.Nx, Grid.Ny];

        // Инициализация фона
        for (int i = 0; i < Grid.Nx; i++)
            for (int j = 0; j < Grid.Ny; j++)
            {
                eps[i, j] = Grid.BackgroundMaterial?.Eps ?? 1;
                mu[i, j] = Grid.BackgroundMaterial?.Mu ?? 1;
                sigma[i, j] = Grid.BackgroundMaterial?.Sigma ?? 0;
            }

        // Наложение фигур
        foreach (var shape in Grid.Shapes)
        {
            if (shape is RectViewModel rect)
            {
                FillRect(eps, mu, sigma, rect); // внутри используются rect.Eps, rect.Mu, rect.Sigma
            }
            else if (shape is EllipseViewModel ellipse)
            {
                FillEllipse(eps, mu, sigma, ellipse);
            }
        }


        if(Grid.BoundaryBottom == BoundaryType.PML || Grid.BoundaryLeft == BoundaryType.PML 
            || Grid.BoundaryBottom == BoundaryType.PML || Grid.BoundaryLeft == BoundaryType.PML) Grid.UsePml = true;
        else Grid.UsePml = false;



        // Добавление PML
        AddPmlLayers(sigma, Grid);

        // Применение граничных классов (PEC, PMC, ABC)
        ApplyBoundaryConditions(solver, Grid);

        // установка источников в solver
        foreach (var src in CreateSourcesFromViewModels(Grid))
        {
            solver.Sources.Add(src);
        }

        // Делаем проверку, если весь массив сигма нулевой, то делаем его Null что бы в движке моделировать без потерь
        if(sigma.Cast<double>().All(x => x == 0.0)) solver.SetSigmaGrid(null);
        else solver.SetSigmaGrid((i, j) => sigma[i, j]);
        // Установка массивов в solver через делегаты
        solver.SetEpsGrid((i, j) => eps[i, j]);
        solver.SetMuGrid((i, j) => mu[i, j]);



        


        return (solver, new MaterialArrays(eps, mu, sigma));
    }

    private void FillRect(double[,] eps, double[,] mu, double[,] sigma, RectViewModel rect)
    {
        // Преобразование координат фигуры в индексы ячеек
        int i0 = (int)(rect.X / Grid.dx);
        int i1 = (int)((rect.X + rect.Width) / Grid.dx);
        int j0 = (int)(rect.Y / Grid.dy);
        int j1 = (int)((rect.Y + rect.Height) / Grid.dy);

        i0 = Math.Max(0, i0);
        i1 = Math.Min(Grid.Nx - 1, i1);
        j0 = Math.Max(0, j0);
        j1 = Math.Min(Grid.Ny - 1, j1);

        for (int i = i0; i <= i1; i++)
            for (int j = j0; j <= j1; j++)
            {
                eps[i, j] = rect.AppliedMaterial.Eps;
                mu[i, j] = rect.AppliedMaterial.Mu;
                sigma[i, j] = rect.AppliedMaterial.Sigma;
            }
    }

    private void FillEllipse(double[,] eps, double[,] mu, double[,] sigma, EllipseViewModel ellipse)
    {
        // Аналогично, но с проверкой принадлежности точки эллипсу
        double a = ellipse.Width / 2;
        double b = ellipse.Height / 2;
        double cx = ellipse.X + a;
        double cy = ellipse.Y + b;
        double angle = ellipse.Angle * Math.PI / 180.0;
        double cosA = Math.Cos(angle);
        double sinA = Math.Sin(angle);

        int i0 = (int)((cx - a) / Grid.dx);
        int i1 = (int)((cx + a) / Grid.dx);
        int j0 = (int)((cy - b) / Grid.dy);
        int j1 = (int)((cy + b) / Grid.dy);

        i0 = Math.Max(0, i0);
        i1 = Math.Min(Grid.Nx - 1, i1);
        j0 = Math.Max(0, j0);
        j1 = Math.Min(Grid.Ny - 1, j1);

        for (int i = i0; i <= i1; i++)
            for (int j = j0; j <= j1; j++)
            {
                double x = i * Grid.dx - cx;
                double y = j * Grid.dy - cy;
                // Поворот координат
                double xr = x * cosA + y * sinA;
                double yr = -x * sinA + y * cosA;
                if ((xr * xr) / (a * a) + (yr * yr) / (b * b) <= 1.0)
                {
                    eps[i, j] = ellipse.AppliedMaterial.Eps;
                    mu[i, j] = ellipse.AppliedMaterial.Mu;
                    sigma[i, j] = ellipse.AppliedMaterial.Sigma;
                }
            }
    }

    private void RunRealTimeSimulation(CancellationToken token, bool recordFrames = false)
    {
        #region подготовка к расчету

        var mesh = _simulationService.Solver.GetMesh(_simulationService.Dt);
        double totalTime = SimulationTotalTime; // берём из главной VM
        double dt = Grid.dt;
        int totalSteps = (int)(totalTime / dt);
        int sleepDelay = SleepDelayMs; // берём из главной VM

        Application.Current.Dispatcher.Invoke(() =>
        {
            ProgressMaximum = totalSteps;
            ProgressValue = 0;
        });

        int frameCount = 0;

        // Подготовка зондов (выполняется один раз перед циклом)
        var probes = Grid.Probes.Select(p => new
        {
            Probe = p,
            I = (int)(p.X / Grid.dx),
            J = (int)(p.Y / Grid.dy),
            Component = p.Component
        }).Where(p => p.I >= 0 && p.I < Grid.Nx && p.J >= 0 && p.J < Grid.Ny).ToList();

        // Очистка старых данных
        foreach (var p in Grid.Probes) p.ClearData();
        // Очищаем список перед записью
        if (recordFrames)
            lock (_recordedFramesLock) _recordedFrames.Clear();

        #endregion


        ///
        /// Цикл расчета в реальном времени по кадрам 
        ///
        foreach (var frame in mesh.Calculation(totalTime))
        {

            try
            {
                token.ThrowIfCancellationRequested();
            }
            finally
            {
                //Очистка ресурсов
            }


            // Сбор данных
            foreach (var p in probes)
            {
                double value = GetFieldComponent(frame, p.Component, p.I, p.J);
                p.Probe.AddSample(frame.Time, value);
            }

            if (recordFrames)
            {
                // Клонируем кадр, чтобы сохранить копию данных
                var copy = new Solver2DFrame(frame.Index, frame.Time,
                    CloneArray(frame.Hx), CloneArray(frame.Hy), CloneArray(frame.Hz),
                    CloneArray(frame.Ex), CloneArray(frame.Ey), CloneArray(frame.Ez));
                lock (_recordedFramesLock) _recordedFrames.Add(copy);
            }

            // Обновление WriteableBitmap через Dispatcher
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateFieldBitmap(frame.Ez); // например, используем Ez
                ProgressValue = frame.Index + 1;
            });

            // Можно добавить задержку для замедления анимации
            Thread.Sleep(sleepDelay);
            frameCount++;

        }//foreach (var frame in mesh.Calculation(totalTime))
        ///
        /// КОНЕЦ РАСЧЕТА
        ///


        // После завершения обновляем графики зондов (чтобы при открытии вкладки они были готовы)
        foreach (var p in Grid.Probes) p.UpdatePlotModel();

    }

    private string RunGifSimulation(CancellationToken token)
    {
        var mesh = _simulationService.Solver.GetMesh(_simulationService.Dt);
        double totalTime = SimulationTotalTime;
        double dt = Grid.dt;
        int totalSteps = (int)(totalTime / dt);
        int skip = 10;

        Application.Current.Dispatcher.Invoke(() =>
        {
            ProgressMaximum = totalSteps;
            ProgressValue = 0;
        });

        var frames = new List<Solver2DFrame>();

        foreach (var frame in mesh.Calculation(totalTime))
        {
            token.ThrowIfCancellationRequested();
            if (frame.Index % skip == 0)
                frames.Add(frame);
            Application.Current.Dispatcher.Invoke(() => ProgressValue = frame.Index + 1);
        }

        string gifPath = Path.Combine(Path.GetTempPath(), $"simulation_{DateTime.Now:yyyyMMddHHmmss}.gif");
        SaveFramesToGif(frames, gifPath, skip);
        return gifPath;
    }

    private void SaveFramesToGif(List<Solver2DFrame> frames, string filePath, int frameSkip = 1)
    {
        int width = Grid.Nx;
        int height = Grid.Ny;

        // Вычисление глобального min/max (можно сделать параметром)
        double globalMin = double.MaxValue, globalMax = double.MinValue;
        foreach (var frame in frames)
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    double val = frame.Ez[i, j];
                    if (val < globalMin) globalMin = val;
                    if (val > globalMax) globalMax = val;
                }
        if (globalMax - globalMin < 1e-12) globalMax = globalMin + 1e-12;

        using var gif = new Image<Rgba32>(width, height);

        int frameIndex = 0;
        foreach (var frame in frames)
        {
            // Пропуск кадров по желанию
            if (frameIndex % frameSkip != 0) { frameIndex++; continue; }

            var gifFrame = (frameIndex == 0) ? gif.Frames.RootFrame : gif.Frames.CreateFrame();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    double val = frame.Ez[i, height - 1 - j]; // инверсия Y
                    double t = (val - globalMin) / (globalMax - globalMin);
                    byte r = (byte)(t * 255);
                    byte g = 0;
                    byte b = (byte)((1 - t) * 255);
                    gifFrame[i, j] = new Rgba32(r, g, b);
                }
            frameIndex++;
        }

        gif.Save(filePath, new GifEncoder());
    }

    #region Отображение поля

    private void UpdateFieldBitmap(double[,] field)
    {
        int width = Grid.Nx;
        int height = Grid.Ny;

        // Максимум для нормализации
        double maxAbs = 0;


        for (int i = 0; i < Grid.Sources.Count(); i++) maxAbs = Math.Max(maxAbs, Grid.Sources[i].Amplitude);

        if (FieldBitmap == null || FieldBitmap.PixelWidth != width || FieldBitmap.PixelHeight != height)
            FieldBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

        byte[] pixels = new byte[width * height * 4];
        int stride = width * 4;

        for (int j = 0; j < height; j++)
        {
            int srcJ = height - 1 - j;  // инверсия Y
            for (int i = 0; i < width; i++)
            {
                double value = field[i, srcJ];
                byte r = 0, g = 0, b = 0, a = 0;

                //if (maxAbs > 0)
                //{
                double t = value / 1;    // [-1, 1]
                double absT = Math.Min(1.0, Math.Abs(t));
                // Альфа-канал прямо пропорционален амплитуде (0..255)
                a = (byte)(absT * 255 * 0.9);
                // Цвет: чистый красный (положит.) или чистый синий (отрицат.)
                if (t > 0)
                    r = 255;
                else
                    b = 255;
                //}
                int index = j * stride + i * 4;
                pixels[index] = b;
                pixels[index + 1] = g;
                pixels[index + 2] = r;
                pixels[index + 3] = a;
            }
        }
        FieldBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }

    private void MapValueToColor(double value, out byte r, out byte g, out byte b)
    {
        // Простейшая карта: красный для положительных, синий для отрицательных
        if (value > 0)
        {
            byte intensity = (byte)Math.Min(255, value * 255);
            r = intensity;
            g = 0;
            b = 0;
        }
        else
        {
            byte intensity = (byte)Math.Min(255, -value * 255);
            r = 0;
            g = 0;
            b = intensity;
        }
    }

    private double GetFieldComponent(Solver2DFrame frame, FieldComponent comp, int i, int j)
    {
        switch (comp)
        {
            case FieldComponent.Ex: return frame.Ex[i, j];
            case FieldComponent.Ey: return frame.Ey[i, j];
            case FieldComponent.Ez: return frame.Ez[i, j];
            case FieldComponent.Hx: return frame.Hx[i, j];
            case FieldComponent.Hy: return frame.Hy[i, j];
            case FieldComponent.Hz: return frame.Hz[i, j];
            default: return 0;
        }
    }

    #endregion

    #endregion

    #region Boundary calculate
    private void ApplyBoundaryConditions(Solver2D solver, GridViewModel grid)
    {
        var boundaries = solver.Boundaries;

        // Левая граница (x=0)
        boundaries.X.MinEx = CreateBoundaryMinX(grid.BoundaryLeft);
        boundaries.X.MinEy = CreateBoundaryMinX(grid.BoundaryLeft);
        boundaries.X.MinEz = CreateBoundaryMinX(grid.BoundaryLeft);
        boundaries.X.MinHx = CreateBoundaryMinX(grid.BoundaryLeft);
        boundaries.X.MinHy = CreateBoundaryMinX(grid.BoundaryLeft);
        boundaries.X.MinHz = CreateBoundaryMinX(grid.BoundaryLeft);

        // Правая граница (x=Nx)
        boundaries.X.MaxEx = CreateBoundaryMaxX(grid.BoundaryRight);
        boundaries.X.MaxEy = CreateBoundaryMaxX(grid.BoundaryRight);
        boundaries.X.MaxEz = CreateBoundaryMaxX(grid.BoundaryRight);
        boundaries.X.MaxHx = CreateBoundaryMaxX(grid.BoundaryRight);
        boundaries.X.MaxHy = CreateBoundaryMaxX(grid.BoundaryRight);
        boundaries.X.MaxHz = CreateBoundaryMaxX(grid.BoundaryRight);

        // Нижняя граница (y=0)
        boundaries.Y.MinEx = CreateBoundaryMinY(grid.BoundaryBottom);
        boundaries.Y.MinEy = CreateBoundaryMinY(grid.BoundaryBottom);
        boundaries.Y.MinEz = CreateBoundaryMinY(grid.BoundaryBottom);
        boundaries.Y.MinHx = CreateBoundaryMinY(grid.BoundaryBottom);
        boundaries.Y.MinHy = CreateBoundaryMinY(grid.BoundaryBottom);
        boundaries.Y.MinHz = CreateBoundaryMinY(grid.BoundaryBottom);

        // Верхняя граница (y=Ny)
        boundaries.Y.MaxEx = CreateBoundaryMaxY(grid.BoundaryTop);
        boundaries.Y.MaxEy = CreateBoundaryMaxY(grid.BoundaryTop);
        boundaries.Y.MaxEz = CreateBoundaryMaxY(grid.BoundaryTop);
        boundaries.Y.MaxHx = CreateBoundaryMaxY(grid.BoundaryTop);
        boundaries.Y.MaxHy = CreateBoundaryMaxY(grid.BoundaryTop);
        boundaries.Y.MaxHz = CreateBoundaryMaxY(grid.BoundaryTop);
    }

    //private Boundary2D GetBoundary(BoundaryType type)
    //{
    //    return type switch
    //    {
    //        BoundaryType.ABC => new ABC2DMinX(), // для MinX, для MaxX нужно отдельно
    //        BoundaryType.PEC => new PECBoundary(),
    //        BoundaryType.PMC => new PMCBoundary(),
    //        BoundaryType.PML => new PMLBoundary(/* параметры */),
    //        _ => null
    //    };
    //}

    private Boundary2DMinX CreateBoundaryMinX(BoundaryType type)
    {
        return type switch
        {
            BoundaryType.ABC => new ABC2DMinX(),
            BoundaryType.PEC => new PEC2DMinX(),
            BoundaryType.PMC => new PMC2DMinX(), // если нужно
            _ => null
        };
    }

    private Boundary2DMaxX CreateBoundaryMaxX(BoundaryType type)
    {
        return type switch
        {
            BoundaryType.ABC => new ABC2DMaxX(),
            BoundaryType.PEC => new PEC2DMaxX(),
            BoundaryType.PMC => new PMC2DMaxX(),
            _ => null
        };
    }

    private Boundary2DMinY CreateBoundaryMinY(BoundaryType type)
    {
        return type switch
        {
            BoundaryType.ABC => new ABC2DMinY(),
            BoundaryType.PEC => new PEC2DMinY(),
            BoundaryType.PMC => new PMC2DMinY(),
            _ => null
        };
    }

    private Boundary2DMaxY CreateBoundaryMaxY(BoundaryType type)
    {
        return type switch
        {
            BoundaryType.ABC => new ABC2DMaxY(),
            BoundaryType.PEC => new PEC2DMaxY(),
            BoundaryType.PMC => new PMC2DMaxY(),
            _ => null
        };
    }


    private void AddPmlLayers(double[,] sigma, GridViewModel grid)
    {
        if (!grid.UsePml) return;

        int Nx = grid.Nx, Ny = grid.Ny;
        int pml = grid.PmlThickness;
        double sigmaMax = grid.PmlSigmaMax;
        double power = grid.PmlProfilePower;

        for (int i = 0; i < Nx; i++)
        {
            for (int j = 0; j < Ny; j++)
            {
                double factor = 0;
                // Левая граница
                if (i < pml)
                    factor = Math.Max(factor, Math.Pow(1 - (double)i / pml, power));
                // Правая граница
                if (i >= Nx - pml)
                    factor = Math.Max(factor, Math.Pow(1 - (double)(Nx - 1 - i) / pml, power));
                // Нижняя граница
                if (j < pml)
                    factor = Math.Max(factor, Math.Pow(1 - (double)j / pml, power));
                // Верхняя граница
                if (j >= Ny - pml)
                    factor = Math.Max(factor, Math.Pow(1 - (double)(Ny - 1 - j) / pml, power));

                sigma[i, j] += factor * sigmaMax;
            }
        }
    }

    #endregion

    #region Source calculate
    private IEnumerable<Source2D> CreateSourcesFromViewModels(GridViewModel grid)
    {
        var sources = new List<Source2D>();

        foreach (var src in grid.Sources)
        {
            if (src is PointSourceViewModel point)
            {
                int i = (int)(point.X / grid.dx);
                int j = (int)(point.Y / grid.dy);
                // Проверка границ
                if (i < 0 || i >= grid.Nx || j < 0 || j >= grid.Ny) continue;

                var func = src.GetSignalFunction();
                // Добавляем источник для компоненты Ez (можно выбрать через UI)
                var source2D = new Source2D(i, j, Ez: func);
                sources.Add(source2D);
            }
            else if (src is PlaneWaveSourceViewModel plane)
            {
                var func = src.GetSignalFunction();
                if (plane.IsHorizontal)
                {
                    // Горизонтальная линия: Y = const, X от Start до End
                    int j = (int)(plane.Position / grid.dy);
                    if (j < 0 || j >= grid.Ny) continue;
                    int i0 = (int)(plane.Start / grid.dx);
                    int i1 = (int)(plane.End / grid.dx);
                    i0 = Math.Max(0, i0);
                    i1 = Math.Min(grid.Nx - 1, i1);
                    for (int i = i0; i <= i1; i++)
                    {
                        sources.Add(new Source2D(i, j, Ez: func));
                    }
                }
                else
                {
                    // Вертикальная линия: X = const, Y от Start до End
                    int i = (int)(plane.Position / grid.dx);
                    if (i < 0 || i >= grid.Nx) continue;
                    int j0 = (int)(plane.Start / grid.dy);
                    int j1 = (int)(plane.End / grid.dy);
                    j0 = Math.Max(0, j0);
                    j1 = Math.Min(grid.Ny - 1, j1);
                    for (int j = j0; j <= j1; j++)
                    {
                        sources.Add(new Source2D(i, j, Ez: func));
                    }
                }
            }
        }

        return sources;
    }

    #endregion

    #region Tabs

    private ObservableCollection<TabItemViewModel> _tabs = new();
    public ObservableCollection<TabItemViewModel> Tabs
    {
        get => _tabs;
        set => Set(ref _tabs, value);
    }

    private TabItemViewModel _selectedTab;
    public TabItemViewModel SelectedTab
    {
        get => _selectedTab;
        set => Set(ref _selectedTab, value);
    }

    public ICommand OpenProbeTabCommand { get; }


    private void OpenProbeTab(ProbeViewModel probe)
    {
        var existing = Tabs.FirstOrDefault(t => t.Tag == probe);
        if (existing != null)
        {
            SelectedTab = existing;
            return;
        }

        // Создаём новый PlotModel, копируя данные зонда
        var plotModel = new OxyPlot.PlotModel
        {
            Title = probe.Name,
            IsLegendVisible = true
        };
        var series = new OxyPlot.Series.LineSeries { Title = probe.Component.ToString() };
        for (int i = 0; i < probe.TimeValues.Count; i++)
            series.Points.Add(new OxyPlot.DataPoint(probe.TimeValues[i], probe.FieldValues[i]));
        plotModel.Series.Add(series);
        plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, Title = "Время (с)" });
        plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, Title = probe.Component.ToString() });

        var wrapper = new PlotModelWrapper { PlotModel = plotModel };

        var tab = new TabItemViewModel
        {
            Header = probe.Name,
            Content = new Views.ProbePlotView { DataContext = wrapper },
            CanClose = true,
            Tag = probe
        };
        tab.CloseCommand = new LambdaCommand(() => Tabs.Remove(tab));

        Tabs.Add(tab);
        SelectedTab = tab;
    }

    private LambdaCommand<ProbeViewModel> _OpenMultiPlotCommand;
    public ICommand OpenMultiPlotCommand => _OpenMultiPlotCommand ??= new(OnOpenMultiPlotCommand);

    // В конструкторе:

    private void OnOpenMultiPlotCommand(ProbeViewModel probe)
    {
        // Ищем существующую вкладку MultiProbePlotView
        var existingTab = Tabs.FirstOrDefault(t => t.Content is Views.MultiProbePlotView);
        MultiPlotViewModel multiVm;
        if (existingTab != null)
        {
            multiVm = (MultiPlotViewModel)((Views.MultiProbePlotView)existingTab.Content).DataContext;
        }
        else
        {
            multiVm = new MultiPlotViewModel();
            var view = new Views.MultiProbePlotView { DataContext = multiVm };
            var tab = new TabItemViewModel
            {
                Header = "Сводный график",
                Content = view,
                CanClose = true,
                Tag = multiVm
            };
            tab.CloseCommand = new LambdaCommand(() => Tabs.Remove(tab));
            Tabs.Add(tab);
            SelectedTab = tab;
        }
        multiVm.AddProbe(probe);
    }

    #endregion

}

// Вспомогательный класс для передачи массивов материалов
public class MaterialArrays
{
    public double[,] Eps { get; }
    public double[,] Mu { get; }
    public double[,] Sigma { get; }

    public MaterialArrays(double[,] eps, double[,] mu, double[,] sigma)
    {
        Eps = eps;
        Mu = mu;
        Sigma = sigma;
    }
}

// Сервис моделирования (можно вынести в отдельный файл)
public class FdtdSimulationService
{
    public Solver2D Solver { get; }
    public MaterialArrays Materials { get; }
    public double Dt { get; }

    public FdtdSimulationService(Solver2D solver, MaterialArrays materials, double dt)
    {
        Solver = solver;
        Materials = materials;
        Dt = dt;
    }
}

// Вспомогательный класс обертка для графика
public class PlotModelWrapper
{
    public OxyPlot.PlotModel PlotModel { get; set; }
}

//#region Command AddRectShapeToGrid - добавление прямоугольника на grid

//private LambdaCommand _AddRectShapeToGrid;

///// <summary>Сохранить как</summary>
//public ICommand AddRectShapeToGrid => _AddRectShapeToGrid ??= new(OnAddRectShapeToGridExecute, CanAddRectShapeToGridExecute);

///// <summary>Проверка возможности выполнения - Сохранить как</summary>
//private bool CanAddRectShapeToGridExecute() => true;

///// <summary>Логика выполнения - Сохранить как</summary>
//private void OnAddRectShapeToGridExecute()
//{
//    var item = new RectViewModel
//    {
//        Width = 50,
//        Height = 50,
//        X = 125,
//        Y = 50,
//        IsSelected = false
//    };
//    Grid.Shapes.Add(item);

//    Status = $"Добавлен прямоугольник в пространство задачи";
//}

//#endregion

//#region Command AddRectShapeToGrid - добавление элипса на grid

//private LambdaCommand _AddEllipseShapeToGrid;

///// <summary>Сохранить как</summary>
//public ICommand AddEllipseShapeToGrid => _AddEllipseShapeToGrid ??= new(OnAddEllipseShapeToGridExecute, CanAddEllipseShapeToGridExecute);

///// <summary>Проверка возможности выполнения - Сохранить как</summary>
//private bool CanAddEllipseShapeToGridExecute() => true;

///// <summary>Логика выполнения - Сохранить как</summary>
//private void OnAddEllipseShapeToGridExecute()
//{
//    var item = new EllipseViewModel
//    {
//        Width = 60,
//        Height = 20,
//        X = 50,
//        Y = 40,
//        IsSelected = false
//    };
//    Grid.Shapes.Add(item);

//    Status = $"Добавлен овал в пространство задачи";
//}

//#endregion
