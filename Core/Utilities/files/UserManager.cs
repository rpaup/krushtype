using krushtype.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace krushtype.Core.Utilities.files
{
    public static class UserManager
    {
        private static readonly string usersFileName = "users.json";
        private static readonly string currentUserFileName = "current_user.json";
        private static string UsersFilePath => FilePathManager.GetDataFilePath(usersFileName);
        private static string CurrentUserFilePath => FilePathManager.GetDataFilePath(currentUserFileName);
        private static User _currentUser = null;
        
        public static User CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    LoadCurrentUser();
                }
                return _currentUser;
            }
        }

        public static void Initialize()
        {
            if (!File.Exists(UsersFilePath))
            {
                List<User> users = new List<User>();
                SaveUsers(users);
            }
        }
        
        public static List<User> GetAllUsers()
        {
            try
            {
                if (!File.Exists(UsersFilePath))
                    return new List<User>();
                    
                string json = File.ReadAllText(UsersFilePath);
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting users: {ex.Message}");
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<User>();
            }
        }
        
        private static void SaveUsers(List<User> users)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(UsersFilePath);
                if (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                string json = JsonConvert.SerializeObject(users, Formatting.Indented);
                File.WriteAllText(UsersFilePath, json);
                Console.WriteLine($"Users saved successfully to {UsersFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users: {ex.Message}");
                MessageBox.Show($"Error saving user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public static bool RegisterUser(string username, string password)
        {
            try
            {
                Console.WriteLine("RegisterUser called: " + username);
                List<User> users = GetAllUsers();
                
                if (users != null && users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("Username already exists");
                    return false; 
                }
                
                User newUser = new User(username, password);
                users.Add(newUser);
                Console.WriteLine("Saving user to: " + UsersFilePath);
                SaveUsers(users);
                
                _currentUser = newUser;
                SaveCurrentUser();
                Console.WriteLine("User registration successful");
                
                List<User> allUsers = GetAllUsers();
                if (allUsers.Count == 1)
                {
                    MigrateExistingDataToCurrentUser();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering user: {ex.Message}");
                MessageBox.Show($"Error registering user: {ex.Message}", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool LoginUser(string username, string password)
        {
            List<User> users = GetAllUsers();
            User user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            
            if (user == null || !user.VerifyPassword(password))
            {
                return false;
            }
            
            user.LastLogin = DateTime.Now;
            SaveUsers(users);
            
            _currentUser = user;
            SaveCurrentUser();
            
            return true;
        }

        public static void LogoutUser()
        {
            _currentUser = null;
            if (File.Exists(CurrentUserFilePath))
            {
                File.Delete(CurrentUserFilePath);
            }
        }
        
        private static void SaveCurrentUser()
        {
            try
            {
                if (_currentUser != null)
                {
                    string directoryPath = Path.GetDirectoryName(CurrentUserFilePath);
                    if (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    
                    string json = JsonConvert.SerializeObject(_currentUser, Formatting.Indented);
                    File.WriteAllText(CurrentUserFilePath, json);
                    Console.WriteLine($"Current user saved successfully to {CurrentUserFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving current user: {ex.Message}");
                MessageBox.Show($"Error saving current user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void LoadCurrentUser()
        {
            if (File.Exists(CurrentUserFilePath))
            {
                try
                {
                    string json = File.ReadAllText(CurrentUserFilePath);
                    _currentUser = JsonConvert.DeserializeObject<User>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _currentUser = null;
                    if (File.Exists(CurrentUserFilePath))
                    {
                        File.Delete(CurrentUserFilePath);
                    }
                }
            }
        }

        public static bool IsUserLoggedIn()
        {
            return CurrentUser != null;
        }

        private static void MigrateExistingDataToCurrentUser()
        {
            string jsonDataPath = "json_data.json";
            if (File.Exists(jsonDataPath))
            {
                try
                {
                    JObject json = JObject.Parse(File.ReadAllText(jsonDataPath));
                    
                    string existingName = json["name"].ToString();
                    if (!string.IsNullOrEmpty(existingName))
                    {
                        List<User> users = GetAllUsers();
                        var userToUpdate = users.FirstOrDefault(u => u.Id == _currentUser.Id);
                        if (userToUpdate != null && string.IsNullOrEmpty(userToUpdate.Username))
                        {
                            userToUpdate.Username = existingName;
                            SaveUsers(users);
                            _currentUser = userToUpdate;
                            SaveCurrentUser();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error migrating data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public static void UpdateUserProfile(string username)
        {
            if (_currentUser == null)
                return;

            List<User> users = GetAllUsers();
            User userToUpdate = users.FirstOrDefault(u => u.Id == _currentUser.Id);
            
            if (userToUpdate != null)
            {
                if (!userToUpdate.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                    users.Any(u => u.Id != _currentUser.Id && 
                                 u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Username is already taken. Please choose a different one.", 
                                     "Username Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                userToUpdate.Username = username;
                SaveUsers(users);
                
                _currentUser = userToUpdate;
                SaveCurrentUser();
            }
        }

        public static bool ChangePassword(string currentPassword, string newPassword)
        {
            if (_currentUser == null)
                return false;

            List<User> users = GetAllUsers();
            User userToUpdate = users.FirstOrDefault(u => u.Id == _currentUser.Id);
            
            if (userToUpdate != null && userToUpdate.VerifyPassword(currentPassword))
            {
                User updatedUser = new User(userToUpdate.Username, newPassword)
                {
                    Id = userToUpdate.Id,
                    JoinDate = userToUpdate.JoinDate,
                    LastLogin = userToUpdate.LastLogin,
                    IsActive = userToUpdate.IsActive,
                    ProfileImagePath = userToUpdate.ProfileImagePath
                };
                
                int index = users.FindIndex(u => u.Id == _currentUser.Id);
                if (index >= 0)
                {
                    users[index] = updatedUser;
                    SaveUsers(users);
                    
                    _currentUser = updatedUser;
                    SaveCurrentUser();
                    
                    return true;
                }
            }
            
            return false;
        }
    }
}
