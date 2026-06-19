using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NarrativeUIController : BaseUIController<NarrativeUIController>
{
    [SerializeField] private LocalizeStringEvent nameText;
    [SerializeField] private LocalizeStringEvent descText;
    [SerializeField] private TextMeshProUGUI descTMP;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private Image ColorLine;
    private StoryLocalizingRawData currentdata;
    private bool isEnd;

    private void OnEnable()
    {
        descText.OnUpdateString.AddListener(SetText);
    }
    private void OnDisable()
    {
        descText.OnUpdateString.RemoveListener(SetText);
    }

    public void SetNarrative(StoryLocalizingRawData data)
    {
        if (data == null)
        {
            Service.Get<NarrativeManager>().EndNarrative();
            isEnd = true;
            return;
        }
        currentdata = data;
        nameText.SetEntry(data.NAME);
        descText.SetEntry(data.TEXT_ID);
        
    }

    private void SetText(string text)
    {
        descTMP.maxVisibleCharacters = 0;
        DOTween.To(x => descTMP.maxVisibleCharacters = (int)x, 0f, descTMP.text.Length, 0.5f);
    }

    public void OnNextButton()
    {
        if (isEnd) return;
        if(currentdata.NEXT_ID == ""|| currentdata.NEXT_ID == null)
        {
            Service.Get<NarrativeManager>().EndNarrative();
            isEnd = true;
        }
        else
            SetNarrative(Service.Get<NarrativeManager>().GetNextNarrative());
    }

    public void OnSkipButton()
    {
        if (isEnd) return;
        Service.Get<NarrativeManager>().EndNarrative();
        isEnd = true;
    }
    
}