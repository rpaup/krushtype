using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace krushtype.Core
{
    public enum ThemeType
    {
        Light,
        Dark
    }

    public class ThemeManager
    {
        private const string LightThemeSource = "pack://application:,,,/krushtype;component/UI/Styles/LightTheme.xaml";
        private const string DarkThemeSource = "pack://application:,,,/krushtype;component/UI/Styles/DarkTheme.xaml";

        private static ThemeType _currentTheme = ThemeType.Light;

        public static ThemeType CurrentTheme
        {
            get { return _currentTheme; }
        }

        public static event EventHandler ThemeChanged;

        private static bool ResourceExists(string resourcePath)
        {
            try
            {
                Uri resourceUri = new Uri(resourcePath, UriKind.Absolute);
                var stream = Application.GetResourceStream(resourceUri);
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Resource not found: {resourcePath}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking resource {resourcePath}: {ex.Message}");
                return false;
            }
        }

        private static bool ResourceDictionaryExists(string path)
        {
            try
            {
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri(path, UriKind.Absolute);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void UpdateFallbackResources(ResourceDictionary resources)
        {
            if (!resources.Contains("BgColorBrush"))
                resources["BgColorBrush"] = new SolidColorBrush(Colors.White);
                
            if (!resources.Contains("TextColorBrush"))
                resources["TextColorBrush"] = new SolidColorBrush(Colors.Black);
                
            if (!resources.Contains("MainColorBrush"))
                resources["MainColorBrush"] = new SolidColorBrush(Colors.Blue);
                
            if (!resources.Contains("SubColorBrush"))
                resources["SubColorBrush"] = new SolidColorBrush(Colors.Gray);
                
            if (!resources.Contains("SubAltColorBrush"))
                resources["SubAltColorBrush"] = new SolidColorBrush(Colors.LightGray);
                
            if (!resources.Contains("CaretColorBrush"))
                resources["CaretColorBrush"] = new SolidColorBrush(Colors.Black);
                
            if (!resources.Contains("ErrorColorBrush"))
                resources["ErrorColorBrush"] = new SolidColorBrush(Colors.Red);
                
            if (!resources.Contains("ErrorExtraColorBrush"))
                resources["ErrorExtraColorBrush"] = new SolidColorBrush(Colors.DarkRed);
                
            if (!resources.Contains("WindowButtonHoverBrush"))
                resources["WindowButtonHoverBrush"] = new SolidColorBrush(Colors.LightGray);
                
            if (!resources.Contains("WindowButtonPressedBrush"))
                resources["WindowButtonPressedBrush"] = new SolidColorBrush(Colors.Gray);
        }

        public static void Initialize()
        {
            bool lightThemeExists = ResourceExists(LightThemeSource);
            bool darkThemeExists = ResourceExists(DarkThemeSource);

            if (!lightThemeExists || !darkThemeExists)
            {
                string missingResources = "";
                if (!lightThemeExists) missingResources += LightThemeSource + " ";
                if (!darkThemeExists) missingResources += DarkThemeSource;
                
                MessageBox.Show($"Missing theme resources: {missingResources}", 
                    "Resource Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                int savedTheme = Properties.Settings.Default.ThemeType;
                ThemeType themeToApply = savedTheme == 1 ? ThemeType.Dark : ThemeType.Light;
                ApplyTheme(themeToApply);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing theme: {ex.Message}", "Theme Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ApplyTheme(ThemeType.Light);
            }
        }

        public static void ApplyTheme(ThemeType theme)
        {
            try
            {
                string themePath = theme == ThemeType.Light ? LightThemeSource : DarkThemeSource;
                
                if (!ResourceDictionaryExists(themePath))
                {
                    MessageBox.Show($"Could not load theme from {themePath}. Verify that the theme file exists and is properly formatted.", 
                        "Theme Resource Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                var oldTheme = _currentTheme;
                _currentTheme = theme;

                Properties.Settings.Default.ThemeType = (int)theme;
                Properties.Settings.Default.Save();

                var mergedDicts = Application.Current.Resources.MergedDictionaries;

                for (int i = mergedDicts.Count - 1; i >= 0; i--)
                {
                    var resourceDict = mergedDicts[i];
                    string source = resourceDict.Source?.ToString() ?? string.Empty;
                    if (source.Contains("LightTheme.xaml") || source.Contains("DarkTheme.xaml"))
                    {
                        mergedDicts.RemoveAt(i);
                    }
                }

                ResourceDictionary themeDict = new ResourceDictionary();
                try
                {
                    themeDict.Source = new Uri(themePath, UriKind.Absolute);
                    
                    UpdateFallbackResources(themeDict);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not load theme from {themePath}: {ex.Message}", 
                        "Theme Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                mergedDicts.Add(themeDict);
                
                foreach (Window window in Application.Current.Windows)
                {
                    window.InvalidateVisual();
                    RefreshAllControls(window);
                }

                if (oldTheme != _currentTheme)
                {
                    ThemeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying theme: {ex.Message}", "Theme Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private static void RefreshAllControls(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement element)
                {
                    element.InvalidateVisual();
                    
                    if (element is Control control && control.Template != null)
                    {
                        control.ApplyTemplate();
                    }
                }
                
                RefreshAllControls(child);
            }
        }

        public static void ToggleTheme()
        {
            ThemeType newTheme = _currentTheme == ThemeType.Light ? ThemeType.Dark : ThemeType.Light;
            ApplyTheme(newTheme);
        }
    }
} 