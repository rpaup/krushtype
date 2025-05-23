using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using krushtype.Core.ViewModels;

namespace krushtype.UI.Views
{
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
            
            PasswordBox.PasswordChanged += OnPasswordChanged;
            DataContextChanged += OnDataContextChanged;
        }
        
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ((LoginVM)e.OldValue).PropertyChanged -= ViewModel_PropertyChanged;
            }
            
            if (e.NewValue != null)
            {
                ((LoginVM)e.NewValue).PropertyChanged += ViewModel_PropertyChanged;
            }
        }
        
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginVM.Password))
            {
                string currentPassword = PasswordBox.Password;
                string newPassword = ((LoginVM)DataContext).Password;
                
                if (currentPassword != newPassword)
                {
                    PasswordBox.Password = newPassword;
                }
            }
        }
          private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((LoginVM)DataContext).Password = PasswordBox.Password;
            }
        }
        
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is LoginVM viewModel)
            {
                if (viewModel.GoToRegisterCommand.CanExecute(null))
                {
                    viewModel.GoToRegisterCommand.Execute(null);
                }
            }
        }
    }
}
