using System.Windows;
using FDTD2DLab.Services.Interfaces;
using MathCore.ViewModels;

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
    }
}
