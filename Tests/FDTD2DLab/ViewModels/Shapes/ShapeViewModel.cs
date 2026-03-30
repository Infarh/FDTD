using FDTD2DLab.ViewModels.Material;
using FDTD2DLab.ViewModels.Propertys;
using MathCore.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace FDTD2DLab.ViewModels.Shapes;


[JsonDerivedType(typeof(RectViewModel))]
[JsonDerivedType(typeof(EllipseViewModel))]
public abstract class ShapeViewModel : ViewModel, IOptProperty
{

    private string _name = "Фигура";

    public string Name { get => _name; set => SetValue(ref _name, value); }

    [JsonIgnore]
    private Type _ShapeType;
    [JsonIgnore]
    public Type ShapeType { get => _ShapeType; set => Set(ref _ShapeType, value); }

    private string _ShapeName;

    public string ShapeName { get => _ShapeName; set => Set(ref _ShapeName, value, v => 1 <= v.Length && v.Length <= 12); }

    private MaterialViewModel _appliedMaterial;

    private bool _isApplyingMaterial; // флаг для предотвращения рекурсии

    public MaterialViewModel AppliedMaterial
    {
        get => _appliedMaterial;
        set
        {
            if (Set(ref _appliedMaterial, value))
            {
                if (value != null)
                {
                    // Применяем параметры материала к фигуре
                    _isApplyingMaterial = true;
                    Eps = value.Eps;
                    Mu = value.Mu;
                    Sigma = value.Sigma;
                    _isApplyingMaterial = false;
                }
            }
        }
    }

    //public event PropertyChangedEventHandler PropertyChanged;

    //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}

    //protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    //{
    //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
    //    field = value;
    //    OnPropertyChanged(propertyName);
    //    return true;
    //}

    #region Eps : double - Диэлектрическая проницаемость

    /// <summary>Диэлектрическая проницаемость</summary>
    private double _Eps = 1;

    /// <summary>Диэлектрическая проницаемость</summary>
    public double Eps {
        get => _Eps;
        set
        {
            if (Set(ref _Eps, value))
            {
                // Если изменение не вызвано применением материала и текущий материал не совпадает по этому параметру,
                // сбрасываем привязку к материалу
                if (!_isApplyingMaterial && AppliedMaterial != null && Math.Abs(AppliedMaterial.Eps - value) > 1e-12)
                    AppliedMaterial = null;
            }
        }
    }

    #endregion

    #region Mu : double - Магнитная проницаемость

    /// <summary>Магнитная проницаемость</summary>
    private double _Mu = 1;

    /// <summary>Магнитная проницаемость</summary>
    public double Mu {
        get => _Mu;
        set
        {
            if (Set(ref _Mu, value))
            {
                if (!_isApplyingMaterial && AppliedMaterial != null && Math.Abs(AppliedMaterial.Mu - value) > 1e-12)
                    AppliedMaterial = null;
            }
        }
    }

    #endregion

    #region Sigma : double - Проводимость

    /// <summary>Проводимость</summary>
    private double _Sigma;

    /// <summary>Проводимость</summary>
    public double Sigma {
        get => _Sigma;
        set
        {
            if (Set(ref _Sigma, value))
            {
                if (!_isApplyingMaterial && AppliedMaterial != null && Math.Abs(AppliedMaterial.Sigma - value) > 1e-12)
                    AppliedMaterial = null;
            }
        }
    }

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
