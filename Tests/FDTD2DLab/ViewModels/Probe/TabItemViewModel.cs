using MathCore.WPF.ViewModels;
using System.Windows.Input;

namespace FDTD2DLab.ViewModels.Probe
{
    public class TabItemViewModel : ViewModel
    {
        private string _header;
        private object _content;
        private bool _canClose;
        private object _tag;
        private ICommand _closeCommand;

        public string Header { get => _header; set => Set(ref _header, value); }
        public object Content { get => _content; set => Set(ref _content, value); }
        public bool CanClose { get => _canClose; set => Set(ref _canClose, value); }
        public object Tag { get => _tag; set => Set(ref _tag, value); }
        public ICommand CloseCommand { get => _closeCommand; set => Set(ref _closeCommand, value); }
    }
}