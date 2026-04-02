using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDTD2DLab.ViewModels.Source
{
    public class PointSourceViewModel : SourceViewModel
    {
        private double _x;
        private double _y;

        public double X
        {
            get => _x;
            set => SetField(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetField(ref _y, value);
        }

        public override string TypeDisplayName => "Точечный источник";
    }
}
