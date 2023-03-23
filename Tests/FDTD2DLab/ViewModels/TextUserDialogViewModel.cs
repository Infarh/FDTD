using System;
using System.Windows.Input;

using MathCore.ViewModels;
using MathCore.WPF.Commands;

namespace FDTD2DLab.ViewModels;

internal class TextUserDialogViewModel : ViewModel
{
    public event EventHandler<EventArgs<bool?>> Completed;

    protected virtual void OnCompeted(bool? Result) => Completed?.Invoke(this, Result);

    #region Title : string - Заголовок

    /// <summary>Заголовок</summary>
    private string _Title = "Введите строку";

    /// <summary>Заголовок</summary>
    public string Title { get => _Title; set => Set(ref _Title, value); }

    #endregion

    #region Caption : string - Сообщение

    /// <summary>Сообщение</summary>
    private string _Caption = "Текст";

    /// <summary>Сообщение</summary>
    public string Caption { get => _Caption; set => Set(ref _Caption, value); }

    #endregion

    #region Value : string - Значение

    /// <summary>Значение</summary>
    private string _Value;

    /// <summary>Значение</summary>
    public string Value { get => _Value; set => Set(ref _Value, value); }

    #endregion

    #region Command CompleteCommand - Завершение диалога

    /// <summary>Завершение диалога </summary>
    private LambdaCommand _CompleteCommand;

    /// <summary>Завершение диалога </summary>
    public ICommand CompleteCommand => _CompleteCommand ??= new(OnCompleteCommandExecuted, CanCompleteCommandExecute);

    /// <summary>Проверка возможности выполнения - Завершение диалога </summary>
    protected virtual bool CanCompleteCommandExecute(object p) => true;

    /// <summary>Логика выполнения - Завершение диалога </summary>
    protected virtual void OnCompleteCommandExecuted(object p) =>
        OnCompeted(p switch
        {
            null => null,
            bool result => result,
            _ => Convert.ToBoolean(p)
        });

    #endregion

    #region Command CommitCommand - Принять

    /// <summary>Принять</summary>
    private LambdaCommand _CommitCommand;

    /// <summary>Принять</summary>
    public ICommand CommitCommand => _CommitCommand ??= new(OnCommitCommandExecuted, CanCommitCommandExecute);

    /// <summary>Проверка возможности выполнения - Принять</summary>
    protected virtual bool CanCommitCommandExecute() => true;

    /// <summary>Логика выполнения - Принять</summary>
    protected virtual void OnCommitCommandExecuted() => OnCompeted(true);

    #endregion

    #region Command CancelCommand - Принять

    /// <summary>Принять</summary>
    private LambdaCommand _CancelCommand;

    /// <summary>Принять</summary>
    public ICommand CancelCommand => _CancelCommand ??= new(OnCancelCommandExecuted, CanCancelCommandExecute);

    /// <summary>Проверка возможности выполнения - Принять</summary>
    protected virtual bool CanCancelCommandExecute() => true;

    /// <summary>Логика выполнения - Принять</summary>
    protected virtual void OnCancelCommandExecuted() => OnCompeted(true);

    #endregion
}
