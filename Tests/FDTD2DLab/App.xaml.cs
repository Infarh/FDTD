using FDTD2DLab.Services;
using FDTD2DLab.Services.Interfaces;
using FDTD2DLab.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace FDTD2DLab;

public partial class App : Application
{
    public static Window FocusedWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsFocused);
    public static Window ActiveWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
    public static Window CurrentWindow => FocusedWindow ?? ActiveWindow;

    private static IHost __Hosting;

    public static IHost Hosting => __Hosting ??= CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

    public static IServiceProvider Services => Hosting.Services;

    public static IHostBuilder CreateHostBuilder(string[] args) => Host
       .CreateDefaultBuilder(args)
       .ConfigureServices(ConfigureServices);

    private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<IUserDialog, UserDialog>();
        services.AddTransient<IComputer, Computer>();
    }


    #region startup
    protected override void OnStartup(StartupEventArgs e)
    {
        UnregisterFileAssociations();
        // Проверяем, нужно ли регистрировать ассоциации
        if (!IsRegistered())
        {
            if (!IsAdministrator())
            {
                RestartAsAdministrator();
                return;
            }
            RegisterFileAssociations();
            SetRegisteredFlag();
        }

        base.OnStartup(e);
    }

    private bool IsAdministrator()
    {
        using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
        {
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
    }

    private void RestartAsAdministrator()
    {
        var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location)
        {
            UseShellExecute = true,
            Verb = "runas"
        };
        Process.Start(processInfo);
        Current.Shutdown();
    }

    private bool IsRegistered()
    {
        using (var key = Registry.ClassesRoot.OpenSubKey(".gmfdtd"))
        {
            return key != null;
        }
    }

    private void SetRegisteredFlag()
    {
        using (var key = Registry.CurrentUser.CreateSubKey(@"Software\FDTDLab"))
        {
            key.SetValue("Registered", 1);
        }
    }

    private void RegisterFileAssociations()
    {
        string appPath = Assembly.GetExecutingAssembly().Location;
        appPath.Replace("FDTD2DLab.dll", "FDTD2DLab.exe");
        string appDir = Path.GetDirectoryName(appPath);
        string resourcesDir = Path.Combine(appDir, "Resources");

        var extensions = new[]
        {
        new { Ext = ".gmfdtd", ProgId = "FDTD.Grid", Desc = "Файл сетки FDTD", Icon = Path.Combine(resourcesDir, "grid.ico") },
        new { Ext = ".mmfdtd", ProgId = "FDTD.Materials", Desc = "Файл материалов FDTD", Icon = Path.Combine(resourcesDir, "materials.ico") },
        new { Ext = ".smfdtd", ProgId = "FDTD.Sources", Desc = "Файл источников FDTD", Icon = Path.Combine(resourcesDir, "sources.ico") },
        new { Ext = ".pmfdtd", ProgId = "FDTD.Probes", Desc = "Файл зондов FDTD", Icon = Path.Combine(resourcesDir, "probes.ico") }
        };

        foreach (var ext in extensions)
        {
            // Проверяем, существует ли файл иконки
            if (!File.Exists(ext.Icon))
            {
                Debug.WriteLine($"Иконка не найдена: {ext.Icon}. Регистрация пропущена.");
                continue; // или можно выбросить исключение
            }

            using (var extKey = Registry.ClassesRoot.CreateSubKey(ext.Ext))
                extKey.SetValue("", ext.ProgId);

            using (var progKey = Registry.ClassesRoot.CreateSubKey(ext.ProgId))
            {
                progKey.SetValue("", ext.Desc);
                using (var iconKey = progKey.CreateSubKey("DefaultIcon"))
                    iconKey.SetValue("", ext.Icon);
                using (var shellKey = progKey.CreateSubKey("shell"))
                using (var openKey = shellKey.CreateSubKey("open"))
                using (var commandKey = openKey.CreateSubKey("command"))
                    commandKey.SetValue("", $"\"{appPath}\" \"%1\"");
            }
        }

        // Уведомляем систему об изменениях
        SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
    }

    private string ExtractIconFromResource(string resourceName)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), resourceName);
        using (var stream = Application.GetResourceStream(new Uri($"pack://application:,,,/Resources/{resourceName}")).Stream)
        using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
        {
            stream.CopyTo(fileStream);
        }
        return tempPath;
    }
    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    #endregion


    public static void UnregisterFileAssociations()
    {
        string[] progIds = { "FDTD.Grid", "FDTD.Materials", "FDTD.Sources", "FDTD.Probes" };
        foreach (var progId in progIds)
        {
            Registry.ClassesRoot.DeleteSubKeyTree(progId, false);
        }
        string[] extensions = { ".gmfdtd", ".mmfdtd", ".smfdtd", ".pmfdtd" };
        foreach (var ext in extensions)
        {
            Registry.ClassesRoot.DeleteSubKeyTree(ext, false);
        }
        SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
    }


}
