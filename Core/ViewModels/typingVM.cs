using krushtype.Core.Models;
using krushtype.Core.Utilities;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;
using System.Configuration; 

namespace krushtype.Core.ViewModels
{
    public class TypingParameters
    {
        public int Parameter1 { get; set; }
        public int Parameter2 { get; set; }
        public int Parameter3 { get; set; }
        public int Parameter4 { get; set; }
    }

    internal class typingVM : MainModelView
    {
        private readonly typingmodel _typingmodel;
        private readonly navigationVM _navigationVM;

        public event Action<bool> TestStateChanged;

        private bool _isteststarted;
        public bool IsTestStarted
        {
            get => _isteststarted;
            private set
            {
                if (_isteststarted != value)
                {
                    _isteststarted = value;
                    TestStateChanged?.Invoke(_isteststarted);
                    OnPropertyChanged(); 
                }
            }
        }
        public string DisplayText
        {
            get { return _typingmodel.text; }
            set { _typingmodel.text = value; OnPropertyChanged(); }
        }
        
        private TypingParameters _typingParameters;
        public TypingParameters TypingParameters
        {
            get
            {
                if (_typingParameters == null)
                {
                    _typingParameters = new TypingParameters
                    {
                        Parameter1 = GetDisplayTypingParameter(0),
                        Parameter2 = GetDisplayTypingParameter(1),
                        Parameter3 = GetDisplayTypingParameter(2),
                        Parameter4 = GetDisplayTypingParameter(3)
                    };
                }
                return _typingParameters;
            }
        }
        
        public int DisplayTypingParameter_1 => TypingParameters.Parameter1;
        public int DisplayTypingParameter_2 => TypingParameters.Parameter2;
        public int DisplayTypingParameter_3 => TypingParameters.Parameter3;
        public int DisplayTypingParameter_4 => TypingParameters.Parameter4;
        
        public bool IsParameter1Selected 
        { 
            get 
            { 
                if (_typingmodel.typing_type == "time")
                    return _typingmodel.time_typing == _typingmodel.time_typing_list[0];
                else
                    return _typingmodel.words_count == _typingmodel.words_count_list[0];
            } 
        }
        
        public bool IsParameter2Selected 
        { 
            get 
            { 
                if (_typingmodel.typing_type == "time")
                    return _typingmodel.time_typing == _typingmodel.time_typing_list[1];
                else
                    return _typingmodel.words_count == _typingmodel.words_count_list[1];
            } 
        }
        
        public bool IsParameter3Selected 
        { 
            get 
            { 
                if (_typingmodel.typing_type == "time")
                    return _typingmodel.time_typing == _typingmodel.time_typing_list[2];
                else
                    return _typingmodel.words_count == _typingmodel.words_count_list[2];
            } 
        }
        
        public bool IsParameter4Selected 
        { 
            get 
            { 
                if (_typingmodel.typing_type == "time")
                    return _typingmodel.time_typing == _typingmodel.time_typing_list[3];
                else
                    return _typingmodel.words_count == _typingmodel.words_count_list[3];
            } 
        }
        
        private bool _typingsettingsvisibility;
        public bool TypingSettingsVisibility
        {
            get { return _typingsettingsvisibility; }
            set { _typingsettingsvisibility = value; OnPropertyChanged(); }
        }
        public int FontSizeText
        {
            get { return _typingmodel.charSize; }
        }
        public int WidthText
        {
            get { return _typingmodel.textbLockWidth; }
        }
        public string ToolTipText
        {
            get { return _typingmodel.ToolTipText; }
            set { _typingmodel.ToolTipText = value; OnPropertyChanged(); }
        }
        public string UserInput
        {
            get => _typingmodel.userInput;
            set
            {
                string validatedInput = _typingmodel.ValidateUserInput(value);
                
                _typingmodel.userInput = validatedInput;
                OnPropertyChanged();
                OnUserInputChanged();
            }
        }
        public ObservableCollection<ColoredCharacter> ColoredUserInput
        {
            get => _typingmodel.coloredUserInput;
            set
            {
                _typingmodel.coloredUserInput = value;
                OnPropertyChanged();
            }
        }

        private List<string> _availableLanguages;
        
