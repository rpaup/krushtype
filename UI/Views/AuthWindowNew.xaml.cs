using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using krushtype.Core;
using krushtype.Core.Models;
using krushtype.Core.Utilities.files;

namespace krushtype.UI.Views
{
    public partial class AuthWindowNew : Window
    {
        private bool isLoginPasswordVisible = false;
        private bool isRegisterPasswordVisible = false;
        
        private SolidColorBrush weakBrush;
        private SolidColorBrush mediumBrush;
        private SolidColorBrush strongBrush;
        private SolidColorBrush veryStrongBrush;
        
        public bool IsAuthSuccessful { get; private set; } = false;
        public User LoggedInUser { get; private set; } = null;
        
        public AuthWindowNew()
        {
            InitializeComponent();
            
            UserManager.Initialize();
            
            this.Opacity = 0;
            
            ApplyCurrentTheme();
            
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            
            Loaded += (s, e) => {
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
                this.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
        }
        
        private void ApplyCurrentTheme()
        {
            if (Application.Current != null)
            {
                foreach (ResourceDictionary appDict in Application.Current.Resources.MergedDictionaries)
                {
                    string source = appDict.Source?.ToString() ?? string.Empty;
                    
                    if (source.Contains("LightTheme.xaml") || source.Contains("DarkTheme.xaml")) 
                    {
                        bool alreadyExists = false;
                        foreach (ResourceDictionary existingDict in this.Resources.MergedDictionaries)
                        {
                            if (existingDict.Source == appDict.Source)
                            {
                                alreadyExists = true;
                                break;
                            }
                        }
                        
                        if (!alreadyExists)
                        {
                            this.Resources.MergedDictionaries.Add(appDict);
                        }
                    }
                }
            }
            
            weakBrush = new SolidColorBrush((Color)FindResource("LightErrorColor"));
            mediumBrush = new SolidColorBrush(Colors.Orange);
            strongBrush = new SolidColorBrush(Colors.YellowGreen);
            veryStrongBrush = new SolidColorBrush(Colors.Green);
            
            this.InvalidateVisual();
        }
        
        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyCurrentTheme();
            
            this.InvalidateVisual();
        }
        
        protected override void OnClosed(EventArgs e)
        {
            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            base.OnClosed(e);
        }
        
        #region Window Controls
        
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        #endregion
        
        #region View Switching
        
        private void BtnShowRegister_Click(object sender, RoutedEventArgs e)
        {
            AnimateViewChange(loginView, registerView);
        }
        
        private void BtnShowLogin_Click(object sender, RoutedEventArgs e)
        {
            AnimateViewChange(registerView, loginView);
        }
        
        private void AnimateViewChange(UIElement currentView, UIElement newView)
        {
            DoubleAnimation fadeOut = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2)
            };
            
            fadeOut.Completed += (s, e) => {
                currentView.Visibility = Visibility.Collapsed;
                
                newView.Opacity = 0;
                newView.Visibility = Visibility.Visible;
                
                DoubleAnimation fadeIn = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                
                newView.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            
            currentView.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        
        #endregion
        
        #region Password Visibility Toggle
        
        private void BtnToggleLoginPassword_Click(object sender, RoutedEventArgs e)
        {
            isLoginPasswordVisible = !isLoginPasswordVisible;
            
            if (isLoginPasswordVisible)
            {
                txtLoginPasswordVisible.Text = pwdLoginPassword.Password;
                txtLoginPasswordVisible.Visibility = Visibility.Visible;
                pwdLoginPassword.Visibility = Visibility.Collapsed;
                btnToggleLoginPassword.Content = "hide";
            }
            else
            {
                pwdLoginPassword.Password = txtLoginPasswordVisible.Text;
                txtLoginPasswordVisible.Visibility = Visibility.Collapsed;
                pwdLoginPassword.Visibility = Visibility.Visible;
                btnToggleLoginPassword.Content = "show";
            }
        }
        
        private void BtnToggleRegisterPassword_Click(object sender, RoutedEventArgs e)
        {
            isRegisterPasswordVisible = !isRegisterPasswordVisible;
            
            if (isRegisterPasswordVisible)
            {
                txtRegisterPasswordVisible.Text = pwdRegisterPassword.Password;
                txtRegisterPasswordVisible.Visibility = Visibility.Visible;
                pwdRegisterPassword.Visibility = Visibility.Collapsed;
                btnToggleRegisterPassword.Content = "hide";
            }
            else
            {
                pwdRegisterPassword.Password = txtRegisterPasswordVisible.Text;
                txtRegisterPasswordVisible.Visibility = Visibility.Collapsed;
                pwdRegisterPassword.Visibility = Visibility.Visible;
                btnToggleRegisterPassword.Content = "show";
            }
        }
        
        private void PwdLoginPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isLoginPasswordVisible)
            {
                txtLoginPasswordVisible.Text = pwdLoginPassword.Password;
            }
            
