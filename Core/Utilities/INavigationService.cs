using System;

namespace krushtype.Core.Utilities
{
    public interface INavigationService
    {
        void NavigateToLogin();
        void NavigateToRegister();
        void NavigateToAccount();
        void NavigateToTyping();
        void NavigateToSettings();
    }
}
