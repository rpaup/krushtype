using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Threading.Tasks;
using Newtonsoft.Json;
using krushtype.Core.Utilities.enums;

namespace krushtype.Core.Models
{
    public class PeriodData
    {
        public TimeSpan Time;
        public double WPM;
        public double RawWPM;
        public int Mistakes;
    }


    public class ColoredCharacter
    {
        public string Character { get; set; }
        public State State { get; set; }
        public int FontSizeText { get; set; }
        public bool IsFunctionalSpace { get; set; }
    }
    internal class typingmodel
    {
        private readonly string words_path;
        private readonly string marks_path;
        private readonly string numbers_path;

        private readonly string languages_path;

        private const double StandardCharsPerWord = 5.0;

        public bool AddPunctuation { get; set; } = false;
        public bool AddNumbers { get; set; } = false;
        public string text { get; set; }
        public int[] words_count_list { get; } = new int[] { 10, 25, 50, 100, 150 };
        public int[] time_typing_list { get; } = new int[] { 15, 30, 60, 120 };
        public string language = "english";
        public List<string> AvailableLanguages { get; private set; }
        public string typing_type { get; set; } = "time";
        public int words_count { get; set; } = 150;
        public int time_typing { get; set; } = 15;
        private static string[] words;
        private static string[] mark;
        private static string[] numbers;
        public string userInput;
        public ObservableCollection<ColoredCharacter> UserTotalInput = new ObservableCollection<ColoredCharacter>();
        public ObservableCollection<ColoredCharacter> coloredUserInput = new ObservableCollection<ColoredCharacter>();
        public int charSize = 32;
        public int textbLockWidth = 850;
        public Stopwatch stopwatch = new Stopwatch();
        public bool IsNewLine = false;
        public int MistakesCount = 0;
        public int CorrectCount = 0;
        public int MissedCount = 0;
        public List<PeriodData> PeriodData = new List<PeriodData>();
        public int PeriodMissedCount = 0;
        public int RestartCount = 0;
        public string ToolTipText;

        private double? cachedWPM = null;
        private double? cachedRawWPM = null;
        private bool testCompleted = false;

        public int desiredLineCount = 5;
        private bool needMoreLines = false;

        public int LastWordDelimiterPosition = -1;
        public bool HasWordDelimiters = false;

        public typingmodel()
        {
            string baseAssetsPath = "Assets";

            words_path = Path.Combine(baseAssetsPath, "textdata", "words.txt");
            marks_path = Path.Combine(baseAssetsPath, "textdata", "marks.txt");
            numbers_path = Path.Combine(baseAssetsPath, "textdata", "numbers.txt");
            languages_path = Path.Combine(baseAssetsPath, "languages");

            ToolTipText = $"{time_typing}";
            
            LoadAvailableLanguages();
            
            words = get_words();
            mark = get_marks();
            numbers = get_numbers();

            try
            {
                UserSettings.Initialize();
                
                language = UserSettings.GetLanguage();
                typing_type = UserSettings.GetTypingType();
                time_typing = UserSettings.GetTimeTyping();
                words_count = UserSettings.GetWordsCount();
                AddPunctuation = UserSettings.GetAddPunctuation();
                AddNumbers = UserSettings.GetAddNumbers();
                
                ToolTipText = typing_type == "time" ? $"{time_typing}" : $"0/{words_count}";
                
                if (language != "english")
                {
                    ChangeLanguage(language);
                }
            }
            catch (Exception ex)
            {
                language = "english";
                typing_type = "time";
                time_typing = 15;
                words_count = 150;
                AddPunctuation = false;
                AddNumbers = false;
                
                try {
                    UserSettings.SaveAllSettings(language, typing_type, time_typing, words_count, AddPunctuation, AddNumbers);
                } catch {}
            }
            
            text = AddLineBreaks(create_text(), GetMaxCharsPerLine());
            
            EnsureEnoughLines();
        }
        
        private void LoadAvailableLanguages()
        {
            AvailableLanguages = new List<string>();
            
            try
            {
                if (Directory.Exists(languages_path))
                {
                    string[] languageFiles = Directory.GetFiles(languages_path, "*.json");
                    
                    Dictionary<string, List<string>> languageGroups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                    
                    foreach (string file in languageFiles)
                    {
                        string langName = Path.GetFileNameWithoutExtension(file);
                        
                        if (langName.StartsWith("."))
                            continue;
                            
                        string baseName = langName.Contains("_") ? langName.Split('_')[0] : langName;
                        
                        if (!languageGroups.ContainsKey(baseName))
                        {
                            languageGroups[baseName] = new List<string>();
                        }
                        
                        languageGroups[baseName].Add(langName);
                    }
                    
                    foreach (var groupPair in languageGroups.OrderBy(g => g.Key))
                    {
                        string baseName = groupPair.Key;
                        List<string> variants = groupPair.Value;
                        
                        variants.Sort((a, b) => 
                        {
                            bool aIsBase = a.Equals(baseName, StringComparison.OrdinalIgnoreCase);
                            bool bIsBase = b.Equals(baseName, StringComparison.OrdinalIgnoreCase);
                            
                            if (aIsBase && !bIsBase) return -1;
                            if (!aIsBase && bIsBase) return 1;
                            
                            int aSize = ExtractSizeIndicator(a);
                            int bSize = ExtractSizeIndicator(b);
                            
                            return aSize.CompareTo(bSize);
                        });
                        
                        AvailableLanguages.AddRange(variants);
                    }
                }
            }
            catch (Exception ex)
            {
                AvailableLanguages.Add("english");
            }
            
            if (AvailableLanguages.Count == 0)
            {
                AvailableLanguages.Add("english");
            }
        }
        