            ValidateLoginPassword(pwdLoginPassword.Password);
        }
        
        private void TxtLoginPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoginPasswordVisible)
            {
                pwdLoginPassword.Password = txtLoginPasswordVisible.Text;
                
                ValidateLoginPassword(txtLoginPasswordVisible.Text);
            }
        }
        
        private void ValidateLoginPassword(string password)
        {
            txtLoginError.Visibility = Visibility.Collapsed;
            txtLoginPasswordError.Visibility = Visibility.Collapsed;
        }
        
        private void PwdRegisterPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isRegisterPasswordVisible)
            {
                txtRegisterPasswordVisible.Text = pwdRegisterPassword.Password;
            }
            
            UpdatePasswordStrength(pwdRegisterPassword.Password);
            
            ValidateRegisterPassword(pwdRegisterPassword.Password);
        }
        
        private void TxtRegisterPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isRegisterPasswordVisible)
            {
                pwdRegisterPassword.Password = txtRegisterPasswordVisible.Text;
                UpdatePasswordStrength(txtRegisterPasswordVisible.Text);
                
                ValidateRegisterPassword(txtRegisterPasswordVisible.Text);
            }
        }
        
        private void ValidateRegisterPassword(string password)
        {
            txtRegisterError.Visibility = Visibility.Collapsed;
            txtRegisterPasswordError.Visibility = Visibility.Collapsed;
        }
        
        #endregion
        
        #region Form Field Changes
        
        private void TxtLoginUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtLoginError.Visibility = Visibility.Collapsed;
            txtLoginUsernameError.Visibility = Visibility.Collapsed;
        }
        
        private void TxtRegisterUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtRegisterError.Visibility = Visibility.Collapsed;
            txtRegisterUsernameError.Visibility = Visibility.Collapsed;
        }
        
        #endregion
        
        #region Password Strength
        
        private void UpdatePasswordStrength(string password)
        {
            strengthIndicator1.Background = (SolidColorBrush)FindResource("SubColorBrush");
            strengthIndicator2.Background = (SolidColorBrush)FindResource("SubColorBrush");
            strengthIndicator3.Background = (SolidColorBrush)FindResource("SubColorBrush");
            strengthIndicator4.Background = (SolidColorBrush)FindResource("SubColorBrush");
            
            bool hasMinLength = password.Length >= 8;
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasSpecial = Regex.IsMatch(password, @"[^a-zA-Z0-9]");
            
            iconLength.Text = hasMinLength ? "✓" : "○";
            iconDigit.Text = hasDigit ? "✓" : "○";
            iconUpper.Text = hasUpper ? "✓" : "○"; 
            iconSpecial.Text = hasSpecial ? "✓" : "○";
            
            reqLength.Foreground = hasMinLength ? (SolidColorBrush)FindResource("MainColorBrush") : (SolidColorBrush)FindResource("SubColorBrush");
            reqDigit.Foreground = hasDigit ? (SolidColorBrush)FindResource("MainColorBrush") : (SolidColorBrush)FindResource("SubColorBrush");
            reqUpper.Foreground = hasUpper ? (SolidColorBrush)FindResource("MainColorBrush") : (SolidColorBrush)FindResource("SubColorBrush");
            reqSpecial.Foreground = hasSpecial ? (SolidColorBrush)FindResource("MainColorBrush") : (SolidColorBrush)FindResource("SubColorBrush");
            
            if (string.IsNullOrEmpty(password))
            {
                txtPasswordStrength.Text = "password strength";
                txtPasswordStrength.Foreground = (SolidColorBrush)FindResource("SubColorBrush");
                return;
            }
            
            int score = 0;
            
            if (hasMinLength) score++;
            if (password.Length >= 12) score++;
            
            if (hasDigit) score++;
            if (Regex.IsMatch(password, @"[a-z]")) score++;
            if (hasUpper) score++;
            if (hasSpecial) score++;
            
            if (score >= 6)
            {
                strengthIndicator1.Background = veryStrongBrush;
                strengthIndicator2.Background = veryStrongBrush;
                strengthIndicator3.Background = veryStrongBrush;
                strengthIndicator4.Background = veryStrongBrush;
                txtPasswordStrength.Text = "very strong";
                txtPasswordStrength.Foreground = veryStrongBrush;
            }
            else if (score >= 4)
            {
                strengthIndicator1.Background = strongBrush;
                strengthIndicator2.Background = strongBrush;
                strengthIndicator3.Background = strongBrush;
                strengthIndicator4.Background = (SolidColorBrush)FindResource("SubColorBrush");
                txtPasswordStrength.Text = "strong";
                txtPasswordStrength.Foreground = strongBrush;
            }
            else if (score >= 3)
            {
                strengthIndicator1.Background = mediumBrush;
                strengthIndicator2.Background = mediumBrush;
                strengthIndicator3.Background = (SolidColorBrush)FindResource("SubColorBrush");
                strengthIndicator4.Background = (SolidColorBrush)FindResource("SubColorBrush");
                txtPasswordStrength.Text = "medium";
                txtPasswordStrength.Foreground = mediumBrush;
            }
            else if (score >= 1)
            {
                strengthIndicator1.Background = weakBrush;
                strengthIndicator2.Background = (SolidColorBrush)FindResource("SubColorBrush");
                strengthIndicator3.Background = (SolidColorBrush)FindResource("SubColorBrush");
                strengthIndicator4.Background = (SolidColorBrush)FindResource("SubColorBrush");
                txtPasswordStrength.Text = "weak";
                txtPasswordStrength.Foreground = weakBrush;
            }
            else
            {
                txtPasswordStrength.Text = "password too short";
                txtPasswordStrength.Foreground = weakBrush;
            }
        }
        
        #endregion
        
        #region Login Logic
        
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtLoginUsername.Text.Trim();
            string password = pwdLoginPassword.Password;
            
            txtLoginError.Visibility = Visibility.Collapsed;
            txtLoginUsernameError.Visibility = Visibility.Collapsed;
            txtLoginPasswordError.Visibility = Visibility.Collapsed;
            
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(username))
            {
                txtLoginUsernameError.Text = "enter username";
                txtLoginUsernameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                txtLoginUsernameError.Text = "only latin, numbers and _";
                txtLoginUsernameError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                txtLoginPasswordError.Text = "enter password";
                txtLoginPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            
            if (!isValid) return;
            
            btnLogin.IsEnabled = false;
            btnLogin.Content = "logging in...";
            
            await Task.Delay(300);
            
            try
            {
                bool success = UserManager.LoginUser(username, password);
                
                if (success)
                {
                    IsAuthSuccessful = true;
                    LoggedInUser = UserManager.CurrentUser;
                    
                    DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
                    fadeOut.Completed += (s, ev) => Close();
                    this.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
                else
                {
                    txtLoginError.Text = "invalid username or password";
                    txtLoginError.Visibility = Visibility.Visible;
                    btnLogin.IsEnabled = true;
                    btnLogin.Content = "login";
                }
            }
            catch (Exception ex)
            {
                txtLoginError.Text = $"login error: {ex.Message}";
                txtLoginError.Visibility = Visibility.Visible;
                btnLogin.IsEnabled = true;
                btnLogin.Content = "login";
            }
        }
        
        #endregion
        
        #region Registration Logic
        
        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtRegisterUsername.Text.Trim();
            string password = pwdRegisterPassword.Password;
            
            txtRegisterError.Visibility = Visibility.Collapsed;
            txtRegisterUsernameError.Visibility = Visibility.Collapsed;
            txtRegisterPasswordError.Visibility = Visibility.Collapsed;
            
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(username))
            {
                txtRegisterUsernameError.Text = "enter username";
                txtRegisterUsernameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (username.Length < 3)
            {
                txtRegisterUsernameError.Text = "minimum 3 characters";
                txtRegisterUsernameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                txtRegisterUsernameError.Text = "only latin, numbers and _";
                txtRegisterUsernameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (UserExists(username))
            {
                txtRegisterUsernameError.Text = "username already taken";
                txtRegisterUsernameError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                txtRegisterPasswordError.Text = "enter password";
                txtRegisterPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (password.Length < 8)
            {
                txtRegisterPasswordError.Text = "minimum 8 characters";
                txtRegisterPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Regex.IsMatch(password, @"\d"))
            {
                txtRegisterPasswordError.Text = "need at least 1 digit";
                txtRegisterPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                txtRegisterPasswordError.Text = "need at least 1 uppercase letter";
                txtRegisterPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                txtRegisterPasswordError.Text = "need at least 1 special character";
                txtRegisterPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            
            if (!isValid) return;
            
            btnRegister.IsEnabled = false;
            btnRegister.Content = "registering...";
            
            await Task.Delay(300);
            
            try
            {
                bool success = UserManager.RegisterUser(username, password);
                
                if (success)
                {
                    IsAuthSuccessful = true;
                    LoggedInUser = UserManager.CurrentUser;
                    
                    DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
                    fadeOut.Completed += (s, ev) => Close();
                    this.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
                else
                {
                    txtRegisterError.Text = "registration error. please try again later.";
                    txtRegisterError.Visibility = Visibility.Visible;
                    btnRegister.IsEnabled = true;
                    btnRegister.Content = "register";
                }
            }
            catch (Exception ex)
            {
                txtRegisterError.Text = $"registration error: {ex.Message}";
                txtRegisterError.Visibility = Visibility.Visible;
                btnRegister.IsEnabled = true;
                btnRegister.Content = "register";
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool UserExists(string username)
        {
            var users = UserManager.GetAllUsers();
            return users.Exists(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        
        #endregion
    }
}
