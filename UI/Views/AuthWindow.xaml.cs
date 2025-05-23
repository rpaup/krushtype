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
    /// <summary>
    /// Authentication window for login and registration
    /// </summary>
    public partial class AuthWindow : Window
    {
        private bool isLoginExistsErrorShown = false;
        private bool isRegisterEmailInvalid = false;
        private bool isRegisterPasswordValid = false;
        private bool isRegisterUsernameValid = false;
        private bool isPasswordVisible = false;
        private bool isRegisterPasswordVisible = false;

        // Color definitions
        private SolidColorBrush successColor;
        private SolidColorBrush errorColor;
        private SolidColorBrush neutralColor;

        public bool IsLoginSuccessful { get; private set; }

        public AuthWindow()
        {
            InitializeComponent();

            // Initialize the UserManager
            UserManager.Initialize();
            
            // Set the initial opacity for animation
            this.Opacity = 0;

            // Set color brushes based on theme
            successColor = (SolidColorBrush)Resources["SuccessColorBrush"] ?? new SolidColorBrush(Colors.Green);
            errorColor = (SolidColorBrush)Resources["ErrorColorBrush"] ?? new SolidColorBrush(Colors.Red);
            neutralColor = (SolidColorBrush)Resources["SubColorBrush"] ?? new SolidColorBrush(Colors.Gray);

            Loaded += (s, e) => {
                AnimateWindowAppearance();
            };
        }

        #region Animation Methods

        private void AnimateWindowAppearance()
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            this.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            
            // Animate elements in the active view
            if (loginView.Visibility == Visibility.Visible)
            {
                AnimateElementAppearance(loginHeaderTitle, 0);
                AnimateElementAppearance(windowControls, 0.05);
                AnimateElementAppearance(loginUsernamePanel, 0.1);
                AnimateElementAppearance(loginPasswordPanel, 0.2);
                AnimateElementAppearance(loginButtonsPanel, 0.3);
            }
            else
            {
                AnimateElementAppearance(registerHeaderTitle, 0);
                AnimateElementAppearance(windowControls, 0.05);
                AnimateElementAppearance(registerUsernamePanel, 0.1);
                AnimateElementAppearance(registerEmailPanel, 0.15);
                AnimateElementAppearance(registerPasswordPanel, 0.2);
                AnimateElementAppearance(requirementsPanel, 0.25);
                AnimateElementAppearance(registerButtonsPanel, 0.3);
            }
        }
        
        private void AnimateElementAppearance(FrameworkElement element, double delay)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                BeginTime = TimeSpan.FromSeconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            DoubleAnimation translateAnimation = new DoubleAnimation
            {
                From = element.RenderTransform is TranslateTransform transform ? transform.Y : -15,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                BeginTime = TimeSpan.FromSeconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            
            if (element.RenderTransform is TranslateTransform existingTransform)
            {
                existingTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
            }
            else
            {
                TranslateTransform translateTransform = new TranslateTransform();
                element.RenderTransform = translateTransform;
                translateTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
            }
        }

        private void AnimateFadeIn(UIElement element)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            element.Visibility = Visibility.Visible;
            element.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
        }

        private void AnimateFadeOut(UIElement element)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            fadeAnimation.Completed += (s, e) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
        }

        private void AnimateButtonPress(Button button)
        {
            DoubleAnimation scaleXAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.95,
                Duration = TimeSpan.FromSeconds(0.1),
                AutoReverse = true,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            DoubleAnimation scaleYAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.95,
                Duration = TimeSpan.FromSeconds(0.1),
                AutoReverse = true,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            
            ScaleTransform transform = button.RenderTransform as ScaleTransform;
            
            if (transform == null)
            {
                transform = new ScaleTransform(1, 1);
                button.RenderTransform = transform;
                button.RenderTransformOrigin = new Point(0.5, 0.5);
            }
            
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
        }

        private void SwitchView(bool showLogin)
        {
            if (showLogin)
            {
                loginView.Visibility = Visibility.Visible;
                registerView.Visibility = Visibility.Collapsed;
                AnimateElementAppearance(loginHeaderTitle, 0);
                AnimateElementAppearance(loginUsernamePanel, 0.1);
                AnimateElementAppearance(loginPasswordPanel, 0.2);
                AnimateElementAppearance(loginButtonsPanel, 0.3);
            }
            else
            {
                loginView.Visibility = Visibility.Collapsed;
                registerView.Visibility = Visibility.Visible;
                AnimateElementAppearance(registerHeaderTitle, 0);
                AnimateElementAppearance(registerUsernamePanel, 0.1);
                AnimateElementAppearance(registerEmailPanel, 0.15);
                AnimateElementAppearance(registerPasswordPanel, 0.2);
                AnimateElementAppearance(requirementsPanel, 0.25);
                AnimateElementAppearance(registerButtonsPanel, 0.3);
            }
        }

        #endregion

        #region Login Logic

        private void txtLoginUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Could implement additional validation if needed
            HideLoginError();
        }

        private void txtLoginPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isPasswordVisible)
            {
                txtLoginPasswordVisible.Text = txtLoginPassword.Password;
            }
            HideLoginError();
        }

        private void txtLoginPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPasswordVisible)
            {
                txtLoginPassword.Password = txtLoginPasswordVisible.Text;
                HideLoginError();
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(btnLogin);
            
            string username = txtLoginUsername.Text.Trim();
            string password = txtLoginPassword.Password;

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

            // Simulate loading/processing
            btnLogin.IsEnabled = false;
            btnLogin.Content = "Signing in...";
            
            await Task.Delay(500); // Artificially delay for UI feedback

            bool success = UserManager.LoginUser(username, password);
            
            if (success)
            {
                IsLoginSuccessful = true;
                this.Close();
            }
            else
            {
                ShowLoginError("Invalid username or password");
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Sign In";
            }
        }

        private void ShowLoginError(string message)
        {
            txtLoginStatus.Text = message;
            loginStatusContainer.BorderBrush = errorColor;
            txtLoginStatus.Foreground = errorColor;
            loginStatusContainer.Visibility = Visibility.Visible;
            AnimateFadeIn(loginStatusContainer);
        }

        private void HideLoginError()
        {
            if (loginStatusContainer.Visibility == Visibility.Visible)
            {
                AnimateFadeOut(loginStatusContainer);
            }
        }

        #endregion

        #region Registration Logic

        private void txtRegisterUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            string username = txtRegisterUsername.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(username))
            {
                isRegisterUsernameValid = false;
            }
            else if (username.Length < 3)
            {                isRegisterUsernameValid = false;
                ShowRegisterError("Username must be at least 3 characters long");
            }
            else if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                isRegisterUsernameValid = false;
                ShowRegisterError("Username can only contain letters, numbers, and underscores");
            }
            else if (UserExists(username))
            {
                isRegisterUsernameValid = false;
                isLoginExistsErrorShown = true;
                ShowRegisterError("This username is already taken");
            }
            else
            {
                isRegisterUsernameValid = true;
                isLoginExistsErrorShown = false;
                HideRegisterError();
            }
        }

        private void txtRegisterEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            string email = txtRegisterEmail.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(email))
            {
                isRegisterEmailInvalid = false;
            }            else if (!IsValidEmail(email))
            {
                isRegisterEmailInvalid = true;
                ShowRegisterError("Please enter a valid email address");
            }
            else
            {
                isRegisterEmailInvalid = false;
                HideRegisterError();
            }
        }

        private void txtRegisterPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = txtRegisterPassword.Password;
            
            if (isRegisterPasswordVisible)
            {
                txtRegisterPasswordVisible.Text = password;
            }

            UpdatePasswordRequirements(password);
            
            if (string.IsNullOrWhiteSpace(password))
            {
                isRegisterPasswordValid = false;
            }
            else if (!IsPasswordStrong(password))
            {
                isRegisterPasswordValid = false;
                ShowRegisterError("Password doesn't meet the security requirements");
            }
            else
            {
                isRegisterPasswordValid = true;
                HideRegisterError();
            }
        }

        private void txtRegisterPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isRegisterPasswordVisible)
            {
                txtRegisterPassword.Password = txtRegisterPasswordVisible.Text;
                UpdatePasswordRequirements(txtRegisterPasswordVisible.Text);
            }
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtRegisterUsername.Text.Trim();
            string password = txtRegisterPassword.Password;

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
                HideRegisterError();
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
                HideRegisterError();
            }

            if (!isValid) return;

            // Simulate loading/processing
            btnRegister.IsEnabled = false;
            btnRegister.Content = "Создание аккаунта...";
            
            await Task.Delay(500); // Artificially delay for UI feedback
            
            bool success = UserManager.RegisterUser(username, password);
            
            if (success)
            {
                IsLoginSuccessful = true;
                this.Close();
            }
            else
            {
                ShowRegisterError("Ошибка при регистрации. Возможно, пользователь с таким именем уже существует.");
                btnRegister.IsEnabled = true;
                btnRegister.Content = "Создать аккаунт";
            }
        }

        private void ShowRegisterError(string message)
        {
            txtRegisterStatus.Text = message;
            registerStatusContainer.BorderBrush = errorColor;
            txtRegisterStatus.Foreground = errorColor;
            registerStatusContainer.Visibility = Visibility.Visible;
            AnimateFadeIn(registerStatusContainer);
        }

        private void HideRegisterError()
        {
            // Only hide errors if there are no other validation errors
            if (!isLoginExistsErrorShown && registerStatusContainer.Visibility == Visibility.Visible)
            {
                AnimateFadeOut(registerStatusContainer);
            }
        }

        private void ShowRegisterSuccess(string message)
        {
            txtRegisterStatus.Text = message;
            registerStatusContainer.BorderBrush = successColor;
            txtRegisterStatus.Foreground = successColor;
            registerStatusContainer.Visibility = Visibility.Visible;
            AnimateFadeIn(registerStatusContainer);
        }

        private void UpdatePasswordRequirements(string password)
        {
            // Length requirement
            if (password.Length >= 8)
            {
                iconLength.Text = "✅";
                reqLength.Foreground = successColor;
            }
            else
            {
                iconLength.Text = "❌";
                reqLength.Foreground = neutralColor;
            }

            // Digit requirement
            if (Regex.IsMatch(password, @"\d"))
            {
                iconDigit.Text = "✅";
                reqDigit.Foreground = successColor;
            }
            else
            {
                iconDigit.Text = "❌";
                reqDigit.Foreground = neutralColor;
            }

            // Uppercase requirement
            if (Regex.IsMatch(password, @"[A-Z]"))
            {
                iconUpper.Text = "✅";
                reqUpper.Foreground = successColor;
            }
            else
            {
                iconUpper.Text = "❌";
                reqUpper.Foreground = neutralColor;
            }

            // Special character requirement
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                iconSpecial.Text = "✅";
                reqSpecial.Foreground = successColor;
            }
            else
            {
                iconSpecial.Text = "❌";
                reqSpecial.Foreground = neutralColor;
            }
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

            if (!Regex.IsMatch(password, @"\d"))
                return false;

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                return false;

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Window Control Methods

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            IsLoginSuccessful = false;
            AnimateButtonPress(sender as Button);
            this.Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);
            this.WindowState = WindowState.Minimized;
        }

        private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);
            isPasswordVisible = !isPasswordVisible;
            
            if (isPasswordVisible)
            {
                txtLoginPasswordVisible.Text = txtLoginPassword.Password;
                txtLoginPasswordVisible.Visibility = Visibility.Visible;
                txtLoginPassword.Visibility = Visibility.Collapsed;
                btnLoginTogglePassword.Content = "👁️‍🗨️";
            }
            else
            {
                txtLoginPassword.Password = txtLoginPasswordVisible.Text;
                txtLoginPasswordVisible.Visibility = Visibility.Collapsed;
                txtLoginPassword.Visibility = Visibility.Visible;
                btnLoginTogglePassword.Content = "👁️";
            }
        }

        private void btnRegisterTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);
            isRegisterPasswordVisible = !isRegisterPasswordVisible;
            
            if (isRegisterPasswordVisible)
            {
                txtRegisterPasswordVisible.Text = txtRegisterPassword.Password;
                txtRegisterPasswordVisible.Visibility = Visibility.Visible;
                txtRegisterPassword.Visibility = Visibility.Collapsed;
                btnRegisterTogglePassword.Content = "👁️‍🗨️";
            }
            else
            {
                txtRegisterPassword.Password = txtRegisterPasswordVisible.Text;
                txtRegisterPasswordVisible.Visibility = Visibility.Collapsed;
                txtRegisterPassword.Visibility = Visibility.Visible;
                btnRegisterTogglePassword.Content = "👁️";
            }
        }

        private void btnGoToRegister_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);
            SwitchView(false);
        }

        private void btnGoToLogin_Click(object sender, RoutedEventArgs e)
        {
            AnimateButtonPress(sender as Button);
            SwitchView(true);
        }

        #endregion
    }
}
