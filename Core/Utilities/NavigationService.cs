using System;
using krushtype.Core.ViewModels;

namespace krushtype.Core.Utilities
{
    public class NavigationService : INavigationService
    {
        public readonly navigationVM _navigationViewModel;
        
        public NavigationService(navigationVM navigationViewModel)
        {
            _navigationViewModel = navigationViewModel;
        }
          public void NavigateToLogin()
        {
            _navigationViewModel.LoginCommand.Execute(null);
        }
        
        public void NavigateToRegister()
        {
            _navigationViewModel.RegisterCommand.Execute(null);
        }
        
        public void NavigateToAccount()
        {
            _navigationViewModel.AccountCommand.Execute(null);
        }
        
        public void NavigateToTyping()
        {
            _navigationViewModel.TypingCommand.Execute(null);
        }
        
        public void NavigateToSettings()
        {
            _navigationViewModel.SettingsCommand.Execute(null);
        }
    }
}
