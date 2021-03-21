namespace FDTD2DLab.ViewModels.Shapes
{
    public abstract class SizableViewModel : ShapeViewModel
    {
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
    }
}