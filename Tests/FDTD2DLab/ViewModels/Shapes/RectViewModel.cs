using System;
using FDTD2DLab.ViewModels.Propertys;

namespace FDTD2DLab.ViewModels.Shapes;

public class RectViewModel : ShapeViewModel
{
    private double _x2, _y2;
    private bool _updatingCorners;
    private bool _updatingSize;
    private bool _useCornerCoords;

    public double X2 { get => _x2; set { if (Set(ref _x2, value) && !_updatingSize) { _updatingCorners = true; UpdateSizeFromCorners(); _updatingCorners = false; } } }
    public double Y2 { get => _y2; set { if (Set(ref _y2, value) && !_updatingSize) { _updatingCorners = true; UpdateSizeFromCorners(); _updatingCorners = false; } } }

    public bool UseCornerCoords
    {
        get => _useCornerCoords;
        set
        {
            if (Set(ref _useCornerCoords, value))
            {
                // При переключении сразу синхронизируем значения
                if (value) UpdateCornersFromSize();
                else UpdateSizeFromCorners();
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(X2));
                OnPropertyChanged(nameof(Y2));
            }
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        if (_updatingCorners) return;

        if (propertyName == nameof(X) || propertyName == nameof(Y) ||
            propertyName == nameof(Width) || propertyName == nameof(Height) ||
            propertyName == nameof(Anchor))
        {
            _updatingSize = true;
            if (UseCornerCoords) UpdateCornersFromSize();
            _updatingSize = false;
        }
        else if (propertyName == nameof(X2) || propertyName == nameof(Y2))
        {
            _updatingSize = true;
            if (UseCornerCoords) UpdateSizeFromCorners();
            _updatingSize = false;
        }
    }

    private void UpdateCornersFromSize()
    {
        if (Anchor == AnchorPoint.BottomLeft)
        {
            X2 = X + Width;
            Y2 = Y + Height;
        }
        else
        {
            X2 = X + Width / 2;
            Y2 = Y + Height / 2;
        }
    }

    private void UpdateSizeFromCorners()
    {
        if (Anchor == AnchorPoint.BottomLeft)
        {
            Width = Math.Abs(X2 - X);
            Height = Math.Abs(Y2 - Y);
            if (X2 < X) { (X, X2) = (X2, X); }
            if (Y2 < Y) { (Y, Y2) = (Y2, Y); }
        }
        else
        {
            double newWidth = Math.Abs(X2 - X) * 2;
            double newHeight = Math.Abs(Y2 - Y) * 2;
            double centerX = (X + X2) / 2;
            double centerY = (Y + Y2) / 2;
            X = centerX;
            Y = centerY;
            Width = newWidth;
            Height = newHeight;
        }
    }
}