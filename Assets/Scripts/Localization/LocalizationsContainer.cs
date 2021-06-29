using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EF.Tools;

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
        
       /* public static string Get(string token, Language language)
        {
            var data = Instance.Localization.Find(d => d.Token == token);
            return data != null ? data.Translations[(int)language] : token;
        }*/
        
        public static Dictionary<string, string> GetDictionary(Language language)
        {
            var dictionary = new Dictionary<string, string>();

            var langIndex = (int)language;

            foreach (var localizationData in Instance.Localization)
            {
                var translated = localizationData.Translations[langIndex];

                dictionary.Add(localizationData.Token, translated);
            }

            return dictionary;
        }
        
        /*public void OnParsed()
        {
            var allLanguages = Enum.GetValues(typeof(Language))
                                   .Cast<int>()
                                   .ToList();

            allLanguages.Remove((int)Language.None);
            var maxIndex = allLanguages.LastValue();          
            
            foreach (var localizationData in Localization)  // Проходится по всем пунктам списка Localization контейнера LocalizationContainer - "данным локализации"
            {
                localizationData.Token = localizationData.Token.ToLower();  // 1. токены переводит в нижний регистр
                
                var translations = localizationData.Translations ??
                                              (localizationData.Translations = new List<string>(maxIndex));

                while (translations.Count != maxIndex + 1)                  // 2. добавляет пустых строк переводов по числу языков (в Enum Language)
                {
                    translations.Add(string.Empty);
                }

                if (localizationData.Token == "empty") continue;            

                foreach (var languageIndex in allLanguages)             // 3. в непустых токенах пустые строки переводов заменяет на сами токены
                {
                    if (translations[languageIndex] == string.Empty)
                    {
                        translations[languageIndex] = localizationData.Token;
                    }
                }
            }
#if UNITY_EDITOR
            UnityEditor.Selection.activeObject = this;
#endif
        }*/
    }
}