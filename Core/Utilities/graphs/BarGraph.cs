using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using krushtype.Core.Utilities.files;

namespace krushtype.Core.Utilities.graphs
{
    public class BarGraph
    {
        public PlotModel MyModel { get; }
        private LinearBarSeries barSeries;
        private CategoryAxis categoryAxis;
        private LinearAxis LinearAxis;
        private int Max_WPM;
        public BarGraph(int Max_WPM, List<CSVData.TestResult> All_Tests)
        {
            this.Max_WPM = Max_WPM / 10;
            MyModel = new PlotModel
            {
                Background = OxyColors.Transparent,
                PlotAreaBorderThickness = new OxyThickness(0.4),
                PlotAreaBorderColor = OxyColor.Parse("#2c2e31")
            };
            barSeries = new LinearBarSeries
            {
                FillColor = OxyColor.Parse("#84a0c6"),
                BarWidth = 80 / ((this.Max_WPM / 10) % 10 + 1),
                TrackerFormatString = "\n¦Tests: {4}"
            };
            MyModel.Series.Add(barSeries);

            categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineThickness = 0.4,
                TicklineColor = OxyColor.Parse("#2c2e31"),
                MajorGridlineColor = OxyColor.Parse("#2c2e31"),
                IsPanEnabled = false,
                IsZoomEnabled = false,
                TextColor = OxyColor.Parse("#646669"),
                TitleColor = OxyColor.Parse("#646669")

            };
            LinearAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                MajorStep = double.NaN,
                Title = "tests",
                TickStyle = TickStyle.None,
                TextColor = OxyColor.Parse("#646669"),
                TitleColor = OxyColor.Parse("#646669"),
                TitleFontSize = 14,
                IsPanEnabled = false,
                IsZoomEnabled = false, 
            };

            AddValue(All_Tests);
            AddLabels(Max_WPM);

            MyModel.Axes.Add(categoryAxis);
            MyModel.Axes.Add(LinearAxis);
        }
        public void AddValue(List<CSVData.TestResult> All_Tests)
        {
            if (String.IsNullOrEmpty(All_Tests.First().Mode)) return;
            int step = 0;
            int TestInd = 0;
            int ColumnInd = 0;
            int ColumnY = 0;
            All_Tests = All_Tests.OrderBy(ch => ch.WPM).ToList();
            while (TestInd < All_Tests.Count) 
            {
                if (All_Tests[TestInd].WPM >= step && All_Tests[TestInd].WPM <= step+9)
                {
                    ColumnY++;
                    TestInd++;
                }
                else
                {
                    barSeries.Points.Add(new DataPoint(ColumnInd, ColumnY));
                    ColumnInd++;
                    ColumnY = 0;
                    step += 10;
                }
            }
            if (ColumnY > 0)
            {
                barSeries.Points.Add(new DataPoint(ColumnInd, ColumnY));
            }
        }
        public void AddLabels(int Max_WPM)
        {
            int step = 0;
            while (step  < Max_WPM)
            {
                categoryAxis.Labels.Add($"{step}-{step+9}");
                step += 10;
            }
        }
    }
}

