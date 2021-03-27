using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using FDTD2DLab.Services.Interfaces;

using MathCore.ViewModels;
using MathCore.WPF.Commands;

namespace FDTD2DLab.ViewModels
{
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

        public ObservableCollection<FileInfo> RecentFiles { get; } = new();

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
            file ??= _UserDialog.OpenFile("Открыть проект", "Файлы проекта (*.fdtdproj)|Xml-файлы (*.xml)|*.*|Все файлы (*.*)|*.*");
            if (file is null) return;

            RecentFiles.Remove(file);
            RecentFiles.Insert(0, file);
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
            "Файлы проекта (*.fdtdproj)|Xml-файлы (*.xml)|*.*|Все файлы (*.*)|*.*",
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
            var new_file = GetnewProjectFile(file);
            Status = "Проект сохранён";

            Status = $"Проект сохранён в {new_file.Name}";
        }

        #endregion
    }
}
