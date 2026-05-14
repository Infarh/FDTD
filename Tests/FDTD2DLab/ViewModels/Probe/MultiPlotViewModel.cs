using DynamicData;
using FDTD2DLab.ViewModels.Probe;
using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace FDTD2DLab.ViewModels
{
    public class MultiPlotViewModel : ViewModel
    {
        private readonly List<ProbeViewModel> _probes = new();
        private PlotModel _plotModel;

        public PlotModel PlotModel
        {
            get => _plotModel;
            private set => Set(ref _plotModel, value);
        }

        public ICommand ExportCsvCommand { get; }
        public ICommand ExportJsonCommand { get; }

        public MultiPlotViewModel()
        {
            ExportCsvCommand = new LambdaCommand(ExportCsv);
            ExportJsonCommand = new LambdaCommand(ExportJson);
            PlotModel = new PlotModel { Title = "Сводный график зондов" };
            PlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, Title = "Время (с)" });
            PlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, Title = "Значение" });
        }

        public void AddProbe(ProbeViewModel probe)
        {
            if (_probes.Contains(probe)) return;
            _probes.Add(probe);

            var series = new LineSeries { Title = probe.Name };
            for (int i = 0; i < probe.TimeValues.Count; i++)
                series.Points.Add(new DataPoint(probe.TimeValues[i], probe.FieldValues[i]));
            PlotModel.Series.Add(series);
            PlotModel.InvalidatePlot(true);
        }

        public void RemoveProbe(ProbeViewModel probe)
        {
            _probes.Remove(probe);
            var series = PlotModel.Series.OfType<LineSeries>()
                .FirstOrDefault(s => s.Title == probe.Name);
            if (series != null)
            {
                PlotModel.Series.Remove(series);
                PlotModel.InvalidatePlot(true);
            }
        }

        private string GetCsvData()
        {
            if (_probes.Count == 0) return string.Empty;

            var allTimes = new SortedSet<double>();
            foreach (var p in _probes)
                foreach (var t in p.TimeValues)
                    allTimes.Add(t);

            var lines = new List<string>();
            // Заголовок
            var header = new List<string> { "Время" };
            header.AddRange(_probes.Select(p => p.Name));
            lines.Add(string.Join(",", header));

            foreach (double time in allTimes)
            {
                var row = new List<string> { time.ToString("G") };
                foreach (var p in _probes)
                {
                    int idx = p.TimeValues.IndexOf(time);
                    row.Add(idx >= 0 ? p.FieldValues[idx].ToString("G") : "");
                }
                lines.Add(string.Join(",", row));
            }
            return string.Join(Environment.NewLine, lines);
        }

        private void ExportCsv()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = ".csv"
            };
            if (dlg.ShowDialog() == true)
                File.WriteAllText(dlg.FileName, GetCsvData());
        }

        private void ExportJson()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = ".json"
            };
            if (dlg.ShowDialog() != true) return;

            // Формат: массив объектов { Time, SeriesName1, SeriesName2, ... }
            var allTimes = new SortedSet<double>();
            foreach (var p in _probes)
                foreach (var t in p.TimeValues)
                    allTimes.Add(t);

            var data = new List<Dictionary<string, object>>();
            foreach (double time in allTimes)
            {
                var dict = new Dictionary<string, object> { ["Time"] = time };
                foreach (var p in _probes)
                {
                    int idx = p.TimeValues.IndexOf(time);
                    dict[p.Name] = idx >= 0 ? p.FieldValues[idx] : null;
                }
                data.Add(dict);
            }
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dlg.FileName, json);
        }
    }
}