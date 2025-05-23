using System.Windows;
using krushtype.Core.Utilities.files;
using krushtype.UI.Views;

namespace krushtype
{
    public static class AuthManager
    {        public static bool ShowLoginDialog()
        {
            // Check if user is already logged in
            if (UserManager.IsUserLoggedIn())
                return true;
                
            // Show the new authentication window
            var authWindow = new AuthWindowNew();
            authWindow.Owner = Application.Current.MainWindow;
            authWindow.ShowDialog();
            
            // Return whether login was successful
            return authWindow.IsAuthSuccessful;
        }
        
        public static void Logout()
        {
            UserManager.LogoutUser();
        }
        
        // Example usage:
        /*
        private void InitializeApplication()
        {
            // Check if the user is authenticated
            bool isAuthenticated = AuthManager.ShowLoginDialog();
            
            if (isAuthenticated)
            {
                // Continue with the application
                ShowMainWindow();
            }
            else
            {
                // User cancelled or closed the auth window - exit the application
                Application.Current.Shutdown();
            }
        }
        */
    }
}
