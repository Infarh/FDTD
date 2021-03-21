using System;
using System.IO;

using FDTD2DLab.Services.Interfaces;
using FDTD2DLab.ViewModels;
using FDTD2DLab.Views;
using Microsoft.Win32;

namespace FDTD2DLab.Services
{
    public class UserDialog : IUserDialog
    {
        public FileInfo OpenFile(string Title, string Filter = "Все файлы (*.*)|*.*", string DefaultFilePath = null)
        {
            var dialog = new OpenFileDialog()
            {
                Title = Title,
                RestoreDirectory = true,
                Filter = Filter ?? throw new ArgumentNullException(nameof(Filter)),
            };
            if (DefaultFilePath is { Length: > 0 })
                dialog.FileName = DefaultFilePath;

            return dialog.ShowDialog(App.CurrentWindow) == true 
                ? new(dialog.FileName) 
                : DefaultFilePath is null ? null : new(DefaultFilePath);
        }

        public string GetString(string Title, string Caption, string Default = null)
        {
            var model = new TextUserDialogViewModel
            {
                Title = Title,
                Caption = Caption,
                Value = Default
            };
            var dialog = new TextUserDialogWindow
            {
                DataContext = model,
                Owner = App.CurrentWindow
            };
            model.Completed += (_, e) => dialog.DialogResult = e;
            return dialog.ShowDialog() == true ? model.Value : Default;
        }
    }
}
