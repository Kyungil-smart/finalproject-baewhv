using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopupUI : BaseUIController<SettingPopupUI>
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private GameObject soundUI;
    [SerializeField] private GameObject intensityUI;
    [SerializeField] private GameObject RetireButton;

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
        if (languageDropdown == null) return;

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
        Debug.Log("here");
        languageDropdown.value = Service.Get<LocalizationManager>().GetCurrentLanguage();
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(OnLanguageChange);
    }

    private void OnLanguageChange(int value)
    {
        SystemLanguage language = value switch
        {
            0 => SystemLanguage.Korean,
            2 => SystemLanguage.Thai,
            3 => SystemLanguage.Vietnamese,
            4 => SystemLanguage.Indonesian,
            _ => SystemLanguage.English
        };
        Debug.Log($"language change : {value}");
        Service.Get<LocalizationManager>().ChangeLanguage(language);
    }

    public void OpenPopup(ESettingPopupType type)
    {
        gameObject.SetActive(true);
        soundUI.SetActive(type != ESettingPopupType.OnlyLanguage);
        intensityUI.SetActive(type != ESettingPopupType.OnlyLanguage);
        RetireButton.SetActive(type == ESettingPopupType.Battle);
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    public void OnReturnMenu()
    {
        Service.Get<UIManager>().SimplePopup.SetTwoButtonPopup("경고", "퇴각할 경우 현재 스테이지는 초기화됩니다.\n\n퇴각하시겠습니까?", 
            () =>
            {
                gameObject.SetActive(false);    
                Service.Get<SceneController>().ChangeScene(SceneType.StageSelect);
            },null);
    }
}

public enum ESettingPopupType
{
    none = 0,
    OnlyLanguage = 1,
    Battle = 2,
}