using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using krushtype.Core.Models;
using krushtype.Core.Utilities;
using krushtype.Core.Utilities.files;

namespace krushtype.Core.ViewModels
{
    public class RegisterVM : MainModelView
    {
        public ICommand GoToLoginCommand { get; }
        
        public ICommand RegisterCommand { get; }
        
        private string _username;
        public string Username 
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged(); }
        }
        
        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }
        
        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { _confirmPassword = value; OnPropertyChanged(); }
        }
        
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged(); }
        }
        
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged(); }
        }
        
        private readonly INavigationService _navigationService;
        
        public RegisterVM(INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            GoToLoginCommand = new RelayCommand(ExecuteGoToLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
            
            UserManager.Initialize();
        }
        
        private void ExecuteGoToLogin(object parameter)
        {
            _navigationService.NavigateToLogin();
        }
        
        private async void ExecuteRegister(object parameter)
        {
            try
            {
                ErrorMessage = string.Empty;
                bool isValid = true;
                
                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Введите логин";
                    isValid = false;
                }
                else if (Username.Length < 3)
                {
                    ErrorMessage = "Минимум 3 символа";
                    isValid = false;
                }
                else if (!Regex.IsMatch(Username, @"^[a-zA-Z0-9_]+$"))
                {
                    ErrorMessage = "Только латинские буквы, цифры и _";
                    isValid = false;
                }
                
                if (string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Введите пароль";
                    isValid = false;
                }
                else if (!IsPasswordStrong(Password))
                {
                    ErrorMessage = "Пароль не соответствует требованиям";
                    isValid = false;
                }
                
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Пароли не совпадают";
                    isValid = false;
                }
                
                if (!isValid) return;
                
                IsLoading = true;
                
                await System.Threading.Tasks.Task.Delay(500);
                Console.WriteLine($"Attempting to register user: {Username}");
                bool success = UserManager.RegisterUser(Username, Password);
                
                if (success)
                {
                    Console.WriteLine("Registration successful, navigating to account");
                    _navigationService.NavigateToAccount();
                }
                else
                {
                    Console.WriteLine("Registration failed");
                    ErrorMessage = "Логин уже занят";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка регистрации: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
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
    }
}
