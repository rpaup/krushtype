using System;
using System.Windows;
using System.Windows.Controls;
using krushtype.Core.ViewModels;

namespace krushtype.UI.Views
{
    public partial class Register : UserControl
    {
        public Register()
        {
            InitializeComponent();
            
            PasswordBox.PasswordChanged += OnPasswordChanged;
            ConfirmPasswordBox.PasswordChanged += OnConfirmPasswordChanged;
            DataContextChanged += OnDataContextChanged;
        }
        
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ((RegisterVM)e.OldValue).PropertyChanged -= ViewModel_PropertyChanged;
            }
            
            if (e.NewValue != null)
            {
                ((RegisterVM)e.NewValue).PropertyChanged += ViewModel_PropertyChanged;
            }
        }
        
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RegisterVM.Password))
            {
                string currentPassword = PasswordBox.Password;
                string newPassword = ((RegisterVM)DataContext).Password;
                
                if (currentPassword != newPassword)
                {
                    PasswordBox.Password = newPassword;
                }
            }
            else if (e.PropertyName == nameof(RegisterVM.ConfirmPassword))
            {
                string currentConfirmPassword = ConfirmPasswordBox.Password;
                string newConfirmPassword = ((RegisterVM)DataContext).ConfirmPassword;
                
                if (currentConfirmPassword != newConfirmPassword)
                {
                    ConfirmPasswordBox.Password = newConfirmPassword;
                }
            }
        }
        
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((RegisterVM)DataContext).Password = PasswordBox.Password;
            }
        }
          private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((RegisterVM)DataContext).ConfirmPassword = ConfirmPasswordBox.Password;
            }
        }
        
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is RegisterVM viewModel)
            {
                if (viewModel.GoToLoginCommand.CanExecute(null))
                {
                    viewModel.GoToLoginCommand.Execute(null);
                }
            }
        }
    }
}
