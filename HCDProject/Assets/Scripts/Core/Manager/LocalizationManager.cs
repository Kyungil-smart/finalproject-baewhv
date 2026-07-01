using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LocalizationManager : BaseManager<LocalizationManager>
{
    private readonly List<string> _language = new() { "ko", "en", "th", "vi", "id" };
    private SystemLanguage systemLanguage;

    protected override void Awake()
    {
        base.Awake();

        if (IsManagerDestroy) return;

        var handle = LocalizationSettings.InitializationOperation;

        systemLanguage = Application.systemLanguage;

        if (handle.IsDone) InitializationComplete(handle);
        else handle.Completed += InitializationComplete;
    }


    private void InitializationComplete(AsyncOperationHandle<LocalizationSettings> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            systemLanguage = (SystemLanguage)PlayerPrefs.GetInt("SaveLanguage", (int)Application.systemLanguage);
            ChangeLanguage(systemLanguage);
        }
    }

    public void ChangeLanguage(SystemLanguage index)
    {
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(index);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            Debug.Log(locale);
            PlayerPrefs.SetInt("SaveLanguage", (int)systemLanguage);
            PlayerPrefs.Save();
        }
    }

    public int GetCurrentLanguage()
    {
        Locale currentlocale = LocalizationSettings.SelectedLocale;
        if (currentlocale == null) return 0;

        for (int i = 0; i < _language.Count; i++)
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(_language[i]);
            if (locale == currentlocale) return i;
        }

        return 0;
    }
}