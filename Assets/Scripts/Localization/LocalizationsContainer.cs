using System;
using System.Collections.Generic;
using UnityEngine;

namespace EF.Localization
{
    [Serializable]
    public class LocalizationData
    {
        public string Token;
        public List<string> Translations;
    }
    
    [CreateAssetMenu(fileName = "LocalizationsContainer", menuName = "_EF/Localizations container", order = 1)]
    public class LocalizationsContainer : ScriptableObject
    {
        public List<LocalizationData> Localization;

        private static LocalizationsContainer _instance;
        
        public static LocalizationsContainer Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = Resources.Load<LocalizationsContainer>("LocalizationsContainer");
                return _instance;
            }
        }

        public static Dictionary<string, string> GetDictionary(Language language)
        {
            var dictionary = new Dictionary<string, string>();

            var languageIndex = (int)language;

            foreach (var localizationData in Instance.Localization)
            {
                var translated = localizationData.Translations[languageIndex];

                dictionary.Add(localizationData.Token, translated);
            }

            return dictionary;
        }
    }
}