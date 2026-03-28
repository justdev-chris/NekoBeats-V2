using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace NekoBeats
{
    public static class LanguageManager
    {
        private static Dictionary<string, string> strings = new Dictionary<string, string>();
        private static string currentLang = "en";

        public static void Initialize()
        {
            string lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            string path = $"lang/{lang}.json";
            
            if (!File.Exists(path))
                lang = "en";
            
            LoadLanguage(lang);
        }

        public static void LoadLanguage(string languageCode)
        {
            string path = $"lang/{languageCode}.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                currentLang = languageCode;
            }
        }

        public static string Get(string key)
        {
            return strings.TryGetValue(key, out string value) ? value : key;
        }
        
        public static string Current => currentLang;
    }
}
