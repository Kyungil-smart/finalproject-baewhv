using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RewardUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private RewardButtonUI[] buttonList;
    [SerializeField] private Button reRollButton;
    //private int selectedIndex = -1;

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
        if(!isReRoll)
            Service.Get<AdsManager>()?.ResetAdChance();
        isOpenRewardPopup.Value = true;
        titleText.text = title;
        contentText.text = content;
        Service.Get<TimeManager>()?.SaveTimeScale();
    }


    public void SetLevelUpReward(UnityAction action, bool isReRoll = false)
    {
        StartPopup("Level UP!", "강화 효과를 선택하세요.\n해당 효과는 이번 노드에서만 적용됩니다.", isReRoll);
        CloseCallback = action;
        var CurrentReward = Service.Get<PlayerManager>()?.GetLevelUpRewards();
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetReward(CurrentReward[i], Service.Get<PlayerManager>().OnSelectLevelUpReward, i);
        }

        if (!Service.Get<AdsManager>().IsAdUsed)
        {
            reRollButton.interactable = true;
            reRollButton.onClick.AddListener(() => Service.Get<AdsManager>().ShowRewardedAd(() => { SetLevelUpReward(action, true); }));
        }
        else
        {
            reRollButton.interactable = false;
        }

        if (!Service.Get<AdsManager>().IsAdUsed)
        {
            reRollButton.interactable = true;
            reRollButton.onClick.RemoveAllListeners();
            //  코루틴을 통해 sdk에서 제어하는 타임스케일을 다시 제어해주어야 합니다 null 대기로 close이벤트 발생시 먼저 발동하는 sdk의 resume 이후에 타임스케일을 한번 다시 제어합니다 
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

    // 타임스케일을 다시 0으로 제어 이후 게임 로직상의 타임스케일은 timemanager가 제어가능한걸로 확인 됨 
    private IEnumerator KeepTimeScaleRoutine(Action action)
    {
        yield return null;

        Time.timeScale = 0;
    }

    private void OnShowAds(UnityAction action)
    {
        Service.Get<AdsManager>()?.ShowRewardedAd(() => { SetLevelUpReward(action); });
    }

    public void SetRelicReward(UnityAction action, bool isReRoll = false)
    {
        StartPopup("Stage Clear!", "강화 효과를 선택하세요.\n해당 효과는 <color=red>영구적</color>으로 적용됩니다.", isReRoll);
        CloseCallback = action;
        var CurrentReward = Service.Get<RelicManager>()?.GetStageRandomRewards();
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetReward(CurrentReward[i], Service.Get<RelicManager>().OnSelectRelicReward, i);
        }

        if (!Service.Get<AdsManager>().IsAdUsed)
        {
            reRollButton.interactable = true;
            reRollButton.onClick.AddListener(()=>Service.Get<AdsManager>().ShowRewardedAd(() => { SetRelicReward(action, true); }));    
        }
        else
        {
            reRollButton.interactable = false;
        }
    }

    public void AddListener(UnityAction<bool> action)
    {
        isOpenRewardPopup.AddListener(action);
    }

    public void RemoveListener(UnityAction<bool> action)
    {
        isOpenRewardPopup.RemoveListener(action);
    }

    [SerializeField] private GameObject maintenance;
    [SerializeField] private Button repairRampart;
    [SerializeField] private Button randomReward;

    public void SetMaintenanceReward(UnityAction repair, UnityAction randomReward)
    {
        if (maintenance != null) maintenance.SetActive(true);

        repairRampart.onClick.RemoveAllListeners();
        repairRampart.onClick.AddListener(() =>
        {
            maintenance.SetActive(false);
            repair?.Invoke();
        });

        this.randomReward.onClick.RemoveAllListeners();
        this.randomReward.onClick.AddListener(() =>
        {
            maintenance.SetActive(false);
            randomReward?.Invoke();
        });
    }
}