using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public class TutorialMessageBox : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent nameText;
    [SerializeField] private LocalizeStringEvent descText;
    [SerializeField] private TextMeshProUGUI rawNameText;
    [SerializeField] private TextMeshProUGUI rawDescText;
    private RectTransform rt;

    private void Awake()
    {
        rt = (RectTransform)transform;
    }
    public void SetMessage(string name, string desc, bool isTopPos)
    {
        var table = LocalizationSettings.StringDatabase.GetTable("LocalizationTable");
        if (table.GetEntry(name) != null)
            nameText.SetEntry(name);
        else
            rawNameText.text = name;
        
        if (table.GetEntry(desc) != null)
            descText.SetEntry(desc);
        else
            rawDescText.text = desc;
        rt.DOAnchorPosY(isTopPos ? 775.0f: -305.0f , 0);
    }
}
