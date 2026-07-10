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
    [SerializeField] private RewardUIController reward;
    [SerializeField] private GameObject hpGaugeLayout;

    [SerializeField] private GameObject buttonLayout;
    [SerializeField] private Button positiveButton;
    [SerializeField] private LocalizeStringEvent positiveText;
    [SerializeField] private Button negativeButton;
    [SerializeField] private LocalizeStringEvent negativeText;


    private StoryStageRawData data;

    public void InitLayout()
    {
        gameObject.SetActive(true);
        imageLayout.SetActive(false);
        buttonLayout.SetActive(false);
        maintenanceLayout.SetActive(false);
        relicLayout.SetActive(false);
        eventTopText.gameObject.SetActive(false);
        eventBottomText.gameObject.SetActive(false);
        hpGaugeLayout.SetActive(false);
        bossInfoLayout.SetActive(false);
    }

    public void OpenStagePopup(StoryStageRawData _data, EStageType type)
    {
        int currChapter = Service.Get<GameManager>().CurrentChapter;
        int currStage = Service.Get<GameManager>().CurrentStage;
        if (currStage != _data.STAGE || currChapter != _data.CHAPTER) return;

        data = _data;

        InitLayout();
        stageText.text = $"Stage {_data.CHAPTER} - {_data.STAGE}";
        stageTypeText.SetEntry($"UI_SS_TYPE_{_data.STAGE_TYPE}");

        switch (type)
        {
            case EStageType.NORMAL_F:
            case EStageType.TUTORIAL:
            default:
                SetNormal();
                break;
            case EStageType.EVENT:
                SetEvent();
                break;
            case EStageType.MAINTENANCE:
                SetMaintenance();
                break;
            case EStageType.BOSS_F:
                SetBoss();
                break;
        }
    }


    private void SetEvent()
    {
        imageLayout.SetActive(true);
        eventTopText.gameObject.SetActive(true);
        eventTopText.text = string.Format(eventTopText.text, "세라");
        eventBottomText.gameObject.SetActive(true);
        eventBottomText.text = string.Format(eventBottomText.text, "세라");
        stageImage.sprite = tempCharacterImage;
        stageImage.SetNativeSize();
        SetBottomButton(EStageType.EVENT,
            () =>
            {
                Service.Get<GameManager>().CheckAndStartNarrative(
                    Service.Get<GameManager>().currentStageData,
                    true, SetRelic);
            },
            () => { gameObject.SetActive(false); });
    }

    private void SetMaintenance()
    {
        maintenanceLayout.SetActive(true);
        bossInfoLayout.SetActive(true);
        hpGaugeLayout.SetActive(true);
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
        stageImage.SetNativeSize();
        hpGaugeLayout.SetActive(true);
        SetBottomButton(EStageType.NORMAL_F,
            () => { Service.Get<GameManager>().EnterStage(data.CHAPTER, data.STAGE); },
            () => { gameObject.SetActive(false); });
    }

    private void SetBoss()
    {
        imageLayout.SetActive(true);
        stageImage.sprite = tempBossImage;
        stageImage.SetNativeSize();
        hpGaugeLayout.SetActive(true);
        SetBottomButton(EStageType.BOSS_F,
            () => { Service.Get<GameManager>().EnterStage(data.CHAPTER, data.STAGE); },
            () => { gameObject.SetActive(false); });
    }

    private void SetRelic()
    {
        maintenanceLayout.SetActive(false);
        bossInfoLayout.SetActive(false);
        imageLayout.SetActive(false);
        buttonLayout.SetActive(false);
        relicLayout.SetActive(true);
        eventBottomText.gameObject.SetActive(false);
        reward.SetRelicReward(
            () =>
            {
                Service.Get<GameManager>()?.ClearStage();
            });
    }

    private void SetBottomButton(EStageType type, UnityAction positive, UnityAction negative)
    {
        buttonLayout.SetActive(true);
        positiveButton.onClick.RemoveAllListeners();
        negativeButton.onClick.RemoveAllListeners();
        positiveButton.onClick.AddListener(positive);
        negativeButton.onClick.AddListener(negative);
        positiveText.SetEntry(type == EStageType.EVENT ? "UI_POPUP_CHECK" : "UI_POPUP_START");
        negativeText.SetEntry("UI_POPUP_BACK");
    }

    public void SetWallHP(int current, int value)
    {
        
    }

    public void OnRepairWall()
    {
        Service.Get<GameManager>().CheckAndStartNarrative(
            Service.Get<GameManager>().currentStageData,
            true, () =>
            {
                Service.Get<GameManager>()?.RepairRampart();
                Service.Get<GameManager>()?.ClearStage();
                Service.Get<GameManager>()?.SaveGame(Service.Get<RelicManager>()?.MyRelics);
                gameObject.SetActive(false);                
            });
    }

    public void OnSelectRelic()
    {
        Service.Get<GameManager>().CheckAndStartNarrative(
            Service.Get<GameManager>().currentStageData,
            true, SetRelic);
    }
}