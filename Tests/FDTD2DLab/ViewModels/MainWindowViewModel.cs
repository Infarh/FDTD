using FDTD.Space2D;
using FDTD.Space2D.Sources;
using FDTD2DLab.Services.Interfaces;
using FDTD2DLab.ViewModels.Shapes;
using FDTD2DLab.ViewModels.Source;
using MathCore.ViewModels;
using MathCore.WPF.Commands;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FDTD2DLab.ViewModels;

public class MainWindowViewModel : ViewModel
{
    public MainWindowViewModel(IUserDialog UserDialog, IComputer Computer)
    {
        Grid = new(this);

        _UserDialog = UserDialog;
        _Computer = Computer;

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

    private void UpdateTitle() => Title = $"FDTD+ v0.0a {ProjectName} {ProjectFile?.Name}";

    #endregion

    #region Status : string - Статус

    /// <summary>Статус</summary>
    private string _Status;

    /// <summary>Статус</summary>
    public string Status { get => _Status; set => Set(ref _Status, value); }

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
        UpdateTitle();

        Grid.Shapes.Clear();

        Grid.Nx = 100;
        Grid.Ny = 100;
        Grid.dx = 1;
        Grid.dy = 1;
        Grid.SpaceUnit = Grid.SpaceUnits.First();

        Status = "Новый проект проект";
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
        file ??= _UserDialog.OpenFile("Открыть проект", "Файлы проекта (*.fdtdproj)|*.fdtdproj|Xml-файлы (*.xml)|*.xml|Json-файлы (*.json)|*.json|Все файлы (*.*)|*.*");
        if (file is null) return;

        //RecentFiles.Remove(file);
        //RecentFiles.Insert(0, file);
        ProjectFile = file;

        Status = $"Открыть проект {file.Name}";
    }

    #endregion

    #region Command SaveCommand - Сохранить

    /// <summary>Сохранить</summary>
    private LambdaCommand _SaveCommand;

    /// <summary>Сохранить</summary>
    public ICommand SaveCommand => _SaveCommand ??= new(OnSaveCommandExecuted, CanSaveCommandExecute);

    /// <summary>Проверка возможности выполнения - Сохранить</summary>
    private bool CanSaveCommandExecute() => ProjectFile != null;

    /// <summary>Логика выполнения - Сохранить</summary>
    private void OnSaveCommandExecuted()
    {
        var file = GetnewProjectFile(ProjectFile);

        Status = $"Проект сохранён в {file.Name}";
    }

    private FileInfo GetnewProjectFile(FileInfo Default) => Default ?? _UserDialog.SaveFile(
        "Сохранить проект как...",
        "Файлы проекта (*.fdtdproj)|*.fdtdproj|Xml-файлы (*.xml)|*.xml|Json-файлы (*.json)|*.json|Все файлы (*.*)|*.*",
        ProjectFile?.FullName);

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
        if (GetnewProjectFile(file) is not { } new_file)
            return;
        // здесь записываем данные проекта

        GridViewModel saved = this._Grid;
        

        SaveGridViewModelToJson(new_file.FullName, saved);
            
        Status = $"Проект сохранён в {new_file.Name}";
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

    #endregion

    #region Commands Calculate

    private LambdaCommand _startGifRecordingCommand;
    public ICommand StartGifRecordingCommand => _startGifRecordingCommand ??= new(StartGifRecording, CanStartSimulation);

    private LambdaCommand _startRealTimeCommand;
    public ICommand StartRealTimeCommand => _startRealTimeCommand ??= new(StartRealTime, CanStartSimulation);

    private LambdaCommand _stopSimulationCommand;
    public ICommand StopSimulationCommand => _stopSimulationCommand ??= new(StopSimulation, () => IsSimulating);

    private void StopSimulation()
    {
        _simulationCts?.Cancel();
    }

    private bool CanStartSimulation() => !IsSimulating && Grid != null;

    private async void StartGifRecording()
    {
        await StartSimulationAsync(isRealTime: false);
    }

    private async void StartRealTime()
    {
        await StartSimulationAsync(isRealTime: true);
    }

    private async Task StartSimulationAsync(bool isRealTime)
    {
        if (IsSimulating) return;

        var (solver, materialArrays) = PrepareSolverFromGrid();
        double dt = Grid.dt;

        //// Проверка устойчивости
        //double maxDt = solver.GetMaxStableTimeStep();
        //if (dt > maxDt)
        //{
        //    _UserDialog.ShowWarning($"Шаг по времени ({dt:e3} с) превышает максимально допустимый ({maxDt:e3} с). Уменьшите dt.");
        //    return;
        //}

        // Создаём сервис
        _simulationService = new FdtdSimulationService(solver, materialArrays, Grid.dt);//Grid.dt

        _simulationCts = new CancellationTokenSource();
        IsSimulating = true;

        try
        {
            if (isRealTime)
            {
                // Запуск с визуализацией
                await Task.Run(() => RunRealTimeSimulation(_simulationCts.Token));
            }
            else
            {
                // Запуск с сохранением GIF
                var gifPath = await Task.Run(() => RunGifSimulation(_simulationCts.Token));
                if (!string.IsNullOrEmpty(gifPath))
                {
                    // Открыть GIF в отдельном окне или на вкладке (можно открыть системное приложение)
                    System.Diagnostics.Process.Start(gifPath);
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

        // установка источников в solver
        foreach (var src in CreateSourcesFromViewModels(Grid))
        {
            solver.Sources.Add(src);
        }

        // Установка массивов в solver через делегаты
        solver.SetEpsGrid((i, j) => eps[i, j]);
        solver.SetMuGrid((i, j) => mu[i, j]);
        solver.SetSigmaGrid((i, j) => sigma[i, j]);

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
                eps[i, j] = rect.Eps;
                mu[i, j] = rect.Mu;
                sigma[i, j] = rect.Sigma;
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
                    eps[i, j] = ellipse.Eps;
                    mu[i, j] = ellipse.Mu;
                    sigma[i, j] = ellipse.Sigma;
                }
            }
    }

    private void RunRealTimeSimulation(CancellationToken token)
    {
        var mesh = _simulationService.Solver.GetMesh(_simulationService.Dt);
        double totalTime = 1e-6; // Задать нужное время моделирования
        int frameCount = 0;

        foreach (var frame in mesh.Calculation(totalTime))
        {
            token.ThrowIfCancellationRequested();

            // Обновление WriteableBitmap через Dispatcher
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateFieldBitmap(frame.Ez); // например, используем Ez
            });

            // Можно добавить задержку для замедления анимации
            Thread.Sleep(30);
            frameCount++;
        }
    }

    private string RunGifSimulation(CancellationToken token)
    {
        var mesh = _simulationService.Solver.GetMesh(_simulationService.Dt);
        double totalTime = 1e-9; // или другое значение из настроек
        var frames = new List<Solver2DFrame>();

        int skip = 10; // записывать каждый 10-й кадр (настраиваемо)

        foreach (var frame in mesh.Calculation(totalTime))
        {
            token.ThrowIfCancellationRequested();
            if (frame.Index % skip == 0)
                frames.Add(frame);
        }

        string gifPath = Path.Combine(Path.GetTempPath(), $"simulation_{DateTime.Now:yyyyMMddHHmmss}.gif");
        SaveFramesToGif(frames, gifPath, skip);
        return gifPath;
    }

    private void SaveFramesToGif(List<Solver2DFrame> frames, string filePath, int frameSkip = 1)
    {
        int width = Grid.Nx;
        int height = Grid.Ny;

        // Вычисляем глобальные минимум и максимум поля Ez по всем кадрам
        double globalMin = double.MaxValue;
        double globalMax = double.MinValue;
        foreach (var frame in frames)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    double val = frame.Ez[i, j];
                    if (val < globalMin) globalMin = val;
                    if (val > globalMax) globalMax = val;
                }
        }
        if (globalMax - globalMin < 1e-12)
            globalMax = globalMin + 1e-12; // защита от деления на ноль

        using var gif = new Image<Rgba32>(width, height);

        int frameIndex = 0;
        foreach (var frame in frames)
        {
            // Пропускаем кадры согласно frameSkip (если нужна экономия)
            if (frameIndex % frameSkip != 0)
            {
                frameIndex++;
                continue;
            }

            // Для первого кадра используем корневой фрейм, для остальных создаём новый
            ImageFrame<Rgba32> gifFrame = (frameIndex == 0)
                ? gif.Frames.RootFrame
                : gif.Frames.CreateFrame();

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    double val = frame.Ez[i, j];
                    // Нормализация в [0,1]
                    double t = (val - globalMin) / (globalMax - globalMin);
                    // Цветовая схема: синий (min) → красный (max)
                    byte r = (byte)(t * 255);
                    byte g = 0;
                    byte b = (byte)((1 - t) * 255);
                    gifFrame[i, j] = new Rgba32(r, g, b);
                }

            frameIndex++;
        }

        gif.Save(filePath, new GifEncoder());
    }

    private void UpdateFieldBitmap(double[,] field)
    {
        int width = Grid.Nx;
        int height = Grid.Ny;

        // Пересоздаём битмап, если размер изменился
        if (FieldBitmap == null || FieldBitmap.PixelWidth != width || FieldBitmap.PixelHeight != height)
        {
            FieldBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        }

        // Подготавливаем массив байтов в формате BGR (4 байта на пиксель)
        byte[] pixels = new byte[width * height * 4];
        int stride = width * 4;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                double value = field[i, j];
                // Преобразуем значение поля в цвет
                MapValueToColor(value, out byte r, out byte g, out byte b);
                int index = j * stride + i * 4;
                pixels[index] = b;       // Blue
                pixels[index + 1] = g;   // Green
                pixels[index + 2] = r;   // Red
                                         // index+3 = 0 (альфа-канал не используется)
            }
        }

        // Копируем массив в битмап
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
