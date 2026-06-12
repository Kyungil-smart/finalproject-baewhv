using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NarrativeUIController : BaseUIController<NarrativeUIController>
{
    [SerializeField] private LocalizeStringEvent nameText;
    [SerializeField] private LocalizeStringEvent descText;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private Image ColorLine;
    private StoryLocalizingRawData currentdata;
    private bool isEnd;
    
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
    
}