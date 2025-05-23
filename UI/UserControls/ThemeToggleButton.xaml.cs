using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using krushtype.Core;

namespace krushtype.UI.UserControls
{
    public partial class ThemeToggleButton : UserControl
    {
        private bool _isChecked = false;
        private bool _isInitialized = false;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                UpdateToggleState(false);
            }
        }
        
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ThemeToggleButton), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public ThemeToggleButton()
        {
            InitializeComponent();
            
            Loaded += ThemeToggleButton_Loaded;
            SwitchTrack.SizeChanged += SwitchTrack_SizeChanged;
            
            Unloaded += (s, e) => 
            {
                ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
                SwitchTrack.SizeChanged -= SwitchTrack_SizeChanged;
            };
        }

        private void SwitchTrack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isInitialized)
            {
                UpdateToggleState(false);
            }
        }

        private void ThemeToggleButton_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isChecked = ThemeManager.CurrentTheme == ThemeType.Dark;
                
                ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
                
                _isInitialized = true;
                
                UpdateToggleState(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing theme toggle: {ex.Message}", 
                    "Theme Toggle Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            _isChecked = ThemeManager.CurrentTheme == ThemeType.Dark;
            
            Dispatcher.Invoke(() => 
            {
                UpdateToggleState(true);
            });
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            _isChecked = !_isChecked;
            UpdateToggleState(true);
            
            ThemeManager.ApplyTheme(_isChecked ? ThemeType.Dark : ThemeType.Light);
            
            if (Command != null && Command.CanExecute(null))
            {
                Command.Execute(null);
            }
        }

        private void UpdateToggleState(bool animate)
        {
            if (!IsLoaded || SwitchTrack.ActualWidth == 0 || SwitchThumb.ActualWidth == 0)
                return;
                
            double leftPosition = 2;
            double rightPosition = SwitchTrack.ActualWidth - SwitchThumb.ActualWidth - 2;
            
            if (rightPosition <= leftPosition)
            {
                leftPosition = 2;
                rightPosition = 26; 
            }
            
            double targetPosition = _isChecked ? rightPosition : leftPosition;
            
            if (animate)
            {
                ThicknessAnimation marginAnimation = new ThicknessAnimation
                {
                    To = new Thickness(targetPosition, 0, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(150),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                
                SwitchThumb.BeginAnimation(Border.MarginProperty, marginAnimation);
                
                DoubleAnimation sunOpacityAnimation = new DoubleAnimation
                {
                    To = _isChecked ? 0 : 1,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                
                DoubleAnimation moonOpacityAnimation = new DoubleAnimation
                {
                    To = _isChecked ? 1 : 0,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                
                SunIcon.BeginAnimation(OpacityProperty, sunOpacityAnimation);
                MoonIcon.BeginAnimation(OpacityProperty, moonOpacityAnimation);
            }
            else
            {
                SwitchThumb.BeginAnimation(Border.MarginProperty, null);
                SunIcon.BeginAnimation(OpacityProperty, null);
                MoonIcon.BeginAnimation(OpacityProperty, null);
                
                SwitchThumb.Margin = new Thickness(targetPosition, 0, 0, 0);
                SunIcon.Opacity = _isChecked ? 0 : 1;
                MoonIcon.Opacity = _isChecked ? 1 : 0;
            }
        }
    }
} 