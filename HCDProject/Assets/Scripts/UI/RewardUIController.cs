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
    
    public ObserveValue<bool> isOpenRewardPopup = new();

    private void OnDisable()
    {
        isOpenRewardPopup.Value = false;
        CloseCallback?.Invoke();
        CloseCallback = null;
        Service.Get<TimeManager>().LoadTimeScale();
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

    private void StartPopup(string title, string content)
    {
        gameObject.SetActive(true);
        isOpenRewardPopup.Value = true;
        titleText.text = title;
        contentText.text = content;
        Service.Get<TimeManager>().SaveTimeScale();
    }
    

    public void SetLevelUpReward(UnityAction action)
    {
        StartPopup("Level UP!", "강화 효과를 선택하세요.\n해당 효과는 이번 노드에서만 적용됩니다.");
        CloseCallback = action;
        var CurrentReward = Service.Get<PlayerManager>()?.GetLevelUpRewards();
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetReward(CurrentReward[i], Service.Get<PlayerManager>().OnSelectLevelUpReward, i);
        }
    }

    public void SetRelicReward(UnityAction action)
    {
        StartPopup("Stage Clear!", "강화 효과를 선택하세요.\n해당 효과는 <color=red>영구적</color>으로 적용됩니다.");
        CloseCallback = action;
        var CurrentReward = Service.Get<RelicManager>()?.GetStageRandomRewards();
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetReward(CurrentReward[i], Service.Get<RelicManager>().OnSelectRelicReward, i);
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
}