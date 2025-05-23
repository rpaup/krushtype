using krushtype.UI.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace krushtype.Core.Utilities.files
{
    public static class CSVData
    {
        private static readonly string baseFilePath = "csv_data";
        private static readonly string fileExtension = ".csv";
        
        private static string GetCurrentUserFilePath()
        {
            string userId = "anonymous";
            if (UserManager.IsUserLoggedIn())
            {
                userId = UserManager.CurrentUser.Id;
            }
            
            string fileName = $"{baseFilePath}_{userId}{fileExtension}";
            return FilePathManager.GetDataFilePath(fileName);
        }

        public static void create_csv()
        {
            string filePath = GetCurrentUserFilePath();
            if (!File.Exists(filePath))
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Number,Mode,WPM,RawWPM,Accuracy,Consistency,Date,IsNumbers,IsPunctuation,Language,Chars,RestartCount,Time,IsRecord, QuoteLength");
                }
            }
        }

        public static void export_csv(string UserPath)
        {
            string sourceFilePath = GetCurrentUserFilePath();
            string destinationFilePath = Path.Combine(UserPath, Path.GetFileName(sourceFilePath));
            try
            {
                string destDirectory = Path.GetDirectoryName(destinationFilePath);
                if (!Directory.Exists(destDirectory) && !string.IsNullOrEmpty(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }
                File.Copy(sourceFilePath, destinationFilePath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting CSV: {ex.Message}");
            }
        }

        public class TestResult : MainModelView
        {
            private string mode;
            private double wpm;
            private double rawWpm;
            private double accuracy;
            private double consistency;
            private DateTime date;
            private bool isNumbers;
            private bool isPunctuation;
            private string language;
            private string chars;
            private int restartCount;
            private TimeSpan time;
            private bool isrecord;

            public string Mode
            {
                get => mode;
                set
                {
                    if (mode != value)
                    {
                        mode = value;
                        OnPropertyChanged(nameof(Mode));
                    }
                }
            }

            public double WPM
            {
                get => wpm;
                set
                {
                    if (wpm != value)
                    {
                        wpm = value;
                        OnPropertyChanged(nameof(WPM));
                    }
                }
            }

            public double RawWPM
            {
                get => rawWpm;
                set
                {
                    if (rawWpm != value)
                    {
                        rawWpm = value;
                        OnPropertyChanged(nameof(RawWPM));
                    }
                }
            }

            public double Accuracy
            {
                get => accuracy;
                set
                {
                    if (accuracy != value)
                    {
                        accuracy = value;
                        OnPropertyChanged(nameof(Accuracy));
                    }
                }
            }

            public double Consistency
            {
                get => consistency;
                set
                {
                    if (consistency != value)
                    {
                        consistency = value;
                        OnPropertyChanged(nameof(Consistency));
                    }
                }
            }

            public DateTime Date
            {
                get => date;
                set
                {
                    if (date != value)
                    {
                        date = value;
                        OnPropertyChanged(nameof(Date));
                    }
                }
            }

            public bool IsNumbers
            {
                get => isNumbers;
                set
                {
                    if (isNumbers != value)
                    {
                        isNumbers = value;
                        OnPropertyChanged(nameof(IsNumbers));
                    }
                }
            }

            public bool IsPunctuation
            {
                get => isPunctuation;
                set
                {
                    if (isPunctuation != value)
                    {
                        isPunctuation = value;
                        OnPropertyChanged(nameof(IsPunctuation));
                    }
                }
            }

            public string Language
            {
                get => language;
                set
                {
                    if (language != value)
                    {
                        language = value;
                        OnPropertyChanged(nameof(Language));
                    }
                }
            }

            public string Chars
            {
                get => chars;
                set
                {
                    if (chars != value)
                    {
                        chars = value;
                        OnPropertyChanged(nameof(Chars));
                    }
                }
            }

            public int RestartCount
            {
                get => restartCount;
                set
                {
                    if (restartCount != value)
                    {
                        restartCount = value;
                        OnPropertyChanged(nameof(RestartCount));
                    }
                }
            }

            public TimeSpan Time
            {
                get => time;
                set
                {
                    if (time != value)
                    {
                        time = value;
                        OnPropertyChanged(nameof(Time));
                    }
                }
            }
            public bool IsRecord
            {
                get => isrecord;
                set
                {
                    if (isrecord != value)
                    {
                        isrecord = value;
                        OnPropertyChanged(nameof(IsRecord));
                    }
                }
            }
        }



        public static async Task add_test(string mode, double wpm, double rawwpm, double accuracy, double consistency, bool isnumbers, bool ispunctuation, string language, string chars, int restartcount, TimeSpan time)
        {
            string WPM = Math.Round(wpm, 2).ToString().Replace(',', '.');
            string RawWPM = Math.Round(rawwpm, 2).ToString().Replace(',', '.');
            string Accuracy = Math.Round(accuracy, 2).ToString().Replace(',', '.');
            string Consistency = Math.Round(consistency, 2).ToString().Replace(',', '.');
            bool IsRecord = CheckIsRecord(mode, wpm);
            int TestNum = read_tests().Count + 1;
            string filePath = GetCurrentUserFilePath();
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                DateTime Date = DateTime.Now - time;
                string formattedDate = Date.ToString("yyyy-MM-dd HH:mm:ss");
                string formattedTime = time.ToString(@"hh\:mm\:ss");
                
                string escapedLanguage = language.Replace(",", ";");
                
                string line = $"{TestNum},{mode},{WPM},{RawWPM},{Accuracy},{Consistency},{formattedDate},{isnumbers},{ispunctuation},{escapedLanguage},{chars},{restartcount},{formattedTime},{IsRecord}";
                writer.WriteLine(line);
            }
        }
        public static List<TestResult> read_tests()
        {
            List<TestResult> tests = new List<TestResult>();
            try
            {
                string filePath = GetCurrentUserFilePath();
                
                if (!File.Exists(filePath))
                {
                    create_csv();
                    tests.Add(new TestResult());
                    return tests;
                }
                
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                    {
                        try
                {
                    string[] CurrentLine = line.Split(',');
                            
                            if (CurrentLine.Length < 14)
                                continue;
                                
                    TestResult test = new TestResult
                    {
                        Mode = CurrentLine[1],
                        WPM = Convert.ToDouble(CurrentLine[2], CultureInfo.InvariantCulture),
                        RawWPM = Convert.ToDouble(CurrentLine[3], CultureInfo.InvariantCulture),
                        Accuracy = Convert.ToDouble(CurrentLine[4], CultureInfo.InvariantCulture),
                        Consistency = Convert.ToDouble(CurrentLine[5], CultureInfo.InvariantCulture),
                                Date = DateTime.TryParse(CurrentLine[6], out DateTime date) ? date : DateTime.Now,
                                IsNumbers = bool.TryParse(CurrentLine[7], out bool isNumbers) ? isNumbers : false,
                                IsPunctuation = bool.TryParse(CurrentLine[8], out bool isPunctuation) ? isPunctuation : false,
                                Language = CurrentLine[9].Replace(";", ","), // Restore any escaped commas
                                Chars = CurrentLine[10],
                                RestartCount = int.TryParse(CurrentLine[11], out int restartCount) ? restartCount : 0,
                                Time = TimeSpan.TryParse(CurrentLine[12], out TimeSpan timeSpan) ? timeSpan : TimeSpan.Zero,
                                IsRecord = bool.TryParse(CurrentLine[13], out bool isRecord) ? isRecord : false
                    };
                    tests.Add(test);
                }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                tests.Clear();
                tests.Add(new TestResult());
            }
            
            if (tests.Count > 0)
            {
                return tests;
            }
            else
            {
                tests.Add(new TestResult());
                return tests;
            }
        }
        public class RecordTests : MainModelView
        {
            private TestResult _time15;
            private TestResult _time30;
            private TestResult _time60;
            private TestResult _time120;
            private TestResult _words10;
            private TestResult _words25;
            private TestResult _words50;
            private TestResult _words100;

            public TestResult Time15
            {
                get => _time15;
                set
                {
                    if (_time15 != value)
                    {
                        _time15 = value;
                        OnPropertyChanged(nameof(Time15));
                    }
                }
            }

            public TestResult Time30
            {
                get => _time30;
                set
                {
                    if (_time30 != value)
                    {
                        _time30 = value;
                        OnPropertyChanged(nameof(Time30));
                    }
                }
            }

            public TestResult Time60
            {
                get => _time60;
                set
                {
                    if (_time60 != value)
                    {
                        _time60 = value;
                        OnPropertyChanged(nameof(Time60));
                    }
                }
            }

            public TestResult Time120
            {
                get => _time120;
                set
                {
                    if (_time120 != value)
                    {
                        _time120 = value;
                        OnPropertyChanged(nameof(Time120));
                    }
                }
            }

            public TestResult Words10
            {
                get => _words10;
                set
                {
                    if (_words10 != value)
                    {
                        _words10 = value;
                        OnPropertyChanged(nameof(Words10));
                    }
                }
            }

            public TestResult Words25
            {
                get => _words25;
                set
                {
                    if (_words25 != value)
                    {
                        _words25 = value;
                        OnPropertyChanged(nameof(Words25));
                    }
                }
            }

            public TestResult Words50
            {
                get => _words50;
                set
                {
                    if (_words50 != value)
                    {
                        _words50 = value;
                        OnPropertyChanged(nameof(Words50));
                    }
                }
            }

            public TestResult Words100
            {
                get => _words100;
                set
                {
                    if (_words100 != value)
                    {
                        _words100 = value;
                        OnPropertyChanged(nameof(Words100));
                    }
                }
            }
        }
        public static RecordTests get_best_test_results()
        {
            RecordTests Records = new RecordTests();
            try
            {
            List<TestResult> tests = read_tests();
            foreach (var test in tests)
            {
                    try
                    {
                        if (string.IsNullOrEmpty(test.Mode))
                            continue;
                            
                switch (test.Mode)
                {
                    case "time 15":
                    {
                                if (Records.Time15 == null || test.WPM > Records.Time15.WPM)
                        {
                            Records.Time15 = test;
                        }
                        break;
                    }
                    case "time 30":
                    {
                                if (Records.Time30 == null || test.WPM > Records.Time30.WPM)
                        {
                            Records.Time30 = test;
                        }
                        break;
                    }
                    case "time 60":
                    {
                                if (Records.Time60 == null || test.WPM > Records.Time60.WPM)
                        {
                            Records.Time60 = test;
                        }
                        break;
                    }
                    case "time 120":
                    {
                                if (Records.Time120 == null || test.WPM > Records.Time120.WPM)
                        {
                            Records.Time120 = test;
                        }
                        break;
                    }
                    case "words 10":
                    {
                                if (Records.Words10 == null || test.WPM > Records.Words10.WPM)
                        {
                            Records.Words10 = test;
                        }
                        break;
                    }
                    case "words 25":
                    {
                                if (Records.Words25 == null || test.WPM > Records.Words25.WPM)
                        {
                            Records.Words25 = test;
                        }
                        break;
                    }
                    case "words 50":
                    {
                                if (Records.Words50 == null || test.WPM > Records.Words50.WPM)
                        {
                            Records.Words50 = test;
                        }
                        break;
                    }
                    case "words 100":
                    {
                                if (Records.Words100 == null || test.WPM > Records.Words100.WPM)
                        {
                            Records.Words100 = test;
                        }
                        break;
                    }
                }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Records;
        }
        public static bool CheckIsRecord(string Mode, double WPM)
        {
            List<TestResult> tests = read_tests();
            int HigherWPMTestsCount = tests.Where(ch => ch.Mode == Mode && ch.WPM > WPM).Count();
            return HigherWPMTestsCount == 0;
        }

        public static int get_max_round_wpm_tests()
        {
            List<TestResult> tests = read_tests();
            return Convert.ToInt32(Math.Ceiling(tests.Max(ch => ch.WPM) / 10.0) * 10.0);
        }
        public static TestResult get_max_wpm_test()
        {
            List<TestResult> tests = read_tests();
            double MaxWPM = tests.Max(ch => ch.WPM);
            return tests.Where(ch => ch.WPM == MaxWPM).Last();
        }
        public static double get_avg_wpm()
        {
            List<TestResult> tests = read_tests();
            return tests.Average(ch => ch.WPM);
        }
        public static double get_avg_wpm_10_last_tests()
        {
            List<TestResult> tests = read_tests();
            tests = tests.Skip(Math.Max(0, tests.Count - 10)).ToList();
            return tests.Average(ch => ch.WPM);
        }
        public static double get_max_raw_wpm()
        {
            List<TestResult> tests = read_tests();
            return tests.Max(ch => ch.RawWPM);
        }
        public static double get_avg_raw_wpm()
        {
            List<TestResult> tests = read_tests();
            return tests.Average(ch => ch.RawWPM);
        }
        public static double get_avg_rawwpm_10_last_tests()
        {
            List<TestResult> tests = read_tests();
            tests = tests.Skip(Math.Max(0, tests.Count - 10)).ToList();
            return tests.Average(ch => ch.RawWPM);
        }
        public static double get_max_accuracy()
        {
            List<TestResult> tests = read_tests();
            return tests.Max(ch => ch.Accuracy);
        }
        public static double get_avg_accuracy()
        {
            List<TestResult> tests = read_tests();
            return tests.Average(ch => ch.Accuracy);
        }
        public static double get_avg_acc_10_last_tests()
        {
            List<TestResult> tests = read_tests();
            tests = tests.Skip(Math.Max(0, tests.Count - 10)).ToList();
            return tests.Average(ch => ch.Accuracy);
        }
        public static double get_max_consistency()
        {
            List<TestResult> tests = read_tests();
            return tests.Max(ch => ch.Consistency);
        }
        public static double get_avg_consistency()
        {
            List<TestResult> tests = read_tests();
            return tests.Average(ch => ch.Consistency);
        }
        public static double get_avg_con_10_last_tests()
        {
            List<TestResult> tests = read_tests();
            tests = tests.Skip(Math.Max(0, tests.Count - 10)).ToList();
            return tests.Average(ch => ch.Consistency);
        }

        public static ObservableCollection<TestResult> get_10_last_tests()
        {
            ObservableCollection<TestResult> Last10Tests = new ObservableCollection<TestResult>();
            List<TestResult> tests = read_tests();
            if (String.IsNullOrEmpty(tests.First().Mode)) return new ObservableCollection<TestResult>();
            foreach (var i in tests.Skip(Math.Max(0, tests.Count - 10)))
            {
                Last10Tests.Add(i);
            }
            return Last10Tests;
        }
    }

}
