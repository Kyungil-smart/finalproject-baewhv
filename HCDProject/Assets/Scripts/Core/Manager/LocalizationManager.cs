using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizationManager : BaseManager<LocalizationManager>
{
    private readonly List<string> _language = new() { "ko", "en", "th", "vi", "id" };

    private void Awake()
    {
        base.Awake();

        LocalizationSettings.InitializationOperation.WaitForCompletion();
    }

    public void ChangeLanguage(int index)
    {
        if (index < 0 || index >= _language.Count) return;
        
        string targetLanguage = _language[index];
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(targetLanguage);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            
            PlayerPrefs.SetInt("SaveLanguage", index);
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
