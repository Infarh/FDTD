using FDTD2DLab.ViewModels.Probe;
using FDTD2DLab.ViewModels.Source;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FDTD2DLab.Infrastructure.Serialization
{
    public class GridData
    {
        public int Nx { get; set; }
        public int Ny { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }
        public double Dt { get; set; }
        public string SpaceUnit { get; set; }
        public string BackgroundMaterialName { get; set; }
        public List<ShapeData> Shapes { get; set; } = new();
    }

    public class ShapeData
    {
        public string Type { get; set; } // "Rect" or "Ellipse"
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Angle { get; set; }
        public string MaterialName { get; set; } // ссылка на материал
        public double Eps { get; set; } // если материал не задан
        public double Mu { get; set; }
        public double Sigma { get; set; }
        public bool? UseCornerCoords { get; set; } // null для эллипсов
    }
    public class MaterialData
    {
        public string Name { get; set; }
        public double Eps { get; set; }
        public double Mu { get; set; }
        public double Sigma { get; set; }
    }
    public class SourceData
    {
        public string Type { get; set; } // "Point" or "PlaneWave"
        public double X { get; set; } // для точечного
        public double Y { get; set; }
        public double Position { get; set; } // для плоской волны
        public double Start { get; set; }
        public double End { get; set; }
        public bool IsHorizontal { get; set; }
        public string Name { get; set; }
        public SignalType SignalType { get; set; }
        public double Amplitude { get; set; }
        public double T0 { get; set; }
        public double Tau { get; set; }
        public double Frequency { get; set; }
        public double Phase { get; set; }
    }
    public class ProbeData
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public FieldComponent Component { get; set; }
    }
}