using MathCore.WPF.ViewModels;

namespace FDTD2DLab.ViewModels.Shapes
{
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
    }
}
