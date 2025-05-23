using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using krushtype.Core.ViewModels;

namespace krushtype.UI.Views
{
    public partial class typing : UserControl
    {
        private typingVM ViewModel => DataContext as typingVM;

        public typing()
        {
            InitializeComponent();
            this.Loaded += Typing_Loaded;
            this.SizeChanged += Typing_SizeChanged;
        }
        
        private void Typing_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTextWidth();
        }

        private void Typing_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTextWidth();
        }

        private void UpdateTextWidth()
        {
            if (ViewModel != null && TextContainer != null)
            {
                double actualWidth = TextContainer.ActualWidth;
                
                if (actualWidth >= 200)
                {
                    ViewModel.UpdateTextWidth(actualWidth);
                }
            }
        }
        
        private void LanguageSelector_Click(object sender, RoutedEventArgs e)
        {
            LanguagePopup.IsOpen = !LanguagePopup.IsOpen;
            
            if (LanguagePopup.IsOpen)
            {
                LanguageSearchBox.Text = string.Empty;
                
                LanguageSearchBox.Focus();
                
                if (DataContext is typingVM viewModel)
                {
                    viewModel.FilterLanguages(string.Empty);
                }
            }
        }
        
        private void LanguageSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is typingVM viewModel)
            {
                viewModel.FilterLanguages(LanguageSearchBox.Text);
            }
        }

        private void LanguageItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Child is TextBlock textBlock)
            {
                if (DataContext is typingVM viewModel)
                {
                    viewModel.SelectedDisplayLanguage = textBlock.Text;
                    
                    LanguagePopup.IsOpen = false;
                }
            }
        }
        
        private void LanguageItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(79, 81, 86)); 
            }
        }
        
        private void LanguageItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = Brushes.Transparent;
            }
        }
        
        private void CustomParameterDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (CustomParameterDialog.Visibility == Visibility.Visible)
            {
                CustomValueTextBox?.Focus();
            }
        }
        
        private void CustomValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void CustomValueTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CustomParameterDialog.Visibility = Visibility.Collapsed;
            }
            else if (e.Key == Key.Escape)
            {
                CustomParameterDialog.Visibility = Visibility.Collapsed;
            }
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || !(DataContext is typingVM viewModel)) return;

            var typingModel = viewModel.GetTypingModel();
            if (typingModel == null) return;

            if (e.Key == Key.Back)
            {
                if (typingModel.HasWordDelimiters && textBox.CaretIndex <= typingModel.LastWordDelimiterPosition)
                {
                    e.Handled = true;
                    textBox.CaretIndex = typingModel.LastWordDelimiterPosition + 1;
                }
                return; 
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control || 
                (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true;
                return;
            }

            bool isAllowedKey = false;

            if ((e.Key >= Key.A && e.Key <= Key.Z) || 
                (e.Key >= Key.OemSemicolon && e.Key <= Key.OemBackslash) ||
                (e.Key == Key.OemCloseBrackets) ||
                (e.Key == Key.OemTilde) ||
                (e.Key == Key.OemOpenBrackets) ||
                (e.Key == Key.OemQuotes) ||
                (e.Key == Key.OemComma) ||
                (e.Key == Key.OemPeriod) ||
                (e.Key == Key.Oem1) ||
                (e.Key == Key.Oem2) ||
                (e.Key == Key.Oem3) ||
                (e.Key == Key.Oem4) ||
                (e.Key == Key.Oem5) ||
                (e.Key == Key.Oem6) ||
                (e.Key == Key.Oem7) ||
                (e.Key == Key.Oem8))
            {
                isAllowedKey = true;
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                isAllowedKey = true;
            }
            else if (e.Key == Key.Space)
            {
                isAllowedKey = true;
            }
            else if (e.Key == Key.OemMinus || e.Key == Key.Subtract || e.Key == Key.Decimal || e.Key == Key.Divide)
            {
                isAllowedKey = true;
            }
            
            if (!isAllowedKey)
            {
                e.Handled = true;
            }
        }

        private void InputTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox && DataContext is typingVM viewModel)
            {
                var typingModel = viewModel.GetTypingModel();
                
                if (!typingModel.HasWordDelimiters)
                    return;
                
                int clickPosition = -1;
                try
                {
                    clickPosition = textBox.GetCharacterIndexFromPoint(e.GetPosition(textBox), false);
                }
                catch
                {
                    return;
                }
                
                if (clickPosition <= typingModel.LastWordDelimiterPosition)
                {
                    e.Handled = true;
                    
                    textBox.Focus();
                    
                    textBox.CaretIndex = typingModel.LastWordDelimiterPosition + 1;
                }
            }
        }
        
        private void InputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && DataContext is typingVM viewModel)
            {
                var typingModel = viewModel.GetTypingModel();
                
                if (!typingModel.HasWordDelimiters)
                    return;
                
                if (textBox.SelectionStart <= typingModel.LastWordDelimiterPosition)
                {
                    int newSelectionStart = typingModel.LastWordDelimiterPosition + 1;
                    int newSelectionLength = Math.Max(0, textBox.SelectionLength - (newSelectionStart - textBox.SelectionStart));
                    
                    textBox.Select(newSelectionStart, newSelectionLength);
                }
            }
        }
    }
}
