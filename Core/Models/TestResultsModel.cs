using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using krushtype.Core.Utilities;
using krushtype.Core.Utilities.files;
using Ookii.Dialogs.Wpf;

namespace krushtype.Core.Models
{
    public class TestResultsModel
    {
        public async void AddTest(string Mode, double WPM, double RawWPM, double Accuracy, double Consistency, bool IsNumbers, bool IsPunctuation, string Language, string Chars, int RestartCount, TimeSpan Time, int WordsTyped)
        {
            await CSVData.add_test(Mode, WPM, RawWPM, Accuracy, Consistency, IsNumbers, IsPunctuation, Language, Chars, RestartCount, Time);
            JsonData.update_tests_started(RestartCount + 1);
            JsonData.update_tests_completed();
            JsonData.update_time_typing(Time);
            JsonData.update_words_typed(WordsTyped);
        }
    }
}
