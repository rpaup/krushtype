using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using krushtype.Core.Utilities.files;

namespace krushtype.Core.Models
{
    public static class UserSettings
    {
        private static readonly string file_name = "user_settings.json";
        private static string file_path => FilePathManager.GetDataFilePath(file_name);

        private static JObject ReadSettings()
        {
            try
            {
                if (!File.Exists(file_path))
                {
                    Initialize();
                }

                string jsonContent = File.ReadAllText(file_path);
                return JObject.Parse(jsonContent);
            }
            catch (Newtonsoft.Json.JsonReaderException jEx)
            {
                try
                {
                    File.Delete(file_path);
                    Initialize();
                    string jsonContent = File.ReadAllText(file_path); 
                    return JObject.Parse(jsonContent); 
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Критическая ошибка при восстановлении файла настроек: {ex.Message}");
                    return GetDefaultSettingsObject();
                }
            }
            catch (Exception ex) 
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при чтении файла настроек: {ex.Message}");
                return GetDefaultSettingsObject();
            }
        }

        private static JObject GetDefaultSettingsObject()
        {
            return new JObject
            (
                new JProperty("language", "english"),
                new JProperty("typing_type", "time"),
                new JProperty("time_typing", 15),
                new JProperty("words_count", 150),
                new JProperty("add_punctuation", false),
                new JProperty("add_numbers", false)
            );
        }

        public static void Initialize()
        {
            if (!File.Exists(file_path))
            {
                JObject json = GetDefaultSettingsObject(); 
                File.WriteAllText(file_path, json.ToString());
            }
        }

        public static string GetLanguage()
        {
            JObject settings = ReadSettings();
            return settings["language"].ToString();
        }

        public static void SaveLanguage(string language)
        {
            JObject settings = ReadSettings();
            settings["language"] = language;
            File.WriteAllText(file_path, settings.ToString());
        }

        public static string GetTypingType()
        {
            JObject settings = ReadSettings();
            return settings["typing_type"].ToString();
        }

        public static void SaveTypingType(string typingType)
        {
            JObject settings = ReadSettings();
            settings["typing_type"] = typingType;
            File.WriteAllText(file_path, settings.ToString());
        }

        public static int GetTimeTyping()
        {
            JObject settings = ReadSettings();
            return (int)settings["time_typing"];
        }

        public static void SaveTimeTyping(int timeTyping)
        {
            JObject settings = ReadSettings();
            settings["time_typing"] = timeTyping;
            File.WriteAllText(file_path, settings.ToString());
        }

        public static int GetWordsCount()
        {
            JObject settings = ReadSettings();
            return (int)settings["words_count"];
        }

        public static void SaveWordsCount(int wordsCount)
        {
            JObject settings = ReadSettings();
            settings["words_count"] = wordsCount;
            File.WriteAllText(file_path, settings.ToString());
        }

        public static bool GetAddPunctuation()
        {
            JObject settings = ReadSettings();
            return (bool)settings["add_punctuation"];
        }

        public static void SaveAddPunctuation(bool addPunctuation)
        {
            JObject settings = ReadSettings();
            settings["add_punctuation"] = addPunctuation;
            File.WriteAllText(file_path, settings.ToString());
        }

        public static bool GetAddNumbers()
        {
            JObject settings = ReadSettings();
            return (bool)settings["add_numbers"];
        }

        public static void SaveAddNumbers(bool addNumbers)
        {
            JObject settings = ReadSettings();
            settings["add_numbers"] = addNumbers;
            File.WriteAllText(file_path, settings.ToString());
        }

        public static void SaveAllSettings(string language, string typingType, int timeTyping, 
            int wordsCount, bool addPunctuation, bool addNumbers)
        {
            JObject settings = ReadSettings();
            settings["language"] = language;
            settings["typing_type"] = typingType;
            settings["time_typing"] = timeTyping;
            settings["words_count"] = wordsCount;
            settings["add_punctuation"] = addPunctuation;
            settings["add_numbers"] = addNumbers;
            
            File.WriteAllText(file_path, settings.ToString());
        }
    }
} 