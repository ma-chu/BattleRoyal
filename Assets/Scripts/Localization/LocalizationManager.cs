using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using EF.Localization;
using EF.UI;
using EF.Tools;

public class LocalizationManager : MonoBehaviour
{
    private AutoLocalization[] _autoLocalizations = new AutoLocalization[0];
    private  EFButton[] _buttons = new EFButton[0];
    
    [SerializeField] private Dropdown languageDropdown;
    private void Awake()
    {
        var allLanguages = Enum.GetValues(typeof(Language))
            .Cast<Language>()
            .ToList();
        allLanguages.Remove(Language.None);
        
        foreach (var language in allLanguages) languageDropdown.options.Add(new Dropdown.OptionData(language.ToString()));

        _autoLocalizations = GetComponentsInChildren<AutoLocalization>(true);
        _buttons = GetComponentsInChildren<EFButton>(true);
        
        var snapshot = GameSave.LastLoadedSnapshot ?? GameSave.Load();
        if (!snapshot.IsNull())
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
        foreach (var button in _buttons) button.UpdateLocalization();
        foreach (var autoLocalization in _autoLocalizations) autoLocalization.UpdateLocalization();
    }

}