        public List<string> AvailableLanguages
        {
            get
            {
                if (_availableLanguages == null)
                {
                    _availableLanguages = LoadAvailableLanguages();
                }
                return _availableLanguages;
            }
        }
        
        public List<string> DisplayLanguageNames
        {
            get { return AvailableLanguages.Select(FormatDisplayLanguageName).ToList(); }
        }
        
        private string _languageSearchTerm = string.Empty;
        public string LanguageSearchTerm
        {
            get { return _languageSearchTerm; }
            set
            {
                if (_languageSearchTerm != value)
                {
                    _languageSearchTerm = value;
                    OnPropertyChanged(nameof(FilteredLanguageNames));
                }
            }
        }
        
        private ObservableCollection<string> _filteredLanguageNames;
        public ObservableCollection<string> FilteredLanguageNames
        {
            get
            {
                if (_filteredLanguageNames == null)
                {
                    _filteredLanguageNames = new ObservableCollection<string>(
                        AvailableLanguages.Select(FormatDisplayLanguageName));
                }
                return _filteredLanguageNames;
            }
        }
        
        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set 
            { 
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();
                    ChangeLanguage(_selectedLanguage);
                }
            }
        }
        
        private string _selectedDisplayLanguage;
        public string SelectedDisplayLanguage
        {
            get { return FormatDisplayLanguageName(_selectedLanguage); }
            set
            {
                if (value != null)
                {
                    string fileName = AvailableLanguages.FirstOrDefault(l => 
                        FormatDisplayLanguageName(l).Equals(value, StringComparison.OrdinalIgnoreCase));
                    
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        SelectedLanguage = fileName;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public bool AddPunctuationProperty
        {
            get { return _typingmodel.AddPunctuation; }
            set 
            { 
                if (_typingmodel.AddPunctuation != value)
                {
                    _typingmodel.AddPunctuation = value;
                    OnPropertyChanged();
                    Properties.Settings.Default.AddPunctuation = value;
                    Properties.Settings.Default.Save();
                    UpdateTextSettings(); 
                }
            }
        }

        public bool AddNumbersProperty
        {
            get { return _typingmodel.AddNumbers; }
            set 
            { 
                if (_typingmodel.AddNumbers != value)
                {
                    _typingmodel.AddNumbers = value;
                    OnPropertyChanged();
                    Properties.Settings.Default.AddNumbers = value;
                    Properties.Settings.Default.Save();
                    UpdateTextSettings(); 
                }
            }
        }

        private string FormatDisplayLanguageName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;
                
            string displayName = fileName.Replace('_', ' ');
            
            if (displayName.Contains(" 1k"))
                displayName = displayName.Replace(" 1k", " 1K");
            else if (displayName.Contains(" 10k"))
                displayName = displayName.Replace(" 10k", " 10K");
            else if (displayName.Contains(" 5k"))
                displayName = displayName.Replace(" 5k", " 5K");
            else if (displayName.Contains(" 25k"))
                displayName = displayName.Replace(" 25k", " 25K");
            else if (displayName.Contains(" 50k"))
                displayName = displayName.Replace(" 50k", " 50K");
            else if (displayName.Contains(" 375k"))
                displayName = displayName.Replace(" 375k", " 375K");
            else if (displayName.Contains(" 100k"))
                displayName = displayName.Replace(" 100k", " 100K");
            else if (displayName.Contains(" 450k"))
                displayName = displayName.Replace(" 450k", " 450K");
            else if (displayName.Contains(" 550k"))
                displayName = displayName.Replace(" 550k", " 550K");
            else if (displayName.Contains(" 600k"))
                displayName = displayName.Replace(" 600k", " 600K");
            else if (displayName.Contains(" 650k"))
                displayName = displayName.Replace(" 650k", " 650K");
                
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(displayName);
        }

        public string TypingTypeDisplay
        {
            get 
            { 
                return _typingmodel.typing_type == "time" ? "time (seconds)" : "word count";
            }
        }

        public string TypingType
        {
            get { return _typingmodel.typing_type; }
        }

        public ICommand AddPunctiationCommand { get; set; }
        public ICommand AddNumbersCommand { get; set; }
        public ICommand ChangeTypingFormCommand { get; set; }
        public ICommand ChangeTypingParameterCommand { get; set; }
        public ICommand ResetTextCommand { get; set; }
        public ICommand ChangeLanguageCommand { get; set; }
        public ICommand SetCustomParameterCommand { get; set; }
        public ICommand ShowCustomParameterDialogCommand { get; set; }
        public ICommand CancelCustomParameterCommand { get; set; }
        
        private int _customParameterValue;
        public int CustomParameterValue
        {
            get { return _customParameterValue; }
            set 
            { 
                _customParameterValue = value;
                OnPropertyChanged();
            }
        }
        
        private bool _customParameterDialogVisible;
        public bool CustomParameterDialogVisible
        {
            get { return _customParameterDialogVisible; }
            set 
            { 
                _customParameterDialogVisible = value;
                OnPropertyChanged();
            }
        }
        
        public void AddPunctuation(object obj)
        {
            AddPunctuationProperty = !AddPunctuationProperty;
        }
        public void AddNumbers(object obj)
        {
            AddNumbersProperty = !AddNumbersProperty;
        }
        public void ChangeTypingForm(object obj)
        {
            if ((string)obj != _typingmodel.typing_type )
            {
                if (_typingmodel.typing_type == "time")
                {
                    _typingmodel.typing_type = "words";
                    _typingmodel.words_count = _typingmodel.words_count_list[0];
                }
                else
                {
                    _typingmodel.typing_type = "time";
                    _typingmodel.words_count = _typingmodel.words_count_list[4];
                }
                
                Properties.Settings.Default.TypingType = _typingmodel.typing_type;
                Properties.Settings.Default.WordsCount = _typingmodel.words_count;
                Properties.Settings.Default.Save();
                
                OnPropertyChanged(nameof(TypingType)); 
                RefreshTypingParameters();
                OnPropertyChanged(nameof(TypingTypeDisplay));
                UpdateTextSettings();
            }
        }
        
        private void RefreshTypingParameters()
        {
            _typingParameters = null; 
            OnPropertyChanged(nameof(TypingParameters)); 
            OnPropertyChanged(nameof(DisplayTypingParameter_1));
            OnPropertyChanged(nameof(DisplayTypingParameter_2));
            OnPropertyChanged(nameof(DisplayTypingParameter_3));
            OnPropertyChanged(nameof(DisplayTypingParameter_4));
            OnPropertyChanged(nameof(IsParameter1Selected));
            OnPropertyChanged(nameof(IsParameter2Selected));
            OnPropertyChanged(nameof(IsParameter3Selected));
            OnPropertyChanged(nameof(IsParameter4Selected));
        }
        
        public void ChangeTypingParameter(object obj)
        {
            int parameter = (int)obj;
            
            if (_typingmodel.typing_type == "time")
            {
                _typingmodel.time_typing = parameter;
                Properties.Settings.Default.TimeTyping = parameter;
                Properties.Settings.Default.Save();
            }
            else
            {
                _typingmodel.words_count = parameter;
                Properties.Settings.Default.WordsCount = parameter;
                Properties.Settings.Default.Save();
            }
            
            RefreshTypingParameters();
            UpdateTextSettings();
        }
        public void ResetText(object obj)
        {
            IsTestStarted = false; 
            UpdateTextSettings();
            _typingmodel.UpdateRestartCount();
        }

        public typingVM(navigationVM navigationVM)
        {
            _typingmodel = new typingmodel();
            
            _navigationVM = navigationVM;
            
            TypingSettingsVisibility = true;
            
            IsTestStarted = false;
            
            LoadSettingsFromFile();
            
            CustomParameterDialogVisible = false;
            
            AddPunctiationCommand = new RelayCommand(AddPunctuation);
            AddNumbersCommand = new RelayCommand(AddNumbers);
            ChangeTypingFormCommand = new RelayCommand(ChangeTypingForm);
            ChangeTypingParameterCommand = new RelayCommand(ChangeTypingParameter);
            ResetTextCommand = new RelayCommand(ResetText);
            ChangeLanguageCommand = new RelayCommand(ChangeLanguage);
            SetCustomParameterCommand = new RelayCommand(SetCustomParameter);
            ShowCustomParameterDialogCommand = new RelayCommand(x => ShowCustomParameterDialog());
            CancelCustomParameterCommand = new RelayCommand(x => CancelCustomParameter());
        }

        private void LoadSettingsFromFile()
        {
            try
            {
                string savedLanguage = Properties.Settings.Default.SelectedLanguage;
                if (!string.IsNullOrEmpty(savedLanguage))
                {
                    if (AvailableLanguages.Contains(savedLanguage))
                    {
                        _selectedLanguage = savedLanguage;
                        _typingmodel.ChangeLanguage(savedLanguage);
                    }
                    else
                    {
                        _selectedLanguage = "english";
                        Properties.Settings.Default.SelectedLanguage = _selectedLanguage;
                        Properties.Settings.Default.Save();
                        _typingmodel.ChangeLanguage("english");
                    }
                    OnPropertyChanged(nameof(SelectedLanguage));
                    OnPropertyChanged(nameof(SelectedDisplayLanguage));
                }
                
                string typingType = Properties.Settings.Default.TypingType;
                if (typingType != "time" && typingType != "words")
                {
                    typingType = "time"; 
                    Properties.Settings.Default.TypingType = typingType;
                    Properties.Settings.Default.Save();
                }
                _typingmodel.typing_type = typingType;
                
                int timeTyping = Properties.Settings.Default.TimeTyping;
                if (timeTyping <= 0)
                {
                    timeTyping = 15; 
                    Properties.Settings.Default.TimeTyping = timeTyping;
                    Properties.Settings.Default.Save();
                }
                _typingmodel.time_typing = timeTyping;
                
                int wordsCount = Properties.Settings.Default.WordsCount;
                if (wordsCount <= 0)
                {
                    wordsCount = (_typingmodel.typing_type == "time") ? 150 : 10; 
                    Properties.Settings.Default.WordsCount = wordsCount;
                    Properties.Settings.Default.Save();
                }
                _typingmodel.words_count = wordsCount;
                
                _typingmodel.AddPunctuation = Properties.Settings.Default.AddPunctuation;
                
                _typingmodel.AddNumbers = Properties.Settings.Default.AddNumbers;
                
                RefreshTypingParameters();
                OnPropertyChanged(nameof(TypingTypeDisplay));
                OnPropertyChanged(nameof(TypingType));
                OnPropertyChanged(nameof(AddPunctuationProperty));
                OnPropertyChanged(nameof(AddNumbersProperty));
                
                UpdateTextSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                
                SetDefaultSettings();
            }
        }
        
        private void SetDefaultSettings()
        {
            _selectedLanguage = "english";
            _typingmodel.typing_type = "time";
            _typingmodel.time_typing = 15;
            _typingmodel.words_count = 150;
            _typingmodel.AddPunctuation = false;
            _typingmodel.AddNumbers = false;
            
            SaveSettingsToFile();
            
            RefreshTypingParameters();
            OnPropertyChanged(nameof(TypingTypeDisplay));
            OnPropertyChanged(nameof(AddPunctuationProperty));
            OnPropertyChanged(nameof(AddNumbersProperty));
            UpdateTextSettings();
        }
        
        private void SaveSettingsToFile()
        {
            try
            {
                Properties.Settings.Default.SelectedLanguage = _selectedLanguage ?? "english";
                Properties.Settings.Default.TypingType = _typingmodel.typing_type;
                Properties.Settings.Default.TimeTyping = _typingmodel.time_typing;
                Properties.Settings.Default.WordsCount = _typingmodel.words_count;
                Properties.Settings.Default.AddPunctuation = _typingmodel.AddPunctuation;
                Properties.Settings.Default.AddNumbers = _typingmodel.AddNumbers;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public int GetDisplayTypingParameter(int index)
        {
            return _typingmodel.typing_type == "time" ? _typingmodel.time_typing_list[index] : _typingmodel.words_count_list[index];
        }

        public void UpdateTextSettings()
        {
            if (!IsTestStarted)
            {
                _typingmodel.UpdateString();
                
                DisplayText = _typingmodel.ReformatText();
                
                UserInput = "";
                ColoredUserInput.Clear();
                
                ToolTipText = _typingmodel.GetToolTipText();
            }
        }

        private void OnUserInputChanged()
        {
            ColoredUserInput = _typingmodel.UpdateColoredUserInput();

            if (UserInput.Length > 0)
            {
                if (!IsTestStarted)
                {
                    IsTestStarted = true;
                    if (_typingmodel.GetTypingType() == "time")
                    {
                        ChangeTimeInToolTip(); 
                    }
                }
            }
            else 
            {
                if (IsTestStarted && _typingmodel.UserTotalInput.Count == 0)
                {
                    IsTestStarted = false;
                }
            }

            if (IsTestStarted) 
            {
                TypingSettingsVisibility = false;
            } 
            else 
            {
                TypingSettingsVisibility = true; 
            }

            switch (_typingmodel.GetTypingType())
            {
                case "words":
                    ToolTipText = _typingmodel.GetToolTipText();
                    if (_typingmodel.IsTestComplete())
                    {
                        ShowResults(); 
                    }
                    break;

                case "time":
                    if (!IsTestStarted)
                    {
                        ToolTipText = _typingmodel.GetToolTipText(); 
                    }
                    break;
            }
            if (_typingmodel.GetIsNewLine())
            {
                DisplayText = _typingmodel.GetText(); 
                ColoredUserInput = _typingmodel.GetColoredUserInput(); 
                _typingmodel.UpdateIsNewLine(); 
            }
        }

        private async void ChangeTimeInToolTip()
        {
            while (!_typingmodel.IsTestComplete())
            {
                ToolTipText = _typingmodel.GetToolTipText();
                await Task.Delay(1000);
            }
            IsTestStarted = false; 
            ShowResults();
            return;
        }
        public void ShowResults()
        {
            IsTestStarted = false; 

            string testtype = _typingmodel.GetTestType();
            string language = _typingmodel.GetLanguage();
            double wpm = _typingmodel.GetWPM();
            double accuracy = _typingmodel.GetAccuracy();
            double rawwpm = _typingmodel.GetRawWPM();
            double consistency = _typingmodel.GetConsistency();
            bool IsNumbers = _typingmodel.GetIsNumbers();
            bool IsPunctuation = _typingmodel.GetIsPunctuation();
            string chars = _typingmodel.GetChars();
            TimeSpan time = _typingmodel.GetTime();
            int correctcount = _typingmodel.GetCorrectCount();
            int mistakescount = _typingmodel.GetMistakesCount();
            int restartcount = _typingmodel.GetRestartCount();
            List<PeriodData> PeriodData = _typingmodel.GetPeriodData();
            int wordstyped = _typingmodel.GetWordsTyped();
            _navigationVM.CurrentView = new TestResultsVM(_navigationVM,testtype, language, wpm, rawwpm, accuracy, consistency,IsNumbers, IsPunctuation, restartcount, chars, time, correctcount, mistakescount, PeriodData, wordstyped);
        }

        public void ChangeLanguage(object obj)
        {
            string newLanguage = obj as string;
            if (!string.IsNullOrEmpty(newLanguage))
            {
                string[] codeLanguagePrefixes = new[] { "code_" };
                bool isCodeLanguage = codeLanguagePrefixes.Any(prefix => 
                    newLanguage.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
                    
                if (isCodeLanguage)
                {
                    try
                    {
                        var languageLoadTask = Task.Run(() => _typingmodel.ChangeLanguage(newLanguage));
                        bool completed = languageLoadTask.Wait(3000); 
                        
                        if (completed)
                        {
                            _selectedLanguage = newLanguage;
                            Properties.Settings.Default.SelectedLanguage = newLanguage;
                            Properties.Settings.Default.Save();
                            
                            OnPropertyChanged(nameof(SelectedLanguage));
                            OnPropertyChanged(nameof(SelectedDisplayLanguage));
                            UpdateTextSettings();
                        }
                        else
                        {
                            MessageBox.Show($"The language '{FormatDisplayLanguageName(newLanguage)}' took too long to load. Selecting English instead.", 
                                "Language Unavailable", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
                            
                            _selectedLanguage = "english";
                            Properties.Settings.Default.SelectedLanguage = "english";
                            Properties.Settings.Default.Save();
                            
                            OnPropertyChanged(nameof(SelectedLanguage));
                            OnPropertyChanged(nameof(SelectedDisplayLanguage));
                            _typingmodel.ChangeLanguage("english");
                            UpdateTextSettings();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show($"The language '{FormatDisplayLanguageName(newLanguage)}' could not be loaded. Selecting English instead.", 
                            "Language Unavailable", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                        
                        _selectedLanguage = "english";
                        Properties.Settings.Default.SelectedLanguage = "english";
                        Properties.Settings.Default.Save();
                        
                        OnPropertyChanged(nameof(SelectedLanguage));
                        OnPropertyChanged(nameof(SelectedDisplayLanguage));
                        _typingmodel.ChangeLanguage("english");
                        UpdateTextSettings();
                    }
                }
                else
                {
                    _selectedLanguage = newLanguage;
                    Properties.Settings.Default.SelectedLanguage = newLanguage;
                    Properties.Settings.Default.Save();
                    
                    OnPropertyChanged(nameof(SelectedLanguage));
                    OnPropertyChanged(nameof(SelectedDisplayLanguage));
                    _typingmodel.ChangeLanguage(newLanguage);
                    UpdateTextSettings();
                }
            }
        }

        public void SetCustomParameter(object obj)
        {
            if (_customParameterValue > 0)
            {
                if (_typingmodel.typing_type == "time")
                {
                    int timeValue = Math.Min(Math.Max(_customParameterValue, 5), 600);
                    _typingmodel.time_typing = timeValue;
                    Properties.Settings.Default.TimeTyping = timeValue;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    int wordCount = Math.Min(Math.Max(_customParameterValue, 5), 500);
                    _typingmodel.words_count = wordCount;
                    Properties.Settings.Default.WordsCount = wordCount;
                    Properties.Settings.Default.Save();
                }
                
                RefreshTypingParameters();
                UpdateTextSettings();
                CustomParameterDialogVisible = false;
            }
        }
        
        public void ShowCustomParameterDialog()
        {
            if (_typingmodel.typing_type == "time")
            {
                CustomParameterValue = 45; 
            }
            else
            {
                CustomParameterValue = 75; 
            }
            
            CustomParameterDialogVisible = true;
        }
        
        public void CancelCustomParameter()
        {
            CustomParameterDialogVisible = false;
        }
        
        public void UpdateTextWidth(double newWidth)
        {
            if (newWidth > 0 && Math.Abs(_typingmodel.textbLockWidth - newWidth) > 5)
            {
                _typingmodel.textbLockWidth = (int)newWidth;
                
                if (!IsTestStarted)
                {
                    string formattedText = _typingmodel.ReformatText();
                    if (!string.IsNullOrEmpty(formattedText))
                    {
                        DisplayText = formattedText;
                    }
                }
                
                OnPropertyChanged(nameof(WidthText));
            }
        }
        
        private List<string> LoadAvailableLanguages()
        {
            string baseAssetsPath = "Assets";
                
            string languagesPath = Path.Combine(baseAssetsPath, "languages");
            
            if (Directory.Exists(languagesPath))
            {
                return Directory.GetFiles(languagesPath, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .OrderBy(name => name)
                    .ToList();
            }
            
            return new List<string> { "english" }; 
        }
    
        public void FilterLanguages(string searchText)
        {
            _filteredLanguageNames.Clear();
            
            var filtered = string.IsNullOrWhiteSpace(searchText) 
                ? AvailableLanguages 
                : AvailableLanguages.Where(lang => 
                    FormatDisplayLanguageName(lang).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            
            foreach (var language in filtered.Select(FormatDisplayLanguageName))
            {
                _filteredLanguageNames.Add(language);
            }
        }

        public typingmodel GetTypingModel()
        {
            return _typingmodel;
        }
        
        public double Consistency
        {
            get
            {
                if (_typingmodel.CorrectCount == 0 && _typingmodel.MistakesCount > 0)
                {
                    return 100.0;
                }
                else
                {
                    if (_typingmodel.PeriodData == null || !_typingmodel.PeriodData.Any())
                    {
                        return 100.0; 
                    }

                    double avgWPM = _typingmodel.PeriodData.Average(pd => pd.WPM);

                    if (avgWPM == 0)
                    {
                        return 100.0; 
                    }

                    double dispersion = _typingmodel.PeriodData.Average(pd => Math.Pow(pd.WPM - avgWPM, 2));
                    double deviation = Math.Sqrt(dispersion);
                    
                    double consistencyValue = (1 - (deviation / avgWPM)) * 100;

                    if (double.IsNaN(consistencyValue))
                    {
                        return 100.0; 
                    }
                    
                    return Math.Min(100.0, Math.Max(0.0, consistencyValue));
                }
            }
        }
    }
}
