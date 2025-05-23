using krushtype.Core.Utilities;
using krushtype.Core.Utilities.files;
using krushtype.Core.Utilities.graphs;
using krushtype.UI.Views;
using Ookii.Dialogs.Wpf;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace krushtype.Core.Models
{
    public class AccountModel
    {
        public string nickname { get; set; } = JsonData.get_name();
        public DateTime CreateAccount { get; set; } = JsonData.get_join_date();
        public int tests_started { get; set; } = JsonData.get_tests_started();
        public int tests_completed { get; set; } = JsonData.get_tests_completed();
        public TimeSpan time_typing { get; set; } = TimeSpan.FromSeconds(Math.Round(JsonData.get_time_typing().TotalSeconds));
        public int words_typed { get; set; } = JsonData.get_words_typed();
        public CSVData.RecordTests tests_records = CSVData.get_best_test_results();
        public List<CSVData.TestResult> all_tests = CSVData.read_tests();
        public int max_round_wpm = CSVData.get_max_round_wpm_tests();
        public CSVData.TestResult max_test_wpm = CSVData.get_max_wpm_test();
        public double avg_wpm = CSVData.get_avg_wpm();
        public double avg_wpm_10_last_tests = CSVData.get_avg_wpm_10_last_tests();
        public double max_raw_wpm = CSVData.get_max_raw_wpm();
        public double avg_raw_wpm = CSVData.get_avg_raw_wpm();
        public double avg_raw_wpm_10_lasts_tests = CSVData.get_avg_rawwpm_10_last_tests();
        public double max_accuracy = CSVData.get_max_accuracy();
        public double avg_accuracy = CSVData.get_avg_accuracy();
        public double avg_accuracy_10_last_tests = CSVData.get_avg_acc_10_last_tests();
        public double max_consistency = CSVData.get_max_consistency();
        public double avg_consistency = CSVData.get_avg_consistency();
        public double avg_con_10_last_tests = CSVData.get_avg_con_10_last_tests();
        public ObservableCollection<CSVData.TestResult> last_10_tests = CSVData.get_10_last_tests();
        public Core.Utilities.graphs.ScatterGraph scattergraph
        {
            get { return new Core.Utilities.graphs.ScatterGraph(all_tests.Count, max_round_wpm, all_tests); }
        }
        public PlotModel scattermodel
        {
            get { return scattergraph.MyModel; }
        }
        public Core.Utilities.graphs.BarGraph bargraph
        {
            get { return new Core.Utilities.graphs.BarGraph(max_round_wpm, all_tests); }
        }
        public PlotModel barmodel
        {
            get { return bargraph.MyModel; }
        }
        public PlotController custom_controller
        {
            get { return scattergraph.customController; }
        }


        public void update_nick(string nickname)
        {
            JsonData.set_name(nickname);
            this.nickname = nickname;
        }
        public void ExportCSV()
        {
            var folderBrowserDialog = new VistaFolderBrowserDialog();
            folderBrowserDialog.Description = "Выберите папку";
            folderBrowserDialog.UseDescriptionForTitle = true;
            if (folderBrowserDialog.ShowDialog() == true)
            {
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                CSVData.export_csv(selectedFolderPath);
                MessageBox.Show("Файл успешно скачан!");
            }
        }
    }
}
