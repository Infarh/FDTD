using FDTD2DLab.ViewModels.Probe;
using FDTD2DLab.ViewModels.Shapes;
using FDTD2DLab.ViewModels.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FDTD2DLab.Infrastructure.Behaviors
{
    public class ObjectTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element == null) return null;

            if (item is RectViewModel)
                return element.FindResource("RectTemplate") as DataTemplate;
            if (item is EllipseViewModel)
                return element.FindResource("EllipseTemplate") as DataTemplate;
            if (item is PointSourceViewModel)
                return element.FindResource("PointSourceTemplate") as DataTemplate;
            if (item is PlaneWaveSourceViewModel)
                return element.FindResource("PlaneWaveTemplate") as DataTemplate;
            if (item is ProbeViewModel)
                return element.FindResource("ProbeTemplate") as DataTemplate;

            return null;
        }
    }
}
