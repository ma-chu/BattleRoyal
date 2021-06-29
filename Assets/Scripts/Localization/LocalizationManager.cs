using UnityEngine;
using EF.Localization;
using UnityEngine.UI;
using System;
using System.Linq;

public class LocalizationManager : MonoBehaviour
{
    private AutoLocalization[] _autoLocalizations = new AutoLocalization[0];
    [SerializeField] private Dropdown languageDropdown;
    private void Awake()
    {
        var allLanguages = Enum.GetValues(typeof(Language))
            .Cast<Language>()
            .ToList();
        allLanguages.Remove(Language.None);
        
        foreach (var lang in allLanguages) languageDropdown.options.Add(new Dropdown.OptionData(lang.ToString()));

        _autoLocalizations = GetComponentsInChildren<AutoLocalization>(true);
        
        var snapshot = GameSave.Load();
        if (snapshot != null)
        {
            Localization.ApplyLanguage(snapshot.language);
            languageDropdown.value = (int) snapshot.language;
        }
        else
        {
            Localization.ApplyLanguage(Language.Ru);
        }

        UpdateLocalization();
    }

    public void ChangeLanguage(int language)
    {
        Localization.ApplyLanguage((Language)language);
        UpdateLocalization();
    }
    
    public virtual void UpdateLocalization()
    {
        //foreach (var button in _buttons) button.UpdateLocalization();
        foreach (var autoLocalization in _autoLocalizations) autoLocalization.UpdateLocalization();
    }

}
