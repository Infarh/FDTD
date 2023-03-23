using FDTD2DLab.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FDTD2DLab;

public class ServiceLocator
{
    public MainWindowViewModel MainWindowModel => App.Services.GetRequiredService<MainWindowViewModel>();
}
