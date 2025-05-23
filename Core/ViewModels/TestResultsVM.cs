using krushtype.Core.Models;
using krushtype.Core.Utilities;
using krushtype.Core.Utilities.graphs;
using Newtonsoft.Json.Bson;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace krushtype.Core.ViewModels
{
    public class TestResultsVM : MainModelView
    {
        private readonly TestResultsModel _model = new TestResultsModel();
        private readonly navigationVM _navigationVM;
        private string _testtype;
        private string _language;
        private double _wpm;
        private double _rawwpm;
        private double _accuracy;
        private double _consistency;
        private bool _isnumbers;
        private bool _ispunctuation;
        private int _restartcount;
        private string _chars;
        private TimeSpan _time;
        private int _correctcount;
        private int _mistakescount;
        private List<PeriodData> _perioddata;
        private int _wordstyped;
        public string TestType
        {
            get { return _testtype; }
            set { _testtype = value; OnPropertyChanged(); }
        }

        public string Language
        {
            get { return _language; }
            set { _language = value; OnPropertyChanged(); }
        }

        public double WPM
        {
            get { return _wpm; }
            set { _wpm = value; OnPropertyChanged(); }
        }

        public double RawWPM
        {
            get { return _rawwpm; }
            set { _rawwpm = value; OnPropertyChanged(); }
        }

        public double Accuracy
        {
            get { return _accuracy; }
            set { _accuracy = value; OnPropertyChanged(); }
        }

        public double Consistency
        {
            get { return _consistency; }
            set { _consistency = value; OnPropertyChanged(); }
        }

        public bool IsNumbers
        {
            get { return _isnumbers; }
            set { _isnumbers = value; OnPropertyChanged(); }
        }
        public bool IsPunctuation
        {
            get { return _ispunctuation; }
            set { _ispunctuation = value; OnPropertyChanged(); }
        }
        public int RestartCount
        {
            get { return _restartcount; }
            set { _restartcount = value; OnPropertyChanged(); }
        }
        public string Chars
        {
            get { return _chars; }
            set { _chars = value; OnPropertyChanged(); }
        }

        public TimeSpan Time
        {
            get { return _time; }
            set { _time = value; OnPropertyChanged(); }
        }
        public double NumTime
        {
            get { return _time.TotalSeconds; }
        }
        public int CorrectCount
        {
            get { return _correctcount; }
            set { _correctcount = value; OnPropertyChanged(); }
        }
        public int MistakesCount
        {
            get { return _mistakescount; }
            set { _mistakescount = value; OnPropertyChanged(); }
        }
        public List<PeriodData> periodData
        {
            get { return _perioddata; }
            set { _perioddata = value; OnPropertyChanged(); }
        }
        public int WordsTyped
        {
            get { return _wordstyped; }
            set { _wordstyped = value; OnPropertyChanged(); }
        }        
        public Core.Utilities.graphs.ScatterGraphResults scatterGraph { get; set; } 
        public PlotModel scatterModel { get; set;}
        public PlotController customController { get; set; }

        public string ToolTipWPM { get; set; }
        public string ToolTipAcc { get; set; }
        public string ToolTipRawWPM { get; set; }
        public string ToolTipCharacters { get; set; }
        public string ToolTipTime { get; set; }
        
        public ICommand NextTestCommand { get; set; }
        
        public void NextTest(object obj)
        {
            _navigationVM.CurrentView = new typingVM(_navigationVM);
        }

        public TestResultsVM(navigationVM navigationvm, string testtype, string language,double wpm, double rawwpm, double accuracy, double consistency,bool isnumbers, bool ispunctuation, int restartcount, string chars, TimeSpan time, int correctcount, int mistakescount, List<PeriodData> PeriodData, int wordstyped)
        {
            _navigationVM = navigationvm;
            TestType = testtype;
            Language = language;
            WPM = wpm;
            RawWPM = rawwpm;
            Accuracy = accuracy;
            Consistency = consistency;
            IsNumbers = isnumbers;
            IsPunctuation = ispunctuation;
            RestartCount = restartcount;
            Chars = chars;
            Time = time;
            CorrectCount = correctcount;
            MistakesCount = mistakescount;
            scatterGraph = new ScatterGraphResults(PeriodData);
            WordsTyped = wordstyped;
            scatterModel = scatterGraph.MyModel;
            customController = scatterGraph.customController;
            NextTestCommand = new RelayCommand(NextTest);
            ToolTipWPM = $"{Math.Round(WPM,2)} wpm";
            ToolTipAcc = $"{Math.Round(Accuracy, 2)}% ({CorrectCount} correct / {MistakesCount} incorrect)";
            ToolTipRawWPM = $"{Math.Round(RawWPM, 2)} wpms";
            ToolTipCharacters = "correct, incorrect, extra, and missed";
            ToolTipTime = $"{Math.Round(Time.TotalSeconds, 2)}s";
            AddTest();
            SetupResultsDisplay();
        }
        private void AddTest()
        {
            _model.AddTest(TestType, WPM, RawWPM, Accuracy, Consistency, IsNumbers, IsPunctuation, Language, Chars, RestartCount, Time, WordsTyped);
        }

        private void SetupResultsDisplay() 
        {
        }
    }
}
