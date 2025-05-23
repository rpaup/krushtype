using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using krushtype.Core.Models;
using krushtype.Core.Utilities.files;

namespace krushtype.UI.Views
{
    public partial class NewAuthWindow : Window
    {
        private bool isLoginPasswordVisible = false;
        private bool isRegisterPasswordVisible = false;
        
        // Color resources for password
        private SolidColorBrush weakBrush;
        private SolidColorBrush mediumBrush;
        private SolidColorBrush strongBrush;
        private SolidColorBrush veryStrongBrush;
        
        public bool IsLoginSuccessful { get; private set; } = false;
        
        public NewAuthWindow()
        {
            InitializeComponent();
            
            // Initialize the UserManager
            UserManager.Initialize();
            
            // Set the initial opacity for animation
            this.Opacity = 0;
            
            // Get colors for password strength indicators from resources
            weakBrush = new SolidColorBrush((Color)FindResource("LightErrorColor"));
            mediumBrush = new SolidColorBrush(Colors.Orange);
            strongBrush = new SolidColorBrush(Colors.YellowGreen);
            veryStrongBrush = new SolidColorBrush(Colors.Green);
            
            Loaded += (s, e) => {
                // Apply fade-in animation when window is loaded
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
                this.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
        }
        
        #region Window Controls
        
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        #endregion
        
        #region View Switching
        
        private void SwitchToRegister_Click(object sender, RoutedEventArgs e)
        {
            SwitchToRegisterView();
        }
        
        private void SwitchToLogin_Click(object sender, RoutedEventArgs e)
        {
            SwitchToLoginView();
        }
        
        private void SwitchToRegisterView()
        {
            // Slide login out of view
            ThicknessAnimation slideLoginOut = new ThicknessAnimation(
                new Thickness(0, 0, 0, 0),
                new Thickness(-450, 0, 0, 0),
                TimeSpan.FromSeconds(0.3)
            );
            slideLoginOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            LoginView.BeginAnimation(MarginProperty, slideLoginOut);
            
            // Slide register into view
            ThicknessAnimation slideRegisterIn = new ThicknessAnimation(
                new Thickness(450, 0, 0, 0),
                new Thickness(0, 0, 0, 0),
                TimeSpan.FromSeconds(0.3)
            );
            slideRegisterIn.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            RegisterView.BeginAnimation(MarginProperty, slideRegisterIn);
        }
        
        private void SwitchToLoginView()
        {
            // Slide register out of view
            ThicknessAnimation slideRegisterOut = new ThicknessAnimation(
                new Thickness(0, 0, 0, 0),
                new Thickness(450, 0, 0, 0),
                TimeSpan.FromSeconds(0.3)
            );
            slideRegisterOut.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            RegisterView.BeginAnimation(MarginProperty, slideRegisterOut);
            
            // Slide login into view
            ThicknessAnimation slideLoginIn = new ThicknessAnimation(
                new Thickness(-450, 0, 0, 0),
                new Thickness(0, 0, 0, 0),
                TimeSpan.FromSeconds(0.3)
            );
            slideLoginIn.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            LoginView.BeginAnimation(MarginProperty, slideLoginIn);
        }
        
        #endregion
        
        #region Password Visibility Toggle
        
        private void ToggleLoginPasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            isLoginPasswordVisible = !isLoginPasswordVisible;
            
            if (isLoginPasswordVisible)
            {
                LoginPasswordTextBox.Text = LoginPasswordBox.Password;
                LoginPasswordTextBox.Visibility = Visibility.Visible;
                LoginPasswordBox.Visibility = Visibility.Collapsed;
                LoginTogglePasswordButton.Content = "Hide";
            }
            else
            {
                LoginPasswordBox.Password = LoginPasswordTextBox.Text;
                LoginPasswordTextBox.Visibility = Visibility.Collapsed;
                LoginPasswordBox.Visibility = Visibility.Visible;
                LoginTogglePasswordButton.Content = "Show";
            }
        }
        
        private void ToggleRegisterPasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            isRegisterPasswordVisible = !isRegisterPasswordVisible;
            
            if (isRegisterPasswordVisible)
            {
                RegisterPasswordTextBox.Text = RegisterPasswordBox.Password;
                RegisterPasswordTextBox.Visibility = Visibility.Visible;
                RegisterPasswordBox.Visibility = Visibility.Collapsed;
                RegisterTogglePasswordButton.Content = "Hide";
            }
            else
            {
                RegisterPasswordBox.Password = RegisterPasswordTextBox.Text;
                RegisterPasswordTextBox.Visibility = Visibility.Collapsed;
                RegisterPasswordBox.Visibility = Visibility.Visible;
                RegisterTogglePasswordButton.Content = "Show";
            }
        }
        
