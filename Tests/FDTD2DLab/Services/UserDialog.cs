using System;
using System.IO;
using System.Windows;
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
            var dialog = new OpenFileDialog
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

        public FileInfo SaveFile(string Title, string Filter = "Все файлы (*.*)|*.*", string DefaultFilePath = null)
        {
            var dialog = new SaveFileDialog
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
            return dialog.ShowDialog() == true ? model.Value : null;
        }

        public bool YesNoQuestion(string Text, string Title = "Вопрос...")
        {
            var result = MessageBox.Show(App.CurrentWindow, Text, Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public bool OkCancelQuestion(string Text, string Title = "Вопрос...")
        {
            var result = MessageBox.Show(App.CurrentWindow, Text, Title, MessageBoxButton.OKCancel, MessageBoxImage.Question);
            return result == MessageBoxResult.OK;
        }

        public void Information(string Text, string Title = "Вопрос...") => 
            MessageBox.Show(App.CurrentWindow, Text, Title, MessageBoxButton.OK, MessageBoxImage.Information);

        public void Warning(string Text, string Title = "Вопрос...") => 
            MessageBox.Show(App.CurrentWindow, Text, Title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void Error(string Text, string Title = "Вопрос...") => 
            MessageBox.Show(App.CurrentWindow, Text, Title, MessageBoxButton.OK, MessageBoxImage.Error);

        public IProgressInfo Progress(string Title)
        {
            var progress_model = new ProgressViewModel
            {
                Title = Title
            };

            var view = new ProgressWindow
            {
                DataContext = progress_model
            };
            progress_model.Disposed += (_, _) => view.Close();
            view.Closing += (_, e) => e.Cancel = !progress_model.CancelCommand.CanExecute(null);
            view.Closed += (_, _) => progress_model.CancelCommand.Execute(null);

            view.Show();

            return progress_model;
        }
    }
}
