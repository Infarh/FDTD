using MathCore.WPF.ViewModels;

namespace FDTD2DLab.ViewModels.Shapes;

public abstract class ShapeViewModel : ViewModel
{
    #region Eps : double - Диэлектрическая проницаемость

    /// <summary>Диэлектрическая проницаемость</summary>
    private double _Eps = 1;

    /// <summary>Диэлектрическая проницаемость</summary>
    public double Eps { get => _Eps; set => Set(ref _Eps, value, v => v >= 1); }

    #endregion

    #region Mu : double - Магнитная проницаемость

    /// <summary>Магнитная проницаемость</summary>
    private double _Mu = 1;

    /// <summary>Магнитная проницаемость</summary>
    public double Mu { get => _Mu; set => Set(ref _Mu, value, v => v >= 1); }

    #endregion

    #region Sigma : double - Проводимость

    /// <summary>Проводимость</summary>
    private double _Sigma;

    /// <summary>Проводимость</summary>
    public double Sigma { get => _Sigma; set => Set(ref _Sigma, value, v => v >= 0); }

    #endregion

    #region X : double - Положение по горизонтали

    /// <summary>Положение по горизонтали</summary>
    private double _X;

    /// <summary>Положение по горизонтали</summary>
    public double X { get => _X; set => Set(ref _X, value); }

    #endregion

    #region Y : double - Положение по вертикали

    /// <summary>Положение по вертикали</summary>
    private double _Y;

    /// <summary>Положение по вертикали</summary>
    public double Y { get => _Y; set => Set(ref _Y, value); }

    #endregion

    #region Width : double - Размер

    /// <summary>Размер</summary>
    private double _Width = 10;

    /// <summary>Размер</summary>
    public double Width { get => _Width; set => Set(ref _Width, value); }

    #endregion

    #region Height : double - Размер

    /// <summary>Размер</summary>
    private double _Height = 10;

    /// <summary>Размер</summary>
    public double Height { get => _Height; set => Set(ref _Height, value); }

    #endregion

    #region Angle : double - Угол поворота в градусах

    /// <summary>Угол поворота в градусах</summary>
    private double _Angle;

    /// <summary>Угол поворота в градусах</summary>
    public double Angle { get => _Angle; set => Set(ref _Angle, value); }

    #endregion

    #region IsSelected : bool - Модель выбрана

    /// <summary>Модель выбрана</summary>
    private bool _IsSelected;

    /// <summary>Модель выбрана</summary>
    public bool IsSelected { get => _IsSelected; set => Set(ref _IsSelected, value); }

    #endregion
}