        private int ExtractSizeIndicator(string langName)
        {
            if (!langName.Contains("_"))
                return 0;
                
            string suffix = langName.Split('_').Last();
            
            if (suffix.EndsWith("k", StringComparison.OrdinalIgnoreCase))
            {
                string numPart = suffix.Substring(0, suffix.Length - 1);
                if (int.TryParse(numPart, out int size))
                {
                    return size;
                }
            }
            
            return int.MaxValue;
        }
        
        public void ChangeLanguage(string newLanguage)
        {
            try
            {
                string languageFilePath = Path.Combine(languages_path, newLanguage + ".json");
                
                if (File.Exists(languageFilePath))
                {
                    language = newLanguage;
                    
                    UserSettings.SaveLanguage(language);
                    
                    string jsonContent = File.ReadAllText(languageFilePath);
                    
                    var langData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
                    
                    if (language.StartsWith("code_"))
                    {
                        ConfigureCodeLanguage(langData);
                    }
                    else
                    {
                        if (langData.TryGetValue("words", out var wordsObj) && wordsObj is Newtonsoft.Json.Linq.JArray wordsArray)
                        {
                            words = wordsArray.ToObject<string[]>();
                        }
                    }
                }
                else
                {
                    language = "english";
                    words = get_words();
                    
                    UserSettings.SaveLanguage("english");
                }
                
                text = AddLineBreaks(create_text(), GetMaxCharsPerLine());
            }
            catch
            {
                language = "english";
                words = get_words();
                
                UserSettings.SaveLanguage("english");
                
                text = AddLineBreaks(create_text(), GetMaxCharsPerLine());
            }
        }

        public string UpdateString()
        {
            userInput = "";
            UserTotalInput.Clear();
            coloredUserInput.Clear();
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
                stopwatch.Reset();
            }
            
            CorrectCount = 0;
            MistakesCount = 0;
            MissedCount = 0;
            PeriodData.Clear();
            
            ResetTestState();
            
            ResetEditingRestrictions();
            
            ToolTipText = UpdateToolTipText();
            
            if (typing_type == "words")
            {
                string rawText = create_text(words_count);
                
                rawText = rawText.Replace("\n", " ").Trim();
                while (rawText.Contains("  "))
                {
                    rawText = rawText.Replace("  ", " ");
                }
                
                text = AddLineBreaks(rawText, GetMaxCharsPerLine());
            }
            else
            {
                text = AddLineBreaks(create_text(), GetMaxCharsPerLine());
            }
            
            return text;
        }


        public string UpdateToolTipText()
        {
            if (typing_type == "time")
            {
                return $"{time_typing}";
            }
            else
            {
                return $"0/{words_count}";
            }
        }
        public string GetToolTipText()
        {
            if (typing_type == "time")
            {
                return $"{Math.Round(time_typing - stopwatch.Elapsed.TotalSeconds)}";
            }
            else
            {
                int wordsTyped = GetWordsTyped();
                
                if (wordsTyped > words_count)
                {
                    wordsTyped = words_count;
                }
                
                return $"{wordsTyped}/{words_count}";
            }
        }

        public string create_text(int wordCount = 0)
        {
            if (wordCount == 0 && typing_type == "words")
            {
                wordCount = words_count;
            }
            else if (wordCount == 0 && typing_type == "time")
            {
                wordCount = words_count;
            }
            
            Random rnd = new Random();
            
            int maxCount = words.Length;
            if (wordCount > maxCount)
            {
                wordCount = maxCount;
            }

            List<int> selectedIndices = new List<int>();
            List<string> textlist = new List<string>();

            while (textlist.Count < wordCount)
            {
                int index = rnd.Next(0, maxCount);
                
                if (selectedIndices.Contains(index)) continue;
                
                selectedIndices.Add(index);
                
                textlist.Add(words[index]);
            }
            
            if (AddPunctuation)
            {
                string[] allowedPunctuation = get_marks();
                
                bool capitalizeNext = false;
                
                int index = rnd.Next(1, 5);
                
                while (index < textlist.Count)
                {
                    if (capitalizeNext && index + 1 < textlist.Count)
                    {
                        string nextWord = textlist[index + 1];
                        textlist[index + 1] = char.ToUpper(nextWord[0]) + nextWord.Substring(1);
                        capitalizeNext = false;
                    }
                    
                    if (index < textlist.Count)
                    {
                        string punctuation = allowedPunctuation[rnd.Next(0, allowedPunctuation.Length)];
                        textlist[index] = textlist[index] + punctuation;
                        
                        if (punctuation == "." || punctuation == "!" || punctuation == "?")
                        {
                            capitalizeNext = true;
                        }
                        
                        index += rnd.Next(2, 6);
                    }
                }
            }
            
            if (AddNumbers)
            {
                int index = rnd.Next(1, 6);
                while (index < textlist.Count)
                {
                    if (index < textlist.Count)
                    {
                        textlist[index] =  numbers[rnd.Next(0, numbers.Length)];
                    }
                    index += rnd.Next(2, 6);
                }
            }
            
            text = String.Join(" ", textlist);
            
            return text;
        }

