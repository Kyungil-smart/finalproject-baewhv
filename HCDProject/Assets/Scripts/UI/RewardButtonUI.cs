using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RewardButtonUI : RewardIconUI
{
    [SerializeField] private Color SelectedColor;
    [SerializeField] private Color DefalutColor = Color.white;
    private Image Background;
    public int GetIndex { get; private set; }

    private bool isSelected;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            Background.color = value ? SelectedColor : DefalutColor;
        }
    }

    private UnityAction<int> buttonAction;

    private void Awake() => Background = GetComponent<Image>();


    public void SetReward(StageClearRewardRawData data, UnityAction<int> func, int _index, bool isContain = false)
    {
        SetResource(data.CLEAR_REWARD_ICON, data.CLEAR_REWARD_NAME,
            isContain ? data.CLEAR_REWARD_TEXT_ID_02 : data.CLEAR_REWARD_TEXT_ID_01);
        if (isContain)
            SetCount(data.CLEAR_REWARD_S_01, data.CLEAR_REWARD_S_02, data.CLEAR_REWARD_S_03);
        else
            SetCount(data.CLEAR_REWARD_F_01, data.CLEAR_REWARD_F_02, data.CLEAR_REWARD_F_03);
        buttonAction = func;
        GetIndex = _index;
        IsSelected = false;
    }

    public void SetReward(LevelRewardRawData data, UnityAction<int> func, int _index)
    {
        SetResource(data.LEVEL_REWARD_ICON, data.LEVEL_REWARD_NAME, data.LEVEL_REWARD_TEXT_ID);
        SetCount(data.LEVEL_REWARD_01, data.LEVEL_REWARD_02, 0);
        buttonAction = func;
        GetIndex = _index;
        IsSelected = false;
    }


    public void OnButtonInvoke()
    {
        buttonAction(GetIndex);
    }

    public void CopyElement(RewardButtonUI ui)
    {
        ui.RewardNameText.text = RewardNameText.text;
        ui.RewardDescText.text = RewardDescText.text;
        ui.icon.sprite = icon.sprite;
    }
}