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
using krushtype.Core.ViewModels;
using System.ComponentModel;

namespace krushtype
{
    public partial class MainWindow : Window
    {
        private navigationVM _navVM;
        private typingVM _typingVM;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContextChanged += MainWindow_DataContextChanged;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_navVM != null)
            {
                _navVM.PropertyChanged -= NavigationVM_PropertyChanged;
            }

            _navVM = DataContext as navigationVM;

            if (_navVM != null)
            {
                _navVM.PropertyChanged += NavigationVM_PropertyChanged;
                HandleCurrentViewChange(_navVM.CurrentView);
            }
            else
            {
                HandleCurrentViewChange(null);
            }
        }

        private void NavigationVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(navigationVM.CurrentView))
            {
                HandleCurrentViewChange(_navVM.CurrentView);
            }
        }

        private void HandleCurrentViewChange(object newView)
        {
            if (_typingVM != null)
            {
                _typingVM.TestStateChanged -= TypingVM_TestStateChanged;
                _typingVM = null;
                UpdateResizeMode(false); 
            }

            if (newView is typingVM currentTypingVM)
            {
                _typingVM = currentTypingVM;
                _typingVM.TestStateChanged += TypingVM_TestStateChanged;
                UpdateResizeMode(_typingVM.IsTestStarted);
            }
            else
            {
                UpdateResizeMode(false);
            }
        }

        private void TypingVM_TestStateChanged(bool isTestStarted)
        {
            UpdateResizeMode(isTestStarted);
        }

        private void UpdateResizeMode(bool isTestInProgress)
        {
            if (isTestInProgress)
            {
                this.ResizeMode = ResizeMode.NoResize;
                
                if (NavigationPanel != null)
                {
                    NavigationPanel.Visibility = Visibility.Collapsed;
                }
                if (ThemeTogglePanel != null)
                {
                    ThemeTogglePanel.Visibility = Visibility.Collapsed;
                }
                if (LogoText != null)
                {
                    LogoText.Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)); 
                }
                if (LogoImage != null)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri("pack://application:,,,/Assets/images/licong.png", UriKind.Absolute);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    LogoImage.Source = bitmapImage;
                }
            }
            else
            {
                this.ResizeMode = ResizeMode.CanResizeWithGrip;
                
                if (NavigationPanel != null)
                {
                    NavigationPanel.Visibility = Visibility.Visible;
                }
                if (ThemeTogglePanel != null)
                {
                    ThemeTogglePanel.Visibility = Visibility.Visible;
                }
                if (LogoText != null)
                {
                    LogoText.SetResourceReference(TextBlock.ForegroundProperty, "MainColorBrush");
                }
                if (LogoImage != null)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri("pack://application:,,,/Assets/images/licon.png", UriKind.Absolute);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    LogoImage.Source = bitmapImage;
                }
            }
        }
    }
}
