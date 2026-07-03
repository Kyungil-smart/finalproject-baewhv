using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class StageSelectUIController : BaseUIController<StageSelectUIController>
{
    [SerializeField] private List<Button> _stageButtons = new();
    [SerializeField] private List<Sprite> stageSprites;
    [SerializeField] private GameObject popupObject;
    [SerializeField] private RewardUIController rewardPopup;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private TMP_Text popupTypeText;
    [SerializeField] private Button continuePopup;
    [SerializeField] private Text continueText;
    [SerializeField] private Button cancelPopup;
    [SerializeField] private Color LockColor;
    [FormerlySerializedAs("OpenColor")] [SerializeField] private Color ClearColor;
    private int _currentChapter;
    public void SetStageMap()
    {
        var stageData = Service.Get<GameManager>()?.GetStageDataList(_currentChapter);
       
        if (stageData == null) return;
        
        int loopCount = Mathf.Min(_stageButtons.Count, stageData.Count);

        for (int i = 0; i < loopCount; i++)
        {
            var data = stageData[i];
            Button stageButton = _stageButtons[i];
            Image stageImage = stageButton.GetComponent<Image>();
            TextMeshProUGUI stageText = stageButton.GetComponentInChildren<TextMeshProUGUI>();

            if (stageText != null) stageText.text = $"{_currentChapter} - {data.Stage}";
            
            stageButton.interactable = data.State == StageState.Current || data.State == StageState.OpenBoss || data.State == StageState.OpenSpecial;

            if (stageImage != null)
            {
                stageImage.sprite = stageSprites[(int)data.State];
                if (data.State == StageState.Clear)
                    stageImage.color = ClearColor;
                    
            }
            stageButton.onClick.RemoveAllListeners();
            stageButton.onClick.AddListener(() => OnClickStageButton(_currentChapter, data.Stage, data.type));
        }
    }

   


    private void OnClickStageButton(int chapter, int stage, StageType type)
    {
        if (type == StageType.NORMAL_F || type == StageType.BOSS_F)
        {
            if (popupObject != null)
            {
                popupObject.SetActive(true);
                
                if (popupTypeText != null) popupTypeText.text = $"{type.ToString()}";

                if (cancelPopup != null) cancelPopup.gameObject.SetActive(true);

                if (continuePopup != null)
                {
                    if (continueText != null) continueText.text = "start";
                }
                
                if (popupText != null) popupText.text = $"{chapter} - {stage} start?";

                if (continuePopup != null)
                {
                    continuePopup.onClick.RemoveAllListeners();
                    continuePopup.onClick.AddListener(() =>
                    {
                        popupObject.SetActive(false);
                        Service.Get<GameManager>()?.EnterStage(chapter, stage);
                    });
                }
            }
        }
        else
        {
            if (popupTypeText != null) popupTypeText.text = $"{type.ToString()}";
            
            Service.Get<GameManager>()?.EnterStage(chapter, stage);

            if (type == StageType.EVENT)
            {
                if (rewardPopup != null)
                {
                    rewardPopup.SetRelicReward(OnRewardSelect);
                }
            }
            else if (type == StageType.MAINTENANCE)
            {
                if (rewardPopup != null)
                {
                     rewardPopup.SetMaintenanceReward(RepairRampart, RandomReward);
                }
            }
        }
    }

    private void RepairRampart()
    {
        if (popupObject != null)
        {
            popupObject.SetActive(true);
            if (popupText != null) popupText.text = "성벽 체력 회복";
            
            if (cancelPopup != null) cancelPopup.gameObject.SetActive(false);

            if (continuePopup != null)
            {
                if (continueText != null) continueText.text = "continue";
                
                continuePopup.onClick.RemoveAllListeners();
                continuePopup.onClick.AddListener(() =>
                {
                    popupObject.SetActive(false);
                    Service.Get<GameManager>()?.RepairRampart();
                    Service.Get<GameManager>()?.ClearStage();
                    //StageMap();
                });
            }
        }
    }

    private void RandomReward()
    {
        if (rewardPopup != null)
        {
            rewardPopup.SetRelicReward(OnRewardSelect);
        }
    }
    
    public void OnRewardSelect()
    {
        if (popupObject != null)
        {
            popupObject.SetActive(true)
                ;
            if (popupText != null) popupText.text = "유물 획득";

            if (cancelPopup != null) cancelPopup.gameObject.SetActive(false);
            
            if (continuePopup != null)
            {
                if (continueText != null) continueText.text = "continue";
                
                continuePopup.onClick.RemoveAllListeners();
                continuePopup.onClick.AddListener(() => 
                {
                    popupObject.SetActive(false);
                    Service.Get<GameManager>()?.ClearStage();
                    //StageMap();
                });
            }
        }
    }

    public void ShowReward(StageType type)
    {
        if (popupTypeText != null) popupTypeText.text = type == StageType.EVENT ? "EVENT" : "MAINTENANCE";

        if (rewardPopup != null)
        {
            if (type == StageType.EVENT) rewardPopup.SetRelicReward(OnRewardSelect);
            // else if (type == StageType.Maintenance) rewardPopup.SetMaintenanceReward(RepairRampart, RandomReward);
        }
    }
    
    public void OnOpenSettingUI()
    {
        Service.Get<UIManager>()?.OpenOption();
    }
    
}

public enum StageState
{
    Lock,
    Clear,
    Current,
    LockSpecial,
    OpenSpecial,
    LockBoss,
    OpenBoss
}


