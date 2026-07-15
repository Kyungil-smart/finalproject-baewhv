using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SimplePopup : MonoBehaviour
{
    [SerializeField] private Button positiveButton;
    [SerializeField] private LocalizeStringEvent positiveText;
    [SerializeField] private Button negativeButton;
    [SerializeField] private LocalizeStringEvent negativeText;
    [SerializeField] private LocalizeStringEvent title;
    [SerializeField] private LocalizeStringEvent desc;
    
    public SimplePopup SetOpenPopup()
    {
        gameObject.SetActive(true);
        positiveButton.transform.SetSiblingIndex(1);
        return this;
    }

    public SimplePopup SetText(string _title, string _desc)
    {
        title.SetEntry(_title);
        desc.SetEntry(_desc);
        return this;
    }

    public SimplePopup SetButtonText(string posText, string negText)
    {
        positiveText.SetEntry(posText);
        negativeText.SetEntry(negText);
        return this;
    }

    public SimplePopup SetButtons(UnityAction positive, UnityAction negative)
    {
        positiveButton.onClick.RemoveAllListeners();
        positiveButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            positive?.Invoke();
        });
        negativeButton.onClick.RemoveAllListeners();
        negativeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            negative?.Invoke();
        });
        return this;
    }

    public SimplePopup SetButtonsPosSwap(bool SetSwap)
    {
        if (SetSwap)
            negativeButton.transform.SetSiblingIndex(1);
        else
            positiveButton.transform.SetSiblingIndex(1);
        return this;
    }
}