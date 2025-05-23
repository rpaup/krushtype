using krushtype.Core.Utilities;
using krushtype.Core.Utilities.files;
using krushtype.UI.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace krushtype.Core.ViewModels
{
    public class navigationVM : Core.Utilities.MainModelView
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }
        
        private readonly INavigationService _navigationService;
        
        public ICommand AccountCommand { get; set; }
        public ICommand TypingCommand { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        public ICommand SettingsCommand { get; set; }
        
        public ICommand CloseWindowCommand { get; set; }
        public ICommand CollapseWindowCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        
        private void Account(object obj) 
        {
            if (UserManager.IsUserLoggedIn())
                CurrentView = new accountVM(_navigationService); 
            else
                ShowAuthWindow(true); 
        }
        
        private void Typing(object obj) => CurrentView = new typingVM(this);
        
        private void Login(object obj)
        {
            ShowAuthWindow(true); 
        }
        
        private void Register(object obj)
        {
            ShowAuthWindow(true); 
        }
        
        private void CloseWindow(object obj) => Process.GetCurrentProcess().Kill();
        private void CollapseWindow(object obj)
        {
            if (obj is Window window)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
        
        private void Logout(object obj)
        {
            UserManager.LogoutUser();
            CurrentView = new typingVM(this);
            ShowAuthWindow(false); 
        }
        
        private void ShowAuthWindow(bool navigateToAccountOnSuccess = true) 
        {
            var currentViewInstance = CurrentView;
            bool isTypingView = currentViewInstance is typingVM;
            
            var authWindow = new AuthWindowNew();
            
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                authWindow.Owner = Application.Current.MainWindow;
            }
            
            authWindow.ShowDialog();
            
            if (authWindow.IsAuthSuccessful)
            {
                if (navigateToAccountOnSuccess)
                {
                    CurrentView = new accountVM(_navigationService); 
                }
            }
            else 
            {
                if (isTypingView)
                {
                    OnPropertyChanged(nameof(CurrentView));
                }
                else
                {
                    CurrentView = new typingVM(this);
                }
            }
        }
        
        public navigationVM()
        {
            _navigationService = new NavigationService(this);
            
            AccountCommand = new RelayCommand(Account);
            TypingCommand = new RelayCommand(Typing);
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
            CloseWindowCommand = new RelayCommand(CloseWindow);
            CollapseWindowCommand = new RelayCommand(CollapseWindow);
            LogoutCommand = new RelayCommand(Logout);
            
            _currentView = new typingVM(this);
        }
    }
}
