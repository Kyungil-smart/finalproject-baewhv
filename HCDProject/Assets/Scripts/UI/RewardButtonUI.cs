using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class RewardButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private LocalizeStringEvent RewardName;
    [SerializeField] private LocalizeStringEvent RewardDesc;
    [SerializeField] private Sprite defaultImage;
    public int GetIndex { get; private set; }
    public bool IsSelected { get; set; }
    private UnityAction<int> buttonAction;

    public void SetReward(StageClearRewardRawData data, UnityAction<int> func, int _index)
    {
        //todo : SpriteAtlas 도입할것
        icon.sprite = Addressables.LoadAssetAsync<Sprite>(data.CLEAR_REWARD_ICON).WaitForCompletion();
        if (!icon.sprite)
            icon.sprite = defaultImage;
        RewardName.SetEntry(data.CLEAR_REWARD_NAME);
        buttonAction = func;
        GetIndex = _index;
        IsSelected = false;
    }

    public void SetReward(LevelRewardRawData data, UnityAction<int> func, int _index)
    {
        icon.sprite = Addressables.LoadAssetAsync<Sprite>(data.LEVEL_REWARD_ICON).WaitForCompletion();
        if (!icon.sprite)
            icon.sprite = defaultImage;
        RewardName.SetEntry(data.LEVEL_REWARD_NAME);
        buttonAction = func;
        GetIndex = _index;
        IsSelected = false;
    }

    public void OnButtonInvoke()
    {
        buttonAction(GetIndex);
    }
}