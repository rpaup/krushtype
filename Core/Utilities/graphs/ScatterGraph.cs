using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using krushtype.Core.Utilities.files;

namespace krushtype.Core.Utilities.graphs
{
    public class ScatterGraph
    {
        public PlotModel MyModel { get; }
        public PlotController customController { get; private set; }
        private LinearAxis xAxis;
        private LinearAxis yAxis1;
        private LinearAxis yAxis2;
        public ScatterSeries series1 { get; private set; }
        public ScatterSeries series2 { get; private set; }
        public int tests_count { get; private set; }
        public int Max_WPM { get; private set; }
        public ScatterGraph(int tests_count, int Max_WPM, List<CSVData.TestResult> All_Tests)
        {
            this.tests_count = tests_count;
            this.Max_WPM = Max_WPM;
            MyModel = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderThickness = new OxyThickness(0.4),
                PlotAreaBorderColor = OxyColor.Parse("#2c2e31")
            };

            xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false,
                Minimum = 5,
                Maximum = this.tests_count * 10 + 5
            };
            yAxis1 = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Accuracy",
                TitleFontSize = 12,
                TitleFontWeight = FontWeights.Bold,
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColor.Parse("#646669"),
                TitleColor = OxyColor.Parse("#646669"),
                TickStyle = TickStyle.None,
                Key = "YAxis1",
                Minimum = 0,
                Maximum = 100,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MajorStep = 10,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                StartPosition = 1,
                EndPosition = 0,
            };
            yAxis2 = new LinearAxis
            {
                Position = AxisPosition.Right,
                Title = "??nu?? ??d sp?o?",
                TitleFontSize = 12,
                TitleFontWeight = FontWeights.Bold,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.Parse("#2c2e31"),
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColor.Parse("#646669"),
                TitleColor = OxyColor.Parse("#646669"),
                TickStyle = TickStyle.None,
                Key = "YAxis2",
                Minimum = 0,
                Maximum = this.Max_WPM,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MajorStep = 10,
                MajorGridlineThickness = 0.4,
            };
            MyModel.Axes.Add(xAxis);
            MyModel.Axes.Add(yAxis1);
            MyModel.Axes.Add(yAxis2);

            series1 = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColor.Parse("#84a0c6"),
                YAxisKey = "YAxis2",
                TrackerFormatString = "{Tag}"
            };

            series2 = new ScatterSeries
            {
                MarkerType = MarkerType.Triangle,
                MarkerSize = 3,
                MarkerFill = OxyColor.Parse("#646669"),
                YAxisKey = "YAxis1",
                TrackerFormatString = "{Tag}"
            };
            MyModel.Series.Add(series1);
            MyModel.Series.Add(series2);

            AddPoints(All_Tests);
            AddLines(series1);


            customController = new PlotController();
            customController.UnbindMouseDown(OxyMouseButton.Left);
            customController.BindMouseEnter(PlotCommands.HoverSnapTrack);
        }
        public void AddPoints(List<CSVData.TestResult> All_tests)
        {
            if (String.IsNullOrEmpty(All_tests.First().Mode)) return;
            int StepPoint = 10;
            foreach (var i in All_tests)
            {
                series1.Points.Add(new ScatterPoint(StepPoint, Convert.ToInt32(i.WPM)) { Tag = $"wpm: {i.WPM}\nraw: {i.RawWPM}\nacc: {i.Accuracy}\n\nmode: {i.Mode}\nnumbers: {i.IsNumbers}\npunctuation: {i.IsPunctuation}\nlanguage: {i.Language}\n\ndate: {i.Date.ToString("d MMMM yyyy HH:mm", new CultureInfo("en-EN"))}" });
                series2.Points.Add(new ScatterPoint(StepPoint, Convert.ToInt32(i.Accuracy)) { Tag = $"error rate: {100 - i.Accuracy}%\nacc: {i.Accuracy}%" });
                StepPoint += 10;
            }
        }
        public void AddLines(ScatterSeries series)
        {
            if (series.Points.Count == 0) return;
            ScatterPoint currentPoint = series.Points[0];
            foreach (var i in series.Points)
            {
                if (i.Y > currentPoint.Y)
                {
                    var Xline = new LineAnnotation
                    {
                        Type = LineAnnotationType.Horizontal,
                        MinimumX = currentPoint.X,
                        Y = currentPoint.Y,
                        MaximumX = i.X + 0.1,
                        MaximumY = currentPoint.Y,
                        Color = OxyColor.Parse("#646669"),
                        LineStyle = LineStyle.Solid,
                        YAxisKey = "YAxis2",
                        Layer = AnnotationLayer.BelowSeries,
                        StrokeThickness = 3
                    };
                    MyModel.Annotations.Add(Xline);
                    var Yline = new LineAnnotation
                    {
                        Type = LineAnnotationType.Vertical,
                        X = i.X,
                        MinimumY = currentPoint.Y,
                        MaximumX = i.X,
                        MaximumY = i.Y,
                        Color = OxyColor.Parse("#646669"),
                        LineStyle = LineStyle.Solid,
                        YAxisKey = "YAxis2",
                        Layer = AnnotationLayer.BelowSeries,
                        StrokeThickness = 3
                    };
                    MyModel.Annotations.Add(Yline);
                    currentPoint = i;
                }
                else
                {
                    var Xline = new LineAnnotation
                    {
                        Type = LineAnnotationType.Horizontal,
                        MinimumX = currentPoint.X,
                        Y = currentPoint.Y,
                        MaximumX = i.X,
                        MaximumY = currentPoint.Y,
                        Color = OxyColor.Parse("#646669"),
                        LineStyle = LineStyle.Solid,
                        YAxisKey = "YAxis2",
                        Layer = AnnotationLayer.BelowSeries,
                        StrokeThickness = 3
                    };
                }
            }
            var lastPoint = currentPoint;
            var finalXline = new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                MinimumX = lastPoint.X,
                Y = lastPoint.Y,
                MaximumX = MyModel.Axes[0].Maximum - 2.5,
                MaximumY = lastPoint.Y,
                Color = OxyColor.Parse("#646669"),
                LineStyle = LineStyle.Solid,
                YAxisKey = "YAxis2",
                Layer = AnnotationLayer.BelowSeries,
                StrokeThickness = 3
            };
            MyModel.Annotations.Add(finalXline);
        }
    }
}