        private double GetAverageCharacterWidth()
        {
            var formattedText = new FormattedText(
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas"),
                charSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return formattedText.Width / formattedText.Text.Length;
        }

        public int GetMaxCharsPerLine()
        {
            double averageCharWidth = GetAverageCharacterWidth();
            return (int)((textbLockWidth - 10) / averageCharWidth);
        }
        public string AddLineBreaks(string text, int maxCharsPerLine)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            
            text = text.Trim();
            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }
            
            var words = text.Split(' ');
            var result = new StringBuilder();
            var currentLineLength = 0;

            words = words.Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();

            if (words.Length == 0)
            {
                return string.Empty;
            }

            foreach (var word in words)
            {
                if (word.Length > maxCharsPerLine)
                {
                    if (currentLineLength > 0)
                    {
                        result.Append(' ');
                        result.Append('\n');
                        currentLineLength = 0;
                    }
                    
                    result.Append(word);
                    
                    result.Append(' ');
                    result.Append('\n');
                    currentLineLength = 0;
                    continue;
                }
                
                if (currentLineLength + word.Length + 1 > maxCharsPerLine)
                {
                    result.Append(' ');
                    result.Append('\n');
                    currentLineLength = 0;
                }
                else if (currentLineLength > 0)
                {
                    result.Append(' ');
                    currentLineLength++;
                }
                result.Append(word);
                currentLineLength += word.Length;
            }
            
            string finalText = result.ToString();
            if (finalText.EndsWith("\n"))
            {
                finalText = finalText.TrimEnd('\n');
            }
            
