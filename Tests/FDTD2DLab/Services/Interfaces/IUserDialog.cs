using System.IO;

namespace FDTD2DLab.Services.Interfaces;

public interface IUserDialog
{
    FileInfo OpenFile(string Title, string Filter = "Все файлы (*.*)|*.*", string DefaultFilePath = null);
    FileInfo SaveFile(string Title, string Filter = "Все файлы (*.*)|*.*", string DefaultFilePath = null);
    string GetString(string Title, string Caption, string Default = null);
    bool YesNoQuestion(string Text, string Title = "Вопрос...");
    bool OkCancelQuestion(string Text, string Title = "Вопрос...");
    void Information(string Text, string Title = "Вопрос...");
    void Warning(string Text, string Title = "Вопрос...");
    void Error(string Text, string Title = "Вопрос...");
}
