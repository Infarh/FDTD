using System.IO;

namespace FDTD2DLab.Services.Interfaces
{
    public interface IUserDialog
    {
        FileInfo OpenFile(string Title, string Filter = "Все файлы (*.*)|*.*", string DefaultFilePath = null);
        string GetString(string Title, string Caption, string Default = null);
    }
}
