using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using krushtype.Core.Models;
using System.Windows.Input;
using krushtype.Core.Utilities;
using krushtype.UI.UserControls;
using System.Runtime.CompilerServices;
using System.IO;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.ComponentModel;
using OxyPlot.Annotations;
using krushtype.Core.Utilities.files;
using System.Windows;
using System.Windows.Threading;
using krushtype.UI.Views;
using krushtype.Core.ViewModels;

namespace krushtype.Core.ViewModels
{    
    public class accountVM : MainModelView
    {
        private readonly AccountModel _accountModel;
        
        private readonly INavigationService _navigationService;
        
        public ICommand LogoutCommand { get; private set; }
        
        public ICommand ChangePasswordCommand { get; private set; }
        
        public string DisplayNickName
        {
            get { return UserManager.IsUserLoggedIn() ? UserManager.CurrentUser.Username : _accountModel?.nickname; }
            set 
            { 
                if (UserManager.IsUserLoggedIn())
                {
                    UserManager.UpdateUserProfile(value);
                }
                _accountModel.nickname = value; 
                OnPropertyChanged(); 
            }
        }
        
        public string DisplayCreateAccount
        {
            get { return (UserManager.IsUserLoggedIn() ? UserManager.CurrentUser.JoinDate : _accountModel.CreateAccount).ToString("d MMMM yyyy", new CultureInfo("en-EN")); }
            set { _accountModel.CreateAccount = DateTime.ParseExact(value, "d MMMM yyyy", new CultureInfo("en-EN")); OnPropertyChanged(); }
        }

        public int DisplayTestsStarted
        {
            get { return _accountModel.tests_started; }
        }
        public int DisplayTestsCompleted
        {
            get { return _accountModel.tests_completed; }
        }
        public double DisplayTestCompletedPerStarted
        {
            get { return  Math.Round((_accountModel.tests_completed / (double)_accountModel.tests_started) * 100,0); }
        }
        public double DisplayRestartsPerCompletedTests
        {
            get { return Math.Round((DisplayTestsStarted - DisplayTestsCompleted) / (double)DisplayTestsCompleted, 1); }
        }
        public CSVData.TestResult DisplayMaxTestWPM
        {
            get { return _accountModel.max_test_wpm; }
            set { _accountModel.max_test_wpm = value; OnPropertyChanged(); }
        }
        public double DisplayAverageWPM
        {
            get { return _accountModel.avg_wpm; }
            set { _accountModel.avg_wpm = value; OnPropertyChanged(); }
        }
        public double DisplayAvgWPMLast10Tests
        {
            get { return _accountModel.avg_wpm_10_last_tests; }
            set { _accountModel.avg_wpm_10_last_tests = value; OnPropertyChanged(); }
        }
        public double DisplayHighestRawWPM
        {
            get { return _accountModel.max_raw_wpm; }
            set { _accountModel.max_raw_wpm = value; OnPropertyChanged(); }
        }
        public double DisplayAverageRawWPM
        {
            get { return _accountModel.avg_raw_wpm; }
            set { _accountModel.avg_raw_wpm = value; OnPropertyChanged(); }
        }
        public double DisplayAvgRawWPM10Tests
        {
            get { return _accountModel.avg_raw_wpm_10_lasts_tests; }
            set { _accountModel.avg_raw_wpm_10_lasts_tests = value; OnPropertyChanged(); }
        }
        public double DisplayHighestAccuracy
        {
            get { return _accountModel.max_accuracy; }
            set { _accountModel.max_accuracy = value; OnPropertyChanged(); }
        }
        public double DisplayAverageAccuracy
        {
            get { return _accountModel.avg_accuracy; }
            set { _accountModel.avg_accuracy = value; OnPropertyChanged(); }
        }
        public double DisplayAvgAcc10lastTests
        {
            get { return _accountModel.avg_accuracy_10_last_tests; }
            set { _accountModel.avg_accuracy_10_last_tests = value; OnPropertyChanged(); }
        }
        public double DisplayHighestConsistency
        {
            get { return _accountModel.max_consistency; }
            set { _accountModel.max_consistency = value; OnPropertyChanged(); }
        }
        public double DisplayAverageConsistency
        {
            get { return _accountModel.avg_consistency; }
            set { _accountModel.avg_consistency = value; OnPropertyChanged(); }
        }
        public double DisplayAvgCon10lastTests
        {
            get { return _accountModel.avg_con_10_last_tests; }
            set { _accountModel.avg_con_10_last_tests = value; OnPropertyChanged(); }
        }
        public string DisplayTestsCompletedToolTip
        {
            get
            {            
                if (DisplayTestsCompleted > 0)
                {
                    return $"{(DisplayTestsCompleted / (double)DisplayTestsStarted) * 100:0}% ({Math.Round((DisplayTestsStarted - DisplayTestsCompleted) / (double)DisplayTestsCompleted, 1)} restarts per completed test)";
                }
                else
                {
                    return "Вы еще не завершили ни одного теста :(";
                }
            }
        }
        public TimeSpan DisplayTimeTyping
        {
            get { return _accountModel.time_typing; }
        }
        public int DisplayWordsTyped
        {
            get { return _accountModel.words_typed; }
        }
        public CSVData.RecordTests RecordTests
        {
            get { return _accountModel.tests_records; }
        }
        public List<CSVData.TestResult> All_Tests
        {
            get { return _accountModel.all_tests; }
            set {  _accountModel.all_tests = value; OnPropertyChanged(); }

        }
        public int Max_Round_WPM
        {
            get { return _accountModel.max_round_wpm; }
            set { _accountModel.max_round_wpm = value; OnPropertyChanged(); }
        }        
        public Core.Utilities.graphs.ScatterGraph Scattergraph
        {
            get { return _accountModel.scattergraph; }
        }
        public PlotModel ScatterModel
        {
            get { return _accountModel.scattermodel; }
        }
        public Core.Utilities.graphs.BarGraph Bargraph
        {
            get { return _accountModel.bargraph; }
        }
        public PlotModel BarModel
        {
            get { return _accountModel.barmodel; }
        }

        public PlotController customController
        {
            get { return _accountModel.custom_controller; }
        }
        public ObservableCollection<CSVData.TestResult> Last10Tests
        {
            get { return _accountModel.last_10_tests; }
            set { _accountModel.last_10_tests = value; OnPropertyChanged(); }
        }
        public ICommand ChangeNickNameCommand { get; set; }
        public ICommand ExportCSVCOmmand {  get; set; }
        private void ChangeNickName(object obj)
        {
            _accountModel.update_nick(obj.ToString());
        }
        private void ExportCSV(object obj)
        {
            _accountModel.ExportCSV();
        }
        public accountVM(INavigationService navigationService = null)
        {
            _accountModel = new AccountModel();
            
            _navigationService = navigationService;
            
            ChangeNickNameCommand = new RelayCommand(ChangeNickName);
            ExportCSVCOmmand = new RelayCommand(ExportCSV);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword);

            if (UserManager.IsUserLoggedIn())
            {
                OnPropertyChanged(nameof(DisplayNickName));
                OnPropertyChanged(nameof(DisplayCreateAccount));
            }
        }
        
        private void ExecuteLogout(object parameter)
        {
            
            var navService = _navigationService as NavigationService;
            if (navService != null && navService._navigationViewModel != null)
            {
                navService._navigationViewModel.LogoutCommand.Execute(null);
            }
            else
            {
                UserManager.LogoutUser(); 
                
                if (_navigationService != null)
                {
                    _navigationService.NavigateToTyping();
                }
            }
            
        }
        
        private void ExecuteChangePassword(object parameter)
        {
        }
    }
}
