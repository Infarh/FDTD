using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDTD2DLab.Infrastructure.Helpers
{
    public static class PlotHelper
    {
        /// <summary>
        /// Добавляет маркер (точку и подпись) в указанную точку данных на графике.
        /// </summary>
        public static void AddMarker(PlotModel plotModel, DataPoint dataPoint)
        {
            // Точка (маркер)
            var pointSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerFill = OxyColor.FromRgb(255, 0, 0)
            };
            pointSeries.Points.Add(new ScatterPoint(dataPoint.X, dataPoint.Y));
            plotModel.Series.Add(pointSeries);

            // Форматирование чисел в экспоненциальном виде (x.xxe±yy)
            string FormatExp(double value) => value.ToString("0.##e0");

            string text = $"({FormatExp(dataPoint.X)}, {FormatExp(dataPoint.Y)})";

            var annotation = new TextAnnotation
            {
                TextPosition = dataPoint,
                Text = text,
                TextColor = OxyColor.FromRgb(0, 0, 0),
                Background = OxyColor.FromArgb(200, 255, 255, 255),
                StrokeThickness = 1,
                TextHorizontalAlignment = HorizontalAlignment.Left,
                TextVerticalAlignment = VerticalAlignment.Top
            };
            plotModel.Annotations.Add(annotation);
        }
    }
}
