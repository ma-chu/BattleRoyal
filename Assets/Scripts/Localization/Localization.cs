using System;
using System.Collections.Generic;
using EF.Tools;

namespace EF.Localization
{
    public enum Language
    {
        None = -1,
        Ru = 0,
        En = 1
    }
    
    public static class Localization
    {
        public static Language CurrentLanguage { get; private set; }
        
        private static Dictionary<string, string> _dictionary = new Dictionary<string, string>();
        
        public static void ApplyLanguage(Language language)     // 1. Вызывать при инициализации и при смене языка в меню
        {
            CurrentLanguage = language;
            _dictionary = LocalizationsContainer.GetDictionary(language);
        }

        public static string Localize(this Enum token)
        {
            return Localize(token.ToString());
        }
        
        public static string Localize(this string token)        // 2. После этого надо бы вызывать этот метод для всех токенов - скрипт AutoLocalization
        {
            var tokenToLower = token.ToLower();

            var localized = _dictionary.ContainsKey(tokenToLower) ? _dictionary[tokenToLower] : "";

            if (localized.IsNullOrEmpty()) localized = token;

            return localized;
        }
        
    }
}