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
        }

        private readonly IUserDialog _UserDialog;
        private readonly IComputer _Computer;

        public GridViewModel Grid { get; }

        #region Title : string - Заголовок

        /// <summary>Заголовок</summary>
        private string _Title = "FDTD";

        /// <summary>Заголовок</summary>
        public string Title { get => _Title; set => Set(ref _Title, value); }

        #endregion

        #region Command ExitCommand - Выход из приложения

        /// <summary>Выход из приложения</summary>
        private ICommand _ExitCommand;

        /// <summary>Выход из приложения</summary>
        public ICommand ExitCommand => _ExitCommand ??= Command.Exec(() => Application.Current.MainWindow!.Close());

        #endregion

        #region Command CreateCommand - Создать

        /// <summary>Создать</summary>
        private LambdaCommand _CreateCommand;

        /// <summary>Создать</summary>
        public ICommand CreateCommand => _CreateCommand ??= new(OnCreateCommandExecuted, CanCreateCommandExecute);

        /// <summary>Проверка возможности выполнения - Создать</summary>
        private bool CanCreateCommandExecute() => true;

        /// <summary>Логика выполнения - Создать</summary>
        private void OnCreateCommandExecuted()
        {
            
        }

        #endregion

        #region Command OpenCommand - Открыть

        /// <summary>Открыть</summary>
        private LambdaCommand _OpenCommand;

        /// <summary>Открыть</summary>
        public ICommand OpenCommand => _OpenCommand ??= new(OnOpenCommandExecuted, CanOpenCommandExecute);

        /// <summary>Проверка возможности выполнения - Открыть</summary>
        private bool CanOpenCommandExecute() => true;

        /// <summary>Логика выполнения - Открыть</summary>
        private void OnOpenCommandExecuted()
        {

        }

        #endregion

        #region Command SaveCommand - Сохранить

        /// <summary>Сохранить</summary>
        private LambdaCommand _SaveCommand;

        /// <summary>Сохранить</summary>
        public ICommand SaveCommand => _SaveCommand ??= new(OnSaveCommandExecuted, CanSaveCommandExecute);

        /// <summary>Проверка возможности выполнения - Сохранить</summary>
        private bool CanSaveCommandExecute() => true;

        /// <summary>Логика выполнения - Сохранить</summary>
        private void OnSaveCommandExecuted()
        {

        }

        #endregion

        #region Command SaveAsCommand - Сохранить как

        /// <summary>Сохранить как</summary>
        private LambdaCommand _SaveAsCommand;

        /// <summary>Сохранить как</summary>
        public ICommand SaveAsCommand => _SaveAsCommand ??= new(OnSaveAsCommandExecuted, CanSaveAsCommandExecute);

        /// <summary>Проверка возможности выполнения - Сохранить как</summary>
        private bool CanSaveAsCommandExecute() => true;

        /// <summary>Логика выполнения - Сохранить как</summary>
        private void OnSaveAsCommandExecuted()
        {

        }

        #endregion

    }
}