            return finalText;
        }

        public ObservableCollection<ColoredCharacter> UpdateColoredUserInput()
        {
            coloredUserInput.Clear();
            
            if (string.IsNullOrEmpty(text))
            {
                return coloredUserInput;
            }
            
            for (int i = 0; i < userInput.Length; i++)
            {
                if (i >= text.Length)
                {
                    break;
                }
                
                char character = text[i];
                ColoredCharacter currentColoredChar = CheckCharacter(userInput[i], character);
                coloredUserInput.Add(currentColoredChar);
                
                if (currentColoredChar.IsFunctionalSpace)
                {
                    UpdateLastWordDelimiterPosition(i);
                }
                
                CheckForNewLine(i);
                
                if (IsNewLine)
                {
                    break;
                }
            }
            
            if (!IsNewLine && userInput.Length > 0 && text.Length > userInput.Length)
            {
                CheckForNewLine(userInput.Length - 1);
            }
            
            if (userInput.Length > 0 && userInput.Length <= text.Length)
            {
                UpdateCharacterCounts(userInput.Length - 1);
            }
            
            CheckTimer();
            
            return coloredUserInput;
        }
        public void UpdateCharacterCounts(int lastInputIndex)
        {
            if (userInput.Length <= 0 || lastInputIndex < 0 || lastInputIndex >= userInput.Length || lastInputIndex >= text.Length) 
                return;

            if (userInput[lastInputIndex] == text[lastInputIndex])
            {
                CorrectCount++;
            }
            else
            {
                MistakesCount++;
            }
        }

        public void UpdateRestartCount()
        {
            RestartCount++;
        }

        public ColoredCharacter CheckCharacter(char UserCharacter, char TextCharacter)
        {
            ColoredCharacter coloredChar = new ColoredCharacter
            {
                FontSizeText = charSize,
                IsFunctionalSpace = false 
            };

            State state = GetState(UserCharacter, TextCharacter);
            coloredChar.State = state;

            if (state == State.Correct)
            {
                coloredChar.Character = TextCharacter.ToString();
                if (TextCharacter == ' ')
                {
                    coloredChar.IsFunctionalSpace = true;
                }
            }
            else 
            {
                if (TextCharacter == ' ') 
                {
                    coloredChar.Character = "_"; 
                    coloredChar.IsFunctionalSpace = true; 
                }
                else if (UserCharacter == ' ') 
                {
                    coloredChar.Character = TextCharacter.ToString(); 
                    coloredChar.IsFunctionalSpace = false; 
                }
                else 
                {
                    coloredChar.Character = TextCharacter.ToString(); 
                    coloredChar.IsFunctionalSpace = false; 
                }
            }
            return coloredChar;
        }

        public State GetState(char UserCharacter, char TextCharacter)
        {
            if (UserCharacter == TextCharacter)
            {
                return State.Correct;
            }
            else
            {
                return State.Incorrect;
            }
        }

        private void CheckForNewLine(int LastCharIndex)
        {
            if (LastCharIndex < 0 || LastCharIndex >= text.Length - 1)
                return;
                
            if (text.Length > LastCharIndex + 1 && text[LastCharIndex + 1] == '\n')
            {
                if (userInput.Length <= LastCharIndex)
                    return;
                    
                bool shouldProcessNewLine = 
                    (LastCharIndex >= 0 && userInput[LastCharIndex] == ' ') || 
                    (LastCharIndex >= 0 && userInput.Length >= LastCharIndex + 1); 

                if (shouldProcessNewLine)
                {
                    bool endsWithSpace = userInput.Length > 0 && userInput[userInput.Length - 1] == ' ';
                    
                    if (!endsWithSpace)
                    {
                        userInput += " "; 
                        coloredUserInput.Add(new ColoredCharacter 
                        { 
                            Character = " ", 
                            State = State.None, 
                            FontSizeText = charSize,
                            IsFunctionalSpace = true 
                        });
                    }
                    
                    foreach (var c in coloredUserInput)
                    {
                        UserTotalInput.Add(c);
                    }
                    
                    int wordsTyped = GetWordsTyped();
                    
                    if (LastCharIndex + 2 < text.Length)
                    {
                        string remainingText = new string(text.Skip(LastCharIndex + 2).ToArray());
                        
                        if (typing_type == "words") 
                        {
                            string cleanText = remainingText.Replace("\n", " ").Trim();
                            while (cleanText.Contains("  "))
                            {
                                cleanText = cleanText.Replace("  ", " ");
                            }
                            
                            if (!string.IsNullOrWhiteSpace(cleanText))
                            {
                                string[] words = cleanText.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
                                
                                cleanText = string.Join(" ", words);
                                
                                text = AddLineBreaks(cleanText, GetMaxCharsPerLine());
                            }
                            else
                            {
                                if (wordsTyped < words_count)
                                {
                                    text = "последнее"; 
                                }
                                else
                                {
                                    text = "";
                                }
                                return;
                            }
                        }
                        else
                        {
                            text = remainingText;
                        }
                        
                        needMoreLines = true;
                    }
                    else
                    {
                        if (typing_type == "words")
                        {
                            if (wordsTyped >= words_count)
                            {
                                text = "";
                            }
                            else if (words_count - wordsTyped <= 1)
                            {
                                text = "последнее";
                            }
                            else
                            {
                                text = AddLineBreaks(create_text(), GetMaxCharsPerLine());
                            }
                        }
                        else
                        {
                            text = AddLineBreaks(create_text(), GetMaxCharsPerLine());
                        }
                        needMoreLines = true;
                    }
                    
                    if (needMoreLines && !string.IsNullOrEmpty(text))
                    {
                        EnsureEnoughLines();
                        needMoreLines = false;
                    }
                    
                    ResetEditingRestrictions();
                    
                    userInput = string.Empty;
                    coloredUserInput.Clear();
                    IsNewLine = true;
                }
            }
        }

        private void EnsureEnoughLines()
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
                
            if (typing_type == "words")
            {
                int currentLineCount = text.Count(c => c == '\n') + 1;
                int minNeededLines = 3;
                
                text = text.Trim();
                
                while (text.Contains("\n\n"))
                {
                    text = text.Replace("\n\n", "\n");
                }
                
                string[] lines = text.Split('\n');
                
                lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                
                if (lines.Length == 0)
                    return;
                
                text = string.Join("\n", lines);
                
                currentLineCount = lines.Length;
                
                if (currentLineCount < minNeededLines)
                {
                    string cleanText = text.Replace("\n", " ").Trim();
                    while (cleanText.Contains("  "))
                    {
                        cleanText = cleanText.Replace("  ", " ");
                    }
                    
                    string[] words = cleanText.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
                    
                    if (words.Length == 0)
                        return;
                    
                    cleanText = string.Join(" ", words);
                    
                    text = AddLineBreaks(cleanText, GetMaxCharsPerLine());
                }
                return;
            }
            
            int timeLineCount = text.Count(c => c == '\n') + 1;
            
            if (timeLineCount < desiredLineCount)
            {
                string additionalText = AddLineBreaks(create_text(), GetMaxCharsPerLine());
                
                text += additionalText;
                
                EnsureEnoughLines();
            }
        }

        private string[] get_words()
        {
            try
            {
                List<string> wordsList = new List<string>();
                
                try
                {
                    string languageFilePath = Path.Combine(languages_path, language + ".json");
                    
                    if (language.Equals("code_csharp", StringComparison.OrdinalIgnoreCase))
                    {
                        return GetSafeCSharpWords();
                    }
                    
                    if (File.Exists(languageFilePath))
                    {
                        try
                        {
                            string jsonContent = File.ReadAllText(languageFilePath);
                            
                            try
                            {
                                dynamic languageData = JsonConvert.DeserializeObject(jsonContent);
                                
                                if (languageData != null && languageData.words != null)
                                {
                                    foreach (string word in languageData.words)
                                    {
                                        if (!string.IsNullOrWhiteSpace(word))
                                        {
                                            wordsList.Add(word);
                                        }
                                    }
                                }
                            }
                            catch (Exception jsonEx)
                            {
                                
                                try
                                {
                                    if (jsonContent.Contains("\"words\""))
                                    {
                                        int wordsStart = jsonContent.IndexOf("\"words\"");
                                        int arrayStart = jsonContent.IndexOf('[', wordsStart);
                                        int arrayEnd = jsonContent.IndexOf(']', arrayStart);
                                        
                                        if (arrayStart > 0 && arrayEnd > arrayStart)
                                        {
                                            string wordsArray = jsonContent.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
                                            string[] parts = wordsArray.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                            
                                            foreach (string part in parts)
                                            {
                                                string cleaned = part.Trim();
                                                if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
                                                {
                                                    cleaned = cleaned.Substring(1, cleaned.Length - 2);
                                                    if (!string.IsNullOrWhiteSpace(cleaned))
                                                    {
                                                        wordsList.Add(cleaned);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch (Exception fileEx)
                        {
                        }
                    }
                }
                catch (Exception) {}
                
                if (wordsList.Count == 0)
                {
                    try
                    {
                        string englishFile = Path.Combine(languages_path, "english.json");
                        if (File.Exists(englishFile) && !language.Equals("english", StringComparison.OrdinalIgnoreCase))
                        {
                            string jsonContent = File.ReadAllText(englishFile);
                            dynamic languageData = JsonConvert.DeserializeObject(jsonContent);
                            
                            if (languageData != null && languageData.words != null)
                            {
                                foreach (string word in languageData.words)
                                {
                                    if (!string.IsNullOrWhiteSpace(word))
                                    {
                                        wordsList.Add(word);
                                    }
                                }
                            }
                        }
                    }
                    catch {}
                    
                    if (wordsList.Count == 0)
                    {
                        try
                        {
                            if (File.Exists(words_path))
                            {
                                StreamReader f = new StreamReader(words_path, Encoding.GetEncoding("windows-1251"));
                                while (!f.EndOfStream)
                                {
                                    string s = f.ReadLine();
                                    if (!string.IsNullOrWhiteSpace(s))
                                    {
                                        wordsList.Add(s);
                                    }
                                }
                                f.Close();
                            }
                        }
                        catch {}
                    }
                }
                
                if (wordsList.Count == 0)
                {
                    wordsList.AddRange(new[] { 
                        "test", "word", "typing", "practice", "keyboard", 
                        "speed", "accuracy", "language", "multilingual", "text" 
                    });
                }
                
                return wordsList.ToArray();
            }
            catch (Exception)
            {
                return new[] { 
                    "test", "word", "typing", "practice", "keyboard", 
                    "speed", "accuracy", "language", "multilingual", "text" 
                };
            }
        }
        
        private string[] GetSafeCSharpWords()
        {
            return new[] {
                "using", "namespace", "class", "interface", "struct", "enum",
                "public", "private", "protected", "internal", "static", "readonly",
                "const", "void", "int", "string", "bool", "float", "double",
                "if", "else", "switch", "case", "for", "foreach", "while", "do",
                "try", "catch", "finally", "throw", "new", "return", "break",
                "continue", "this", "base", "null", "true", "false", "object",
                "virtual", "override", "abstract", "sealed", "get", "set",
                "delegate", "event", "async", "await", "var", "dynamic", "typeof",
                "is", "as", "in", "out", "ref", "params", "yield", "partial",
                "where", "select", "from", "join", "group", "by", "into", "on",
                "equals", "orderby", "ascending", "descending", "let", "default"
            };
        }
        private string[] get_marks()
        {
            List<string> marks = new List<string>();
            
            string[] allowedMarks = new string[] { ".", ",", "!", "?", ";", ":" };
            
            try
            {
                StreamReader f = new StreamReader(marks_path);
                while (!f.EndOfStream)
                {
                    string s = f.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(s) && allowedMarks.Contains(s))
                    {
                        marks.Add(s);
                    }
                }
                f.Close();
            }
            catch
            {
                marks.AddRange(allowedMarks);
            }
            
            if (marks.Count == 0)
            {
                marks.AddRange(allowedMarks);
            }
            
            return marks.ToArray();
        }
        private string[] get_numbers()
        {
            List<string> numbers = new List<string>();
            StreamReader f = new StreamReader(numbers_path);
            while (!f.EndOfStream)
            {
                string s = f.ReadLine();
                numbers.Add(s);
            }
            f.Close();
            return numbers.ToArray();
        }
        public bool IsFirstLine()
        {
            return UserTotalInput.Count > 0;
        }

        public void ChangePunctuationValue()
        {
            AddPunctuation = !AddPunctuation;
            UserSettings.SaveAddPunctuation(AddPunctuation);
        }
        public void ChangeNumbersValue()
        {
            AddNumbers = !AddNumbers;
            UserSettings.SaveAddNumbers(AddNumbers);
        }
        public void ChangeTypeTyping()
        {
            if (typing_type == "time")
            {
                typing_type = "words";
                words_count = words_count_list[0];
            }
            else
            {
                typing_type = "time";
                words_count = words_count_list[4];
            }
            UserSettings.SaveTypingType(typing_type);
            UserSettings.SaveWordsCount(words_count);
        }
        public void ChangeParameter(int number)
        {
            if (typing_type == "time")
            {
                time_typing = number;
                UserSettings.SaveTimeTyping(time_typing);
            }
            else
            {
                words_count = number;
                UserSettings.SaveWordsCount(words_count);
            }
        }
        public string GetTypingType()
        {
            return typing_type;
        }

        public bool GetIsNumbers()
        {
            return AddNumbers;
        }

        public bool GetIsPunctuation()
        {
            return AddPunctuation;
        }

        public int GetRestartCount()
        {
            return RestartCount;
        }

        public string GetText()
        {
            return text;
        }
        public bool GetIsNewLine()
        {
            return IsNewLine;
        }
        public void UpdateIsNewLine()
        {
            IsNewLine = false;
        }
        public string GetInputText()
        {
            return userInput;
        }
        public ObservableCollection<ColoredCharacter> GetColoredUserInput()
        {
            return coloredUserInput;
        }
        public bool IsTestComplete()
        {
            if (testCompleted)
                return true;
                
            bool isComplete = false;
            
            if (typing_type == "time")
            {
                isComplete = stopwatch.ElapsedMilliseconds / 1000 >= time_typing;
            }
            else
            {
                int wordsTyped = GetWordsTyped();
                
                bool textFullyTyped = !string.IsNullOrEmpty(userInput) && text.Length > 0 && text.Length == userInput.Length;
                bool lastWordTyped = userInput.Length > 0 && text == userInput;
                
                if (typing_type == "words")
                {
                    
                    if (wordsTyped >= words_count &&
                        (textFullyTyped || lastWordTyped || string.IsNullOrWhiteSpace(text)))
                    {
                        isComplete = true;
                    }
                    else if (textFullyTyped && text == "последнее" && wordsTyped >= words_count - 1)
                    {
                        isComplete = true;
                    }
                    else
                    {
                        isComplete = false;
                    }
                }
                else
                {
                    isComplete = textFullyTyped;
                }
            }
            
            if (isComplete && !testCompleted)
            {
                CompleteTest();
            }
            
            return isComplete;
        }
        private void CheckTimer()
        {
            if (testCompleted)
                return;
                
            if (!string.IsNullOrEmpty(userInput) && !stopwatch.IsRunning)
            {
                stopwatch.Start();
                RecordPeriodData();
            }
            else if (string.IsNullOrEmpty(userInput) && stopwatch.IsRunning && UserTotalInput.Count == 0)
            {
                stopwatch.Stop();
                stopwatch.Reset();
            }
        }

        private async Task RecordPeriodData()
        {
            PeriodData.Clear();
            MissedCount = 0;
            while (stopwatch.IsRunning && !testCompleted)
            {
                try
                {
                    await Task.Delay(1000);
                    
                    if (testCompleted || !stopwatch.IsRunning)
                        break;
                    
                    double elapsedMinutes = stopwatch.Elapsed.TotalMinutes;
                    if (elapsedMinutes <= 0) elapsedMinutes = 0.016; 
                    
                    int correctChars = GetCorrectStateCount();
                    int allChars = GetCharCount();
                    
                    double rawwpm = Math.Floor((double)allChars / StandardCharsPerWord / elapsedMinutes);
                    
                    double wpm;
                    
                    bool perfectAccuracy = MistakesCount == 0 && MissedCount == 0; 
                    
                    if (perfectAccuracy)
                    {
                        wpm = rawwpm;
                    }
                    else
                    {
                        wpm = Math.Floor((double)correctChars / StandardCharsPerWord / elapsedMinutes);
                    }
                    
                    wpm = Math.Min(300, Math.Max(0, wpm));
                    rawwpm = Math.Min(300, Math.Max(0, rawwpm));
                    
                    PeriodData.Add(new PeriodData
                    {
                        Time = stopwatch.Elapsed,
                        WPM = wpm,
                        RawWPM = rawwpm,
                        Mistakes = GetMistakesStateCount() + MissedCount
                    });
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        public string GetTestType()
        {
            string parameter = typing_type == "time" ? time_typing.ToString() : words_count.ToString();
            return typing_type + " " + parameter;
        }
        public string GetLanguage()
        {
            return language;
        }

        public double GetWPM()
        {
            if (testCompleted && cachedWPM.HasValue)
            {
                return cachedWPM.Value;
            }
            
            if (!testCompleted)
            {
                CompleteTest();
                return cachedWPM.Value;
            }
            
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
            
            var elapsedTime = stopwatch.Elapsed.TotalSeconds;
            
            const double MIN_TIME_SECONDS = 0.1; 
            if (elapsedTime < MIN_TIME_SECONDS)
            {
                elapsedTime = MIN_TIME_SECONDS;
            }
            
            double elapsedMinutes = elapsedTime / 60.0;
            
            if (elapsedMinutes <= 0) 
                elapsedMinutes = 0.001; 
            
            int correctChars = GetCorrectStateCount();
            int totalChars = GetCharCount();
            
            if (totalChars <= 0)
                return 0;
            
            if (MistakesCount == 0 && MissedCount == 0) 
            {
                double wpm = (totalChars / StandardCharsPerWord) / elapsedMinutes;
                return Math.Round(Math.Min(800, Math.Max(0, wpm))); 
            }
            else
            {
                double wpm = (correctChars / StandardCharsPerWord) / elapsedMinutes;
                return Math.Round(Math.Min(800, Math.Max(0, wpm)));
            }
        }

        public double GetRawWPM()
        {
            if (testCompleted && cachedRawWPM.HasValue)
            {
                return cachedRawWPM.Value;
            }
            
            if (!testCompleted)
            {
                CompleteTest();
                return cachedRawWPM.Value;
            }
            
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
            
            var elapsedTime = stopwatch.Elapsed.TotalSeconds;
            
            const double MIN_TIME_SECONDS = 0.1; 
            if (elapsedTime < MIN_TIME_SECONDS)
            {
                elapsedTime = MIN_TIME_SECONDS;
            }
            
            double elapsedMinutes = elapsedTime / 60.0;
            
            if (elapsedMinutes <= 0) 
                elapsedMinutes = 0.001; 
            
            int totalChars = GetCharCount();
            
            if (totalChars <= 0)
                return 0;
            
            double rawwpm = (totalChars / StandardCharsPerWord) / elapsedMinutes;
            
            return Math.Round(Math.Min(800, Math.Max(0, rawwpm))); 
        }

        public double GetAccuracy()
        {
            if (MistakesCount == 0 && MissedCount == 0 && CorrectCount > 0)
            {
                return 100.0;
            }
            
            int correctChars = GetCorrectStateCount();
            int totalChars = GetCharCount();
            
            if (totalChars > 0)
            {
                if (correctChars == totalChars)
                {
                    return 100.0;
                }
                
                if (correctChars > totalChars)
                {
                    return 100.0;
                }
                
                int effectiveCorrectChars = Math.Min(correctChars, totalChars);
                double accuracy = Math.Round(((double)effectiveCorrectChars / totalChars) * 100, 2);
                return Math.Min(100.0, Math.Max(0.0, accuracy));
            }
            else
            {
                return 100.0;
            }
        }

        public double GetConsistency()
        {
            if (PeriodData == null || !PeriodData.Any())
                return 100.0;

            double avgWPM = PeriodData.Average(pd => pd.WPM);

            if (avgWPM == 0)
                return 100.0;

            double dispersion = PeriodData.Average(pd => Math.Pow(pd.WPM - avgWPM, 2));
            double deviation = Math.Sqrt(dispersion);
            
            double consistencyValue = (1 - (deviation / avgWPM)) * 100;
            
            if (double.IsNaN(consistencyValue))
                return 100.0;
            
            return Math.Min(100.0, Math.Max(0.0, consistencyValue));
        }
        public List<PeriodData> GetPeriodData()
        {
            return PeriodData;
        }
        public string GetChars()
        {
            return $"{GetCorrectStateCount()}/{GetMistakesStateCount()}";
        }
        public TimeSpan GetTime()
        {
            return stopwatch.Elapsed;
        }
        public int GetCorrectCount()
        {
            return CorrectCount;
        }
        public int GetMistakesCount()
        {
            return MistakesCount;
        }
        
        public int GetWordsTyped()
        {
            int totalWords = 0;
            
            if (UserTotalInput.Count > 0)
            {
                bool inWord = false;
                foreach (var ch in UserTotalInput)
                {
                    if (ch.IsFunctionalSpace) 
                    {
                        if (inWord)
                        {
                            totalWords++;
                            inWord = false;
                        }
                    }
                    else
                    {
                        inWord = true;
                    }
                }
                
            }
            
            if (coloredUserInput.Count > 0)
            {
                bool inWord = false;
                bool hasCompletedWord = false;
                
                for (int i = 0; i < coloredUserInput.Count; i++)
                {
                    var ch = coloredUserInput[i];
                    if (ch.IsFunctionalSpace) 
                    {
                        if (inWord)
                        {
                            totalWords++;
                            hasCompletedWord = true;
                            inWord = false;
                        }
                    }
                    else
                    {
                        inWord = true;
                    }
                }
                
                
                if (inWord && !string.IsNullOrEmpty(userInput) && !string.IsNullOrEmpty(text))
                {
                    bool isTextFullyTyped = text.Length == userInput.Length;
                    bool isLastWordText = text == "последнее";
                    bool containsLastWord = userInput.Contains("последнее");
                    
                    if (isTextFullyTyped || (isLastWordText && containsLastWord))
                    {
                        totalWords++;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(text) && text == "последнее" && 
                !string.IsNullOrEmpty(userInput))
            {
                int minLength = Math.Min(userInput.Length, "последнее".Length);
                string partTyped = userInput.Substring(0, minLength);
                string partExpected = "последнее".Substring(0, minLength);
                
                if (partTyped == partExpected && minLength >= 4 &&
                    words_count - totalWords <= 1)
                {
                    return words_count;
                }
            }
            
            return totalWords;
        }


        private int GetCharCount()
        {
            if (UserTotalInput.Count > 0)
            {
                return UserTotalInput.Count;
            }
            else
            {
                return coloredUserInput.Count;
            }
        }
        
        private int GetCorrectStateCount()
        {
            if (UserTotalInput.Count > 0)
            {
                return UserTotalInput.Where(ch => ch.State == State.Correct).Count();
            }
            else
            {
                return coloredUserInput.Where(ch => ch.State == State.Correct).Count();
            }
        }
        private int GetMistakesStateCount()
        {
            if (UserTotalInput.Count > 0)
            {
                return UserTotalInput.Where(ch => ch.State == State.Incorrect).Count();
            }
            else
            {
                return coloredUserInput.Where(ch => ch.State == State.Incorrect).Count();
            }
        }
        private int GetMissedStateCount()
        {
            if (UserTotalInput.Count > 0)
            {
                return UserTotalInput.Where(ch => ch.State == State.Missed).Count();
            }
            else
            {
                return coloredUserInput.Where(ch => ch.State == State.Missed).Count();
            }
        }
        private void FillTotalInput()
        {
            foreach (var i in coloredUserInput)
            {
                UserTotalInput.Add(i);
            }
            
            bool endsWithSpace = coloredUserInput.Count > 0 && 
                                 coloredUserInput.Last().Character == " ";
            
            bool lastCharInColoredIsFunctionalSpace = coloredUserInput.Count > 0 && 
                                                      coloredUserInput.Last().IsFunctionalSpace;

            if (coloredUserInput.Count > 0 && !lastCharInColoredIsFunctionalSpace && 
                (typing_type != "words" || GetWordsTyped() < words_count - 1)) 
            {
                UserTotalInput.Add(new ColoredCharacter { 
                    Character = " ", 
                    State = State.None, 
                    FontSizeText = charSize,
                    IsFunctionalSpace = true 
                });
            }
        }

        private void CompleteTest()
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
            
            bool requiresFinalFunctionalSpace = false;
            if (coloredUserInput.Count > 0)
            {
                if (!coloredUserInput.Last().IsFunctionalSpace)
                {
                    requiresFinalFunctionalSpace = true;
                }
            }

            foreach (var c in coloredUserInput)
            {
                UserTotalInput.Add(c);
            }
            
            if (requiresFinalFunctionalSpace)
            {
                UserTotalInput.Add(new ColoredCharacter { 
                    Character = " ", 
                    State = State.None, 
                    FontSizeText = charSize,
                    IsFunctionalSpace = true 
                });
            }
            
            var elapsedTime = stopwatch.Elapsed.TotalSeconds;
            
            const double MIN_TIME_SECONDS = 0.1; 
            if (elapsedTime < MIN_TIME_SECONDS)
            {
                elapsedTime = MIN_TIME_SECONDS;
            }
            
            double elapsedMinutes = elapsedTime / 60.0;
            
            if (elapsedMinutes <= 0) 
                elapsedMinutes = 0.001; 
            
            int correctChars = GetCorrectStateCount();
            int totalChars = GetCharCount();
            
            if (totalChars > 0)
            {
                double rawwpm = (totalChars / StandardCharsPerWord) / elapsedMinutes;
                
                bool perfectAccuracy = MistakesCount == 0 && MissedCount == 0; 
                
                if (perfectAccuracy)
                {
                    cachedWPM = Math.Round(Math.Min(800, Math.Max(0, rawwpm)));
                    cachedRawWPM = cachedWPM;
                }
                else
                {
                    double wpm = (correctChars / StandardCharsPerWord) / elapsedMinutes;
                    
                    cachedWPM = Math.Round(Math.Min(800, Math.Max(0, wpm)));
                    cachedRawWPM = Math.Round(Math.Min(800, Math.Max(0, rawwpm)));
                }
            }
            else
            {
                cachedWPM = 0;
                cachedRawWPM = 0;
            }
            
            testCompleted = true;
        }

        public void ResetTestState()
        {
            CorrectCount = 0;
            MistakesCount = 0;
            MissedCount = 0;
            
            testCompleted = false;
            cachedWPM = null;
            cachedRawWPM = null;
            
            ResetEditingRestrictions();
        }
        
        public string ReformatText()
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            
            string cleanText = text.Replace("\n", " ");
            
            while (cleanText.Contains("  "))
            {
                cleanText = cleanText.Replace("  ", " ");
            }
            
            cleanText = cleanText.Trim();
            
            int maxCharsPerLine = GetMaxCharsPerLine();
            
            string formattedText = AddLineBreaks(cleanText, maxCharsPerLine);
            
            return formattedText;
        }
        
        private void UpdateLastWordDelimiterPosition(int index)
        {
            if (index < 0 || index >= text.Length)
                return;
                
            bool isSpaceInOriginalText = text[index] == ' ';
            
            bool isUserInputSpace = index < userInput.Length && userInput[index] == ' ';
            
            if (isSpaceInOriginalText || isUserInputSpace)
            {
                LastWordDelimiterPosition = index;
                HasWordDelimiters = true;
            }
        }

        public void ResetEditingRestrictions()
        {
            LastWordDelimiterPosition = -1;
            HasWordDelimiters = false;
        }

        public string ValidateUserInput(string newInput)
{
    if (string.IsNullOrEmpty(newInput) || string.IsNullOrEmpty(text))
        return newInput;

    if (newInput.Length < userInput.Length)
    {
        int lastDelimiterPos = FindLastWordDelimiterPosition();
        
        if (IsAtLineBeginning(lastDelimiterPos + 1))
        {
            int firstIncorrectPos = FindFirstIncorrectPosition(lastDelimiterPos + 1);
            
            if (firstIncorrectPos >= 0 && newInput.Length >= firstIncorrectPos)
            {
                return newInput;
            }
        }
        
        if (newInput.Length > lastDelimiterPos)
        {
            return newInput;
        }
        else
        {
            return userInput;
        }
    }
    
    return newInput;
}

private int FindLastWordDelimiterPosition()
{
    if (HasWordDelimiters)
    {
        return LastWordDelimiterPosition;
    }
    
    for (int i = userInput.Length - 1; i >= 0; i--)
    {
        if (i < text.Length && (text[i] == ' ' || text[i] == '\n'))
        {
            return i;
        }
    }
    
    return -1;
}

private int FindFirstIncorrectPosition(int startPos)
{
    for (int i = startPos; i < userInput.Length && i < text.Length; i++)
    {
        if (userInput[i] != text[i])
        {
            return i;
        }
    }
    return -1; 
}

private bool IsAtLineBeginning(int position)
{
    if (position == 0)
        return true;
        
    if (position > 0 && position < text.Length)
    {
        return text[position - 1] == '\n';
    }
    
    return false;
}

        public void SetAllSettings(string newLanguage, string newTypingType, int newTimeTyping, 
            int newWordsCount, bool newAddPunctuation, bool newAddNumbers)
        {
            try
            {
                language = newLanguage;
                typing_type = newTypingType;
                time_typing = newTimeTyping;
                words_count = newWordsCount;
                AddPunctuation = newAddPunctuation;
                AddNumbers = newAddNumbers;
                
                UserSettings.SaveAllSettings(language, typing_type, time_typing, words_count, AddPunctuation, AddNumbers);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureCodeLanguage(Dictionary<string, object> langData)
        {
            try
            {
                if (language.Equals("code_csharp", StringComparison.OrdinalIgnoreCase) || 
                    language.Equals("code_c#", StringComparison.OrdinalIgnoreCase))
                {
                    words = GetSafeCSharpWords();
                }
                else if (langData.TryGetValue("words", out var wordsObj) && wordsObj is Newtonsoft.Json.Linq.JArray wordsArray)
                {
                    words = wordsArray.ToObject<string[]>();
                }
                else
                {
                    words = get_words();
                }
            }
            catch
            {
                words = get_words();
            }
        }
    }
}

