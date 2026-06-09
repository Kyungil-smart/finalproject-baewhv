using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RewardUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private RewardButtonUI[] buttonList;
    [SerializeField] private Button reRollButton;
    private int selectedIndex = -1;
    
    private UnityAction CloseCallback;

    private void OnDisable()
    {
        CloseCallback.Invoke();
        CloseCallback = null;
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

    public void SetLevelUpReward(UnityAction action)
    {
        gameObject.SetActive(true);
        selectedIndex = -1;
        titleText.text = "Level UP!";
        contentText.text = "강화 효과를 선택하세요.\n해당 효과는 이번 노드에서만 적용됩니다.";
        CloseCallback = action;
        var CurrentReward = Service.Get<PlayerManager>()?.GetLevelUpRewards();
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetReward(CurrentReward[i], Service.Get<PlayerManager>().OnSelectLevelUpReward, i);
        }
    }

    public void SetRelicReward(UnityAction action)
    {
        titleText.text = "Stage Clear!";
        contentText.text = "강화 효과를 선택하세요.\n해당 효과는 <color=red>영구적</color>으로 적용됩니다.";
        CloseCallback = action;
        var CurrentReward = Service.Get<RelicManager>()?.GetStageRandomRewards();
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetReward(CurrentReward[i], Service.Get<RelicManager>().OnSelectRelicReward, i);
        }
    }
}