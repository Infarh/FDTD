using FDTD2DLab.Infrastructure.Helpers;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FDTD2DLab.Views
{
    /// <summary>
    /// Логика взаимодействия для MultiProbePlotView.xaml
    /// </summary>
    public partial class MultiProbePlotView : UserControl
    {
        public MultiProbePlotView()
        {
            InitializeComponent();
        }

        private void PlotView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not OxyPlot.Wpf.PlotView plotView) return;
            if (plotView.ActualModel is not PlotModel model) return;

            var position = e.GetPosition(plotView);
            var xAxis = model.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            var yAxis = model.Axes.FirstOrDefault(a => a.Position == AxisPosition.Left);
            if (xAxis == null || yAxis == null) return;

            var dataPoint = xAxis.InverseTransform(position.X, position.Y, yAxis);
            PlotHelper.AddMarker(model, dataPoint);
            model.InvalidatePlot(true);
        }
    }
}
