using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class RewardUIController : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent titleText;
    [SerializeField] private LocalizeStringEvent contentText;
    [SerializeField] private RewardButtonUI[] buttonList;
    [SerializeField] private Button reRollButton;
    [SerializeField] private RectTransform actionUI;
    private int selectedIndex = -1;

    private UnityAction CloseCallback;
    private UnityAction RewardAction;

    public ObserveValue<bool> isOpenRewardPopup = new();

    private void OnDisable()
    {
        isOpenRewardPopup.Value = false;
        CloseCallback?.Invoke();
        CloseCallback = null;
        Service.Get<TimeManager>()?.LoadTimeScale();
    }

    public void OnButtonSelected(int index)
    {
        foreach (var button in buttonList)
        {
            if (button.GetIndex == index)
            {
                if (button.IsSelected)
                {
                    button.OnButtonInvoke();
                    gameObject.SetActive(false);
                }
                else
                {
                    button.IsSelected = true;
                }
            }
            else
            {
                button.IsSelected = false;
            }
        }
    } 

    private void StartPopup(string title, string content, bool isReRoll)
    {
        gameObject.SetActive(true);
        selectedIndex = -1;
        if(!isReRoll)
            Service.Get<AdsManager>()?.ResetAdChance();
        isOpenRewardPopup.Value = true;
        if(titleText) titleText.SetEntry(title);
        if(contentText) contentText.SetEntry(content);
        if (!isReRoll)
            Service.Get<TimeManager>()?.SaveTimeScale();
        actionUI?.DOScale(Vector3.zero, 0.5f).From().SetUpdate(true);
    }


    public void SetLevelUpReward(UnityAction action, bool isReRoll = false)
    {
        StartPopup("UI_LVU_TITLE", "UI_LVU_DESC", isReRoll);
        CloseCallback = action;
        var CurrentReward = Service.Get<PlayerManager>()?.GetLevelUpRewards();
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetReward(CurrentReward[i], Service.Get<PlayerManager>().OnSelectLevelUpReward, i);
        }

        if (!Service.Get<AdsManager>().IsAdUsed)
        {
            reRollButton.interactable = true;
            reRollButton.onClick.RemoveAllListeners();
            reRollButton.onClick.AddListener(() =>
            {
                Service.Get<AdsManager>().ShowRewardedAd(() =>
                {
                    StartCoroutine(KeepTimeScaleRoutine(() =>
                    {
                        SetLevelUpReward(action, true);
                    }));
                });
            });
        }
        else
        {
            reRollButton.interactable = false;
        }
    }

    private IEnumerator KeepTimeScaleRoutine(Action action)
    {
        yield return null;
        if(Service.Get<TimeManager>() && Service.Get<TimeManager>().IsPaused)
            Time.timeScale = 0;

        action?.Invoke();
    }


    public void SetRelicReward(UnityAction action, bool isReRoll = false)
    {
        StartPopup("UI_RW_TITLE", "UI_RW_DESC", isReRoll);
        CloseCallback = action;
        var CurrentReward = Service.Get<RelicManager>()?.GetStageRandomRewards();
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetReward(CurrentReward[i], index =>
            {
                Service.Get<RelicManager>()?.OnSelectRelicReward(index);
                Service.Get<GameManager>()?.SaveGame(Service.Get<RelicManager>()?.MyRelics);
                selectedIndex = index;
            }, i);
        }

        if (!Service.Get<AdsManager>().IsAdUsed)
        {
            reRollButton.interactable = true;
            reRollButton.onClick.RemoveAllListeners();
            reRollButton.onClick.AddListener(() =>
            {
                Service.Get<AdsManager>().ShowRewardedAd(() =>
                {
                    StartCoroutine(KeepTimeScaleRoutine(() =>
                    {
                        SetRelicReward(action, true);
                    }));
                });
            });
        }
        else
        {
            reRollButton.interactable = false;
        }
    }

    public void CopyElement(RewardButtonUI ui)
    {
        if (selectedIndex == -1) return;
        buttonList[selectedIndex].CopyElement(ui);
    }

    public void AddListener(UnityAction<bool> action)
    {
        isOpenRewardPopup.AddListener(action);
    }

    public void RemoveListener(UnityAction<bool> action)
    {
        isOpenRewardPopup.RemoveListener(action);
    }
}