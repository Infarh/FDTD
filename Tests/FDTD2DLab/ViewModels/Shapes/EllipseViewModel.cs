using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace FDTD2DLab.ViewModels.Shapes;

public class EllipseViewModel : ShapeViewModel 
{
    [JsonProperty]
    public double test { get; set; }

}
