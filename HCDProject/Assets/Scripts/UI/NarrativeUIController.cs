using System;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NarrativeUIController : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent nameText;
    [SerializeField] private LocalizeStringEvent descText;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private Image ColorLine;

    public void SetNarrative(StoryLocalizingRawData data)
    {
        
    }
}