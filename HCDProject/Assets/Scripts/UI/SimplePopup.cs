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

    public void SetOneButtonPopup(string _title, string _desc, UnityAction positive, string posText = "UI_POPUP_CHECK")
    {
        gameObject.SetActive(true);
        negativeButton.gameObject.SetActive(false);
        title.SetEntry(_title);
        desc.SetEntry(_desc);
        positiveText.SetEntry(posText);
        positiveButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            positive?.Invoke();
        });
    }
    public void SetTwoButtonPopup(string _title, string _desc, UnityAction positive, UnityAction negative, string posText = "UI_POPUP_YES", string negText = "UI_POPUP_NO")
    {
        TwoButtonPopup(_title, _desc, positive, negative, posText, negText);
        positiveButton.transform.SetSiblingIndex(1);
    }

    public void SetTwoButtonWarningPopup(string _title, string _desc, UnityAction positive, UnityAction negative,
        string posText = "UI_POPUP_NO", string negText = "UI_POPUP_YES")
    {
        TwoButtonPopup(_title, _desc, positive, negative, posText, negText);
        negativeButton.transform.SetSiblingIndex(1);
    }

    private void TwoButtonPopup(string _title, string _desc, UnityAction positive, UnityAction negative,
        string posText, string negText)
    {
        gameObject.SetActive(true);
        negativeButton.gameObject.SetActive(true);
        title.SetEntry(_title);
        desc.SetEntry(_desc);
        positiveText.SetEntry(posText);
        negativeText.SetEntry(negText);
        positiveButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            positive?.Invoke();
        });
        negativeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            negative?.Invoke();
        });
    }
    
}