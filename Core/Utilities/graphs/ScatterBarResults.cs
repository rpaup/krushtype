using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using krushtype.Core.Models;
using System.Diagnostics.Tracing;

namespace krushtype.Core.Utilities.graphs
{
    public class ScatterGraphResults
    {
        public PlotModel MyModel { get; }
        public PlotController customController { get; private set; }
        private LinearAxis xAxis;
        private LinearAxis yAxis1;
        private LinearAxis yAxis2;
        public LineSeries WPMSeries { get; private set; }
        public LineSeries RawWPMSeries { get; private set; }
        public ScatterSeries MistakesSeries { get; private set; }
        public ScatterGraphResults(List<PeriodData> period_tests)
        {
            MyModel = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderThickness = new OxyThickness(0.4),
                PlotAreaBorderColor = OxyColor.Parse("#2c2e31")
            };

            xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                TitleFontSize = 12,
                TitleFontWeight = FontWeights.Bold,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.Parse("#2c2e31"),
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColor.Parse("#646669"),
                TitleColor = OxyColor.Parse("#646669"),
                TickStyle = TickStyle.None,
                MajorStep = 1,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MajorGridlineThickness = 0.4
            };
            yAxis1 = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Words per minute",
                TitleFontSize = 12,
                TitleFontWeight = FontWeights.Bold,
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColor.Parse("#646669"),
                TitleColor = OxyColor.Parse("#646669"),
                AxislineColor = OxyColor.Parse("#2c2e31"),
                TickStyle = TickStyle.None,
                Key = "YAxis1",
                Minimum = 0,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MajorStep = 40,
            };
            yAxis2 = new LinearAxis
            {
                Position = AxisPosition.Right,
                Title = "sɹoɹɹǝ",
                TitleFontSize = 12,
                TitleFontWeight = FontWeights.Bold,
                MajorGridlineColor = OxyColor.Parse("#2c2e31"),
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColor.Parse("#646669"),
                TitleColor = OxyColor.Parse("#646669"),
                AxislineColor = OxyColor.Parse("#2c2e31"),
                TickStyle = TickStyle.None,
                Key = "YAxis2",
                Minimum = 0,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MajorGridlineStyle = LineStyle.None
            };
            MyModel.Axes.Add(xAxis);
            MyModel.Axes.Add(yAxis1);
            MyModel.Axes.Add(yAxis2);

            WPMSeries = new LineSeries
            {
                Color = OxyColor.Parse("#84a0c6"),
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3, 
                MarkerFill = OxyColor.Parse("#84a0c6"),
                YAxisKey = "YAxis1",
                TrackerFormatString = "wpm: {4}"
            };


            RawWPMSeries = new LineSeries
            {
                Color = OxyColor.Parse("#646669"),
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColor.Parse("#646669"),
                YAxisKey = "YAxis1",
                TrackerFormatString = "raw: {4}"
            };

            MistakesSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Cross,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Red,
                MarkerFill = OxyColors.Red,
                YAxisKey = "YAxis2",
                TrackerFormatString = "Errors: {4}"
            };

            MyModel.Series.Add(WPMSeries);
            MyModel.Series.Add(RawWPMSeries);
            MyModel.Series.Add(MistakesSeries);

            AddPoints(period_tests);


            customController = new PlotController();
            customController.UnbindMouseDown(OxyMouseButton.Left);
            customController.BindMouseEnter(PlotCommands.HoverSnapTrack);
        }
        public void AddPoints(List<PeriodData> Period_tests)
        {
            foreach (var i in Period_tests)
            {
                WPMSeries.Points.Add(new DataPoint(Convert.ToInt16(i.Time.TotalSeconds), Convert.ToInt32(i.WPM)));
                RawWPMSeries.Points.Add(new DataPoint(Convert.ToInt16(i.Time.TotalSeconds), Convert.ToInt32(i.RawWPM)));
                if (i.Mistakes > 0)
                {
                    MistakesSeries.Points.Add(new ScatterPoint(Convert.ToInt16(i.Time.TotalSeconds), i.Mistakes));
                }
            }
        }
    }
}
