using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EF.Localization;
using UnityEngine.UI;
using System;
using System.Linq;

public class LocalizationManager : MonoBehaviour
{
    private AutoLocalization[] _autoLocalizations = new AutoLocalization[0];
    [SerializeField] private Dropdown languageDropdown;
    private void OnEnable()
    {
        var allLanguages = Enum.GetValues(typeof(Language))
            .Cast</*int*/Language>()
            .ToList();
        allLanguages.Remove(Language.None);
        
        foreach (var lang in allLanguages) languageDropdown.options.Add(new Dropdown.OptionData(lang.ToString()));

        //var allLanguagesOpt = allLanguagesInt.Cast<Dropdown.OptionData>().ToList();

        //languageDropdown.options.AddRange(allLanguagesOpt);

        Localization.ApplyLanguage(Language.Ru);
        _autoLocalizations = GetComponentsInChildren<AutoLocalization>(true);
        UpdateLocalization();
    }

    public void ChangeLanguage(int language)
    {
        Localization.ApplyLanguage((Language)language);
        UpdateLocalization();
    }
    
    public virtual void UpdateLocalization()
    {
        //foreach (var button in AllButtons) button.UpdateLocalization();
        foreach (var autoLocalization in _autoLocalizations) autoLocalization.UpdateLocalization();
    }

}
