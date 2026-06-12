using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingPopupUi : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    private LocalizationManager localizationManager;

    private void Awake()
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
        localizationManager.ChangeLanguage(value);
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
