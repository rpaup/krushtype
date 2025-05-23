using System;
using System.Windows;
using System.Windows.Input;
using krushtype.Core.Models;
using krushtype.Core.Utilities;
using krushtype.Core.Utilities.files;

namespace krushtype.Core.ViewModels
{
    public class LoginVM : MainModelView
    {
        public ICommand GoToRegisterCommand { get; }
        
        public ICommand LoginCommand { get; }
        
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
        
        public LoginVM(INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            GoToRegisterCommand = new RelayCommand(ExecuteGoToRegister);
            LoginCommand = new RelayCommand(ExecuteLogin);
            
            UserManager.Initialize();
        }
        
        private void ExecuteGoToRegister(object parameter)
        {
            _navigationService.NavigateToRegister();
        }
        
        private async void ExecuteLogin(object parameter)
        {
            try
            {
                ErrorMessage = string.Empty;
                
                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Username cannot be empty";
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Password cannot be empty";
                    return;
                }
                
                IsLoading = true;
                
                await System.Threading.Tasks.Task.Delay(500);
                
                bool success = UserManager.LoginUser(Username, Password);
                
                if (success)
                {
                    _navigationService.NavigateToAccount();
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