        private void LoginPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isLoginPasswordVisible)
            {
                LoginPasswordTextBox.Text = LoginPasswordBox.Password;
            }
        }
        
        private void LoginPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoginPasswordVisible)
            {
                LoginPasswordBox.Password = LoginPasswordTextBox.Text;
            }
        }
        
        private void RegisterPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isRegisterPasswordVisible)
            {
                RegisterPasswordTextBox.Text = RegisterPasswordBox.Password;
            }
            
            // Update password strength
            UpdatePasswordStrength(RegisterPasswordBox.Password);
        }
        
        private void RegisterPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isRegisterPasswordVisible)
            {
                RegisterPasswordBox.Password = RegisterPasswordTextBox.Text;
                UpdatePasswordStrength(RegisterPasswordTextBox.Text);
            }
        }
        
        #endregion
        
        #region Password Strength
        
        private void UpdatePasswordStrength(string password)
        {
            // Reset all indicators to neutral color
            StrengthIndicator1.Background = (SolidColorBrush)FindResource("SubColorBrush");
            StrengthIndicator2.Background = (SolidColorBrush)FindResource("SubColorBrush");
            StrengthIndicator3.Background = (SolidColorBrush)FindResource("SubColorBrush");
            StrengthIndicator4.Background = (SolidColorBrush)FindResource("SubColorBrush");
            
            if (string.IsNullOrEmpty(password))
            {
                PasswordStrengthText.Text = "Password strength";
                PasswordStrengthText.Foreground = (SolidColorBrush)FindResource("SubColorBrush");
                return;
            }
            
            int score = 0;
            
            // Length check
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            
            // Complexity checks
            if (Regex.IsMatch(password, @"\d")) score++;
            if (Regex.IsMatch(password, @"[a-z]")) score++;
            if (Regex.IsMatch(password, @"[A-Z]")) score++;
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) score++;
            
            // Determine strength based on score
            if (score >= 6)
            {
                // Very Strong
                StrengthIndicator1.Background = veryStrongBrush;
                StrengthIndicator2.Background = veryStrongBrush;
                StrengthIndicator3.Background = veryStrongBrush;
                StrengthIndicator4.Background = veryStrongBrush;
                PasswordStrengthText.Text = "Very Strong";
                PasswordStrengthText.Foreground = veryStrongBrush;
            }
            else if (score >= 4)
            {
                // Strong
                StrengthIndicator1.Background = strongBrush;
                StrengthIndicator2.Background = strongBrush;
                StrengthIndicator3.Background = strongBrush;
                PasswordStrengthText.Text = "Strong";
                PasswordStrengthText.Foreground = strongBrush;
            }
            else if (score >= 3)
            {
                // Medium
                StrengthIndicator1.Background = mediumBrush;
                StrengthIndicator2.Background = mediumBrush;
                PasswordStrengthText.Text = "Medium";
                PasswordStrengthText.Foreground = mediumBrush;
            }
            else
            {
                // Weak
                StrengthIndicator1.Background = weakBrush;
                PasswordStrengthText.Text = "Weak";
                PasswordStrengthText.Foreground = weakBrush;
            }
        }
        
        #endregion
        
        #region Login Logic
        
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUsernameTextBox.Text.Trim();
            string password = LoginPasswordBox.Password;
            
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowLoginError("Please enter your username");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowLoginError("Please enter your password");
                return;
            }
            
            LoginButton.IsEnabled = false;
            LoginButton.Content = "Signing in...";
            
            await Task.Delay(300); // Short delay for UX
            
            bool success = UserManager.LoginUser(username, password);
            
            if (success)
            {
                IsLoginSuccessful = true;
                Close();
            }
            else
            {
                ShowLoginError("Invalid username or password");
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Sign In";
            }
        }
        
        private void ShowLoginError(string message)
        {
            LoginErrorMessage.Text = message;
            LoginErrorMessage.Opacity = 0;
            
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            LoginErrorMessage.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }
        
        #endregion
        
        #region Registration Logic
        
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegisterUsernameTextBox.Text.Trim();
            string password = RegisterPasswordBox.Password;
            
            // Validate input
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowRegisterError("Введите логин");
                isValid = false;
            }
            else if (username.Length < 3)
            {
                ShowRegisterError("Минимум 3 символа");
                isValid = false;
            }
            else if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                ShowRegisterError("Только латинские буквы, цифры и _");
                isValid = false;
            }
            else if (UserExists(username))
            {
                ShowRegisterError("Этот логин уже занят");
                isValid = false;
            }
            else
            {
                // Hide error if username is valid
                RegisterErrorMessage.Opacity = 0;
            }
            
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowRegisterError("Введите пароль");
                isValid = false;
            }
            else if (!IsPasswordStrong(password))
            {
                ShowRegisterError("Пароль не соответствует требованиям");
                isValid = false;
            }
            else
            {
                // Hide error if password is valid
                RegisterErrorMessage.Opacity = 0;
            }
            
            if (!isValid) return;
            
            RegisterButton.IsEnabled = false;
            RegisterButton.Content = "Создание аккаунта...";
            
            await Task.Delay(300); // Short delay for UX
            
            bool success = UserManager.RegisterUser(username, password);
            
            if (success)
            {
                IsLoginSuccessful = true;
                Close();
            }
            else
            {
                ShowRegisterError("Ошибка при регистрации. Пожалуйста, попробуйте снова.");
                RegisterButton.IsEnabled = true;
                RegisterButton.Content = "Создать аккаунт";
            }
        }
        
        private void ShowRegisterError(string message)
        {
            RegisterErrorMessage.Text = message;
            RegisterErrorMessage.Opacity = 0;
            
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            RegisterErrorMessage.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool UserExists(string username)
        {
            var users = UserManager.GetAllUsers();
            return users.Exists(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool IsPasswordStrong(string password)
        {
            if (password.Length < 8)
                return false;
                
            // At least one digit
            if (!Regex.IsMatch(password, @"\d"))
                return false;
                
            // At least one uppercase
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;
                
            // At least one special character
            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                return false;
                
            return true;
        }
        
        #endregion
    }
}
