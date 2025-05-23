using krushtype.Core;
using krushtype.Core.ViewModels;
using krushtype.Core.Utilities;
using krushtype.Core.Utilities.files;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace krushtype
{
    public partial class App : Application
    {
        navigationVM mainVM = new navigationVM();
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            FilePathManager.EnsureDataFolderExists();
            
            ThemeManager.Initialize();
            
            UserManager.Initialize();
            
            var mainWindow = new MainWindow() { DataContext = mainVM };
            mainWindow.Title = "krushtype";
            
            // Установка иконки приложения
            try {
                Uri iconUri = new Uri("pack://application:,,,/Assets/images/licon.png", UriKind.Absolute);
                mainWindow.Icon = new System.Windows.Media.Imaging.BitmapImage(iconUri);
            } catch (Exception ex) {
                Console.WriteLine($"Error setting application icon: {ex.Message}");
            }
            
            mainWindow.Show();
            
            JsonData.create_json();
            
            CSVData.create_csv();
        }
    }
}
