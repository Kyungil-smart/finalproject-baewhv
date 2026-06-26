using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using TMPro;
using UnityEngine;

public class SettingPopupUI : BaseUIController<SettingPopupUI>
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private GameObject soundUI;
    [SerializeField] private GameObject intensityUI;
    private LocalizationManager localizationManager;

    private void Start()
    {
        localizationManager = Service.Get<LocalizationManager>();
    }

    private void OnEnable()
    {
        Service.Get<TimeManager>()?.SaveTimeScale();
        DropdownUi();
    }

    private void OnDisable()
    {
        Service.Get<TimeManager>()?.LoadTimeScale();
    }

    private void DropdownUi()
    {
        if (languageDropdown == null || localizationManager == null) return;

        languageDropdown.ClearOptions();

        List<string> languageOptions = new()
        {
            "Korean",
            "English",
            "Thai",
            "Vietnamese",
            "Indonesian"
        };
        languageDropdown.AddOptions(languageOptions);

        languageDropdown.value = localizationManager.GetCurrentLanguage();
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(OnLanguageChange);
    }

    private void OnLanguageChange(int value)
    {
        localizationManager.ChangeLanguage((SystemLanguage)value);
    }

    public void OpenPopup(ESettingPopupType type)
    {
        gameObject.SetActive(true);
        soundUI.SetActive(type != ESettingPopupType.OnlyLanguage);
        intensityUI.SetActive(type != ESettingPopupType.OnlyLanguage);
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}

public enum ESettingPopupType
{
    none = 0,
    OnlyLanguage = 1,
    Battle = 2,
}