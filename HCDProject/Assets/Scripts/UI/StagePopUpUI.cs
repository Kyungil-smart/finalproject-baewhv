using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class StagePopUpUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private LocalizeStringEvent stageTypeText;
    [SerializeField] private GameObject imageLayout;
    [SerializeField] private Image stageImage;
    [SerializeField] private Sprite tempCharacterImage;
    [SerializeField] private Sprite tempStageImage;
    [SerializeField] private Sprite tempBossImage;

    [SerializeField] private TextMeshProUGUI eventTopText;
    [SerializeField] private TextMeshProUGUI eventBottomText;
    
    [SerializeField] private GameObject bossInfoLayout;
    [SerializeField] private Image bossSprite;
    
    [SerializeField] private GameObject maintenanceLayout;
    [SerializeField] private GameObject relicLayout;
    [SerializeField] private GameObject hpGaugeLayout;
        
    [SerializeField] private GameObject buttonLayout;
    [SerializeField] private Button positiveButton;
    [SerializeField] private LocalizeStringEvent positiveText;
    [SerializeField] private Button negativeButton;
    [SerializeField] private LocalizeStringEvent negativeText;


    private StoryStageRawData data;

    public void InitLayout()
    {
        imageLayout.SetActive(false);
        buttonLayout.SetActive(false);
        maintenanceLayout.SetActive(false);
        relicLayout.SetActive(false); 
        eventTopText.gameObject.SetActive(false);
        eventBottomText.gameObject.SetActive(false);
        hpGaugeLayout.SetActive(false);
        bossInfoLayout.SetActive(false);
    }

    public void OpenStagePopup(StoryStageRawData _data)
    {
        data = _data;
        InitLayout();
        stageText.text = $"Stage {_data.CHAPTER} - {_data.STAGE}";
        stageTypeText.SetEntry($"UI_STAGE_{_data.STAGE_TYPE}");
        
        StageType type = Enum.Parse<StageType>(_data.STAGE_TYPE);
        switch (type)
        {
            case StageType.NORMAL_F:
            case StageType.TUTORIAL:
                SetNormal();
                break;
            case StageType.EVENT:
                SetEvent();
                break;
            case StageType.MAINTENANCE:
                SetMaintenance();
                break;
            case StageType.BOSS_F:
                SetBoss();
                break;
        }
    }
    

    private void SetEvent()
    {
        imageLayout.SetActive(true);
        stageImage.sprite = tempCharacterImage;
        
    }

    private void SetMaintenance()
    {
        maintenanceLayout.SetActive(true);
        bossInfoLayout.SetActive(true);
        var subData = Service.Get<DataManager>().StoryStageTable.data
            .Find(x => x.CHAPTER == data.CHAPTER && x.STAGE == data.STAGE + 1);
        Addressables.LoadAssetAsync<Sprite>(subData.BOSS_MONSTER_IMG_ID).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                bossSprite.sprite = handle.Result;
            }
        };
    }

    private void SetNormal()
    {
        imageLayout.SetActive(true);
        stageImage.sprite = tempStageImage;
        hpGaugeLayout.SetActive(true);
        SetBottomButton(StageType.NORMAL_F, () => { }, () => { });
    }

    private void SetBoss()
    {
        imageLayout.SetActive(true);
        stageImage.sprite = tempBossImage;
        hpGaugeLayout.SetActive(true);
        SetBottomButton(StageType.BOSS_F, () => { }, () => { });
    }

    private void SetRelic(bool isPresent)
    {
        maintenanceLayout.SetActive(false);
        bossInfoLayout.SetActive(false);
    }

    private void SetBottomButton(StageType type, UnityAction positive, UnityAction negative)
    {
        buttonLayout.SetActive(true);
        positiveButton.onClick.RemoveAllListeners();
        negativeButton.onClick.RemoveAllListeners();
        positiveButton.onClick.AddListener(positive);
        negativeButton.onClick.AddListener(negative);
        positiveText.SetEntry(type == StageType.EVENT ? "UI_POPUP_CHECK" : "UI_POPUP_START");
        positiveText.SetEntry("UI_POPUP_BACK");
    }
}

public enum StageType
{
    TUTORIAL,
    NORMAL_F,
    EVENT,
    MAINTENANCE,
    BOSS_F
}