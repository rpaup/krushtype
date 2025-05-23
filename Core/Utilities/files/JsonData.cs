using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using krushtype.Properties;

namespace krushtype.Core.Utilities.files
{
    public static class JsonData
    {
        private static readonly string baseFilePath = "json_data";
        private static readonly string fileExtension = ".json";
        
        private static string GetCurrentUserFilePath()
        {
            string userId = "anonymous"; 
            if (UserManager.IsUserLoggedIn())
            {
                userId = UserManager.CurrentUser.Id;
            }
            
            string fileName = $"{baseFilePath}_{userId}{fileExtension}";
            return FilePathManager.GetDataFilePath(fileName);
        }

        public static void create_json()
        {
            string filePath = GetCurrentUserFilePath();
            
            if (!File.Exists(filePath))
            {
                JObject json = new JObject
                (
                    new JProperty("name", UserManager.IsUserLoggedIn() ? UserManager.CurrentUser.Username : string.Empty),
                    new JProperty("join_date", UserManager.IsUserLoggedIn() ? UserManager.CurrentUser.JoinDate : DateTime.Now),
                    new JProperty("tests_started", 0),
                    new JProperty("tests_completed", 0),
                    new JProperty("time_typing", TimeSpan.Zero),
                    new JProperty("words_typed", 0)
                );
                File.WriteAllText(filePath, json.ToString());
            }
        }
        
        private static JObject read_json()
        {
            string filePath = GetCurrentUserFilePath();
            
            create_json();
            
            string jsonContent = File.ReadAllText(filePath);
            JObject json = JObject.Parse(jsonContent);
            return json;
        }       
        public static string get_name()
        {
            if (UserManager.IsUserLoggedIn())
            {
                return UserManager.CurrentUser.Username;
            }
            
            JObject json = read_json();
            string name = json["name"].ToString();
            return name;
        }
        public static void set_name(string name)
        {
            if (UserManager.IsUserLoggedIn())
            {
                UserManager.UpdateUserProfile(name);
            }
            
            JObject json = read_json();
            json["name"] = name;
            File.WriteAllText(GetCurrentUserFilePath(), json.ToString());
        }
        public static DateTime get_join_date()
        {
            if (UserManager.IsUserLoggedIn())
            {
                return UserManager.CurrentUser.JoinDate;
            }
            
            JObject json = read_json();
            DateTime date = Convert.ToDateTime(json["join_date"]);
            return date;
        }       
        public static int get_tests_started()
        {
            JObject json = read_json();
            int tests_started = (int)json["tests_started"];
            return tests_started;
        }
        public static void update_tests_started(int tests_started)
        {
            JObject json = read_json();
            json["tests_started"] = (int)json["tests_started"] + tests_started;
            File.WriteAllText(GetCurrentUserFilePath(), json.ToString());
        }
        public static int get_tests_completed()
        {
            JObject json = read_json();
            int tests_completed = (int)json["tests_completed"];
            return tests_completed;
        }
        public static void update_tests_completed()
        {
            JObject json = read_json();
            json["tests_completed"] = (int)json["tests_completed"] + 1;
            File.WriteAllText(GetCurrentUserFilePath(), json.ToString());
        }        
        public static TimeSpan get_time_typing()
        {
            JObject json = read_json();
            TimeSpan time_typing = TimeSpan.Parse(json["time_typing"].ToString());
            return time_typing;
        }
        public static void update_time_typing(TimeSpan time_typing)
        {
            JObject json = read_json();
            TimeSpan currentTime = TimeSpan.Parse(json["time_typing"].ToString());
            json["time_typing"] = currentTime + time_typing;
            File.WriteAllText(GetCurrentUserFilePath(), json.ToString());
        }
        public static int get_words_typed()
        {
            JObject json = read_json();
            int words_typed = (int)json["words_typed"];
            return words_typed;
        }
        public static void update_words_typed(int word_typed)
        {
            JObject json = read_json();
            json["words_typed"] = (int)json["words_typed"] + word_typed;
            File.WriteAllText(GetCurrentUserFilePath(), json.ToString());
        }
    }
}
