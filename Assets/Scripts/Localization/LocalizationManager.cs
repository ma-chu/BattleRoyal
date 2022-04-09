using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using EF.Localization;
using EF.UI;
using EF.Tools;

public class LocalizationManager : MonoBehaviour
{
    //-->
    private static Action UpdateLoc;
    private void OnEnable()
    {
        UpdateLoc += UpdateLocalization;
    }
    private void OnDisable()
    {
        UpdateLoc -= UpdateLocalization;
    }
    //<--

    private AutoLocalization[] _autoLocalizations = new AutoLocalization[0];
    private  EFButton[] _buttons = new EFButton[0];
    
    [SerializeField] private Dropdown languageDropdown;
    private void Awake()
    {
        var allLanguages = Enum.GetValues(typeof(Language))
            .Cast<Language>()
            .ToList();
        allLanguages.Remove(Language.None);

        if (languageDropdown != null)
        {
            foreach (var language in allLanguages)
                languageDropdown.options.Add(new Dropdown.OptionData(language.ToString()));
        }

        _autoLocalizations = GetComponentsInChildren<AutoLocalization>(true);
        _buttons = GetComponentsInChildren<EFButton>(true);
        
        var snapshot = GameSave.LastLoadedSnapshot ?? GameSave.Load();
        if (!snapshot.IsNull())
        {
            Localization.ApplyLanguage(snapshot.language);
            if (languageDropdown != null) languageDropdown.value = (int) snapshot.language;
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
        
        //UpdateLocalization();
        UpdateLoc?.Invoke();
    }
    
    private void UpdateLocalization()
    {
        foreach (var button in _buttons) button.UpdateLocalization();
        foreach (var autoLocalization in _autoLocalizations) autoLocalization.UpdateLocalization();
    }

}
