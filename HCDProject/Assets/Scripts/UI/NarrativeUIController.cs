using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NarrativeUIController : BaseUIController<NarrativeUIController>
{
    //Desc
    [SerializeField] private LocalizeStringEvent nameText;
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private LocalizeStringEvent descText;
    [SerializeField] private TextMeshProUGUI descTMP;
    
    //Character
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private Image ColorLine;
    
    //Auto
    [SerializeField] private TextMeshProUGUI autoStatusText;
    [SerializeField] private TextMeshProUGUI autoText;
    
    //Queue
    [SerializeField] private NarrativeUIQueue queue;
    
    
    //Region
    [SerializeField] private TextMeshProUGUI StageNumber;
    [SerializeField] private LocalizeStringEvent StageText;
    
    
    
    private StoryLocalizingRawData currentdata;
    private bool isEnd;

    public void SetRegion(int chapter, int stage, string text)
    {
        StageNumber.text = $"stage {chapter}-{stage}";
        StageText.SetEntry(text);
    }


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
        if (string.IsNullOrEmpty(data.NAME))
            nameTMP.text = "";
        else
            nameText.SetEntry(data.NAME);
        if (string.IsNullOrEmpty(data.NAME))
            descTMP.text = "";
        else
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
        if (currentdata.NEXT_ID == "" || currentdata.NEXT_ID == null)
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