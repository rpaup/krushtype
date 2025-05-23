using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace krushtype.Core.Utilities.files
{
    public static class FilePathManager
    {
        private static readonly string DataFolderName = "data";
        private static readonly string DataFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DataFolderName);
        private static bool _migrationPerformed = false;

        static FilePathManager()
        {
            EnsureDataFolderExists();
        }


        public static void EnsureDataFolderExists()
        {
            if (!Directory.Exists(DataFolderPath))
            {
                Directory.CreateDirectory(DataFolderPath);
            }
            
            if (!_migrationPerformed)
            {
                MigrateExistingFiles();
                _migrationPerformed = true;
            }
        }


        public static string GetDataFilePath(string fileName)
        {
            EnsureDataFolderExists();
            return Path.Combine(DataFolderPath, fileName);
        }
        

        private static void MigrateExistingFiles()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            List<string> filesToMigrate = new List<string>
            {
                "user_settings.json",
                "users.json",
                "current_user.json"
            };
            
            string[] jsonFiles = Directory.GetFiles(baseDir, "json_data_*.json");
            string[] csvFiles = Directory.GetFiles(baseDir, "csv_data_*.csv");
            
            filesToMigrate.AddRange(jsonFiles.Select(Path.GetFileName));
            filesToMigrate.AddRange(csvFiles.Select(Path.GetFileName));
            
            foreach (string fileName in filesToMigrate)
            {
                string sourcePath = Path.Combine(baseDir, fileName);
                string destinationPath = Path.Combine(DataFolderPath, fileName);
                
                if (File.Exists(sourcePath) && !File.Exists(destinationPath))
                {
                    try
                    {
                        File.Copy(sourcePath, destinationPath);
                        
                        try
                        {
                            File.Delete(sourcePath);
                        }
                        catch
                        {
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
