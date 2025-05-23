using System;
using System.Security.Cryptography;
using System.Text;

namespace krushtype.Core.Models
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastLogin { get; set; }
        public string ProfileImagePath { get; set; }
        public bool IsActive { get; set; }
        public string Id { get; set; }

        public User() 
        {
            Id = Guid.NewGuid().ToString();
            JoinDate = DateTime.Now;
            LastLogin = DateTime.Now;
            IsActive = true;
            ProfileImagePath = "/Assets/images/account_icon.png";
        }

        public User(string username, string password)
        {
            Id = Guid.NewGuid().ToString();
            Username = username;
            JoinDate = DateTime.Now;
            LastLogin = DateTime.Now;
            IsActive = true;
            ProfileImagePath = "/Assets/images/account_icon.png";
            
            PasswordHash = HashPassword(password);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPassword(string password)
        {
            string hashedInputPassword = HashPassword(password);
            return hashedInputPassword == PasswordHash;
        }
    }
}
