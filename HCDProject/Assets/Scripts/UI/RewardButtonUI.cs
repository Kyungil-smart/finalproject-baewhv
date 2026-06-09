using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI RewardName;
    [SerializeField] private TextMeshProUGUI RewardDesc;
    [SerializeField] private Sprite defaultImage;
    public int GetIndex { get; private set; }
    public bool IsSelected { get; set; }
    private UnityAction<int> buttonAction;
    private Button rewardButton;

    private void Awake()
    {
        rewardButton = GetComponent<Button>();
    }

    public void SetReward(StageClearRewardRawData data, UnityAction<int> func, int _index)
    {
        //todo : SpriteAtlas 도입할것
        icon.sprite = Addressables.LoadAssetAsync<Sprite>(data.CLEAR_REWARD_ICON).WaitForCompletion();
        if (!icon.sprite)
            icon.sprite = defaultImage;
        //todo dictionary로 해야하지 않을까?
        RewardName.text = Service.Get<DataManager>()?.LocalizingTable.data
            .Find(x => x.TEXT_ID == data.CLEAR_REWARD_NAME).Korean;
        buttonAction = func;
        GetIndex = _index;
        IsSelected = false;
    }

    public void SetReward(LevelRewardRawData data, UnityAction<int> func, int _index)
    {
        icon.sprite = Addressables.LoadAssetAsync<Sprite>(data.LEVEL_REWARD_ICON).WaitForCompletion();
        if (!icon.sprite)
            icon.sprite = defaultImage;
        RewardName.text = Service.Get<DataManager>()?.LocalizingTable.data
            .Find(x => x.TEXT_ID == data.LEVEL_REWARD_NAME).Korean;
        buttonAction = func;
        GetIndex = _index;
        IsSelected = false;
    }

    public void OnButtonInvoke()
    {
        buttonAction(GetIndex);
    }
}