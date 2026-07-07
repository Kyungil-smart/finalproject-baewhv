using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class RewardButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private LocalizeStringEvent RewardName;
    [SerializeField] private TextMeshProUGUI RewardNameText;
    [SerializeField] private LocalizeStringEvent RewardDesc;
    [SerializeField] private TextMeshProUGUI RewardDescText;
    [SerializeField] private Sprite defaultImage;
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


    public void SetReward(StageClearRewardRawData data, UnityAction<int> func, int _index)
    {
        LoadIcon(data.CLEAR_REWARD_ICON);
        RewardName.SetEntry(data.CLEAR_REWARD_NAME);
        RewardDesc.SetEntry(data.CLEAR_REWARD_TEXT_ID_01);
        buttonAction = func;
        GetIndex = _index;
        IsSelected = false;
    }

    public void SetReward(LevelRewardRawData data, UnityAction<int> func, int _index)
    {
        LoadIcon(data.LEVEL_REWARD_ICON);

        RewardName.SetEntry(data.LEVEL_REWARD_NAME);
        RewardDesc.SetEntry(data.LEVEL_REWARD_TEXT_ID);
        buttonAction = func;
        GetIndex = _index;
        IsSelected = false;
    }

    private void LoadIcon(string address)
    {
        icon.sprite = defaultImage;
        if (string.IsNullOrEmpty(address)) return;
        Addressables.LoadAssetAsync<Sprite>(address).Completed += (handle) =>
        {
            //if(!icon) return;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                icon.sprite = handle.Result;
            }
        };
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