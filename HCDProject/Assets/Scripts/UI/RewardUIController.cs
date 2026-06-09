using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Button[] buttonList;
    [SerializeField] private Button reRollButton;

    public void SetLevelUpReward(LevelRewardRawData[] datas)
    {
        ClearEvent();
        titleText.text = "Level UP!";
        contentText.text = "강화 효과를 선택하세요.\n해당 효과는 이번 노드에서만 적용됩니다.";
        for (int i = 0; i < buttonList.Length; i++)
        {
            
        }
    }

    public void SetRelicReward(StageClearRewardRawData[] datas)
    {
        ClearEvent();
        titleText.text = "Stage Clear!";
        contentText.text = "강화 효과를 선택하세요.\n해당 효과는 <color=red>영구적</color>으로 적용됩니다.";
        for (int i = 0; i < buttonList.Length; i++)
        {
            
        }
    }

    private void ClearEvent()
    {
        foreach (Button button in buttonList)
        {
            button.onClick.RemoveAllListeners();
        }
    }

}