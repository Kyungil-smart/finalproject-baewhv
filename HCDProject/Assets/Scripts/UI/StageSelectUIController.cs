using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageSelectUIController : BaseUIController<StageSelectUIController>
{
    [SerializeField] private List<Button> _stageButtons = new();

    [SerializeField] private GameObject popupObject;
    [SerializeField] private RewardUIController rewardPopup;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private TMP_Text popupTypeText;
    [SerializeField] private Button continuePopup;
    [SerializeField] private Text continueText;
    [SerializeField] private Button cancelPopup;

    private int _currentChapter;

    private void Start()
    {
        _currentChapter = Service.Get<GameManager>().CurrentChapter;
        
        if (popupObject != null) popupObject.SetActive(false);
        
        if (cancelPopup != null)
        {
            cancelPopup.onClick.RemoveAllListeners();
            cancelPopup.onClick.AddListener(() => { popupObject.SetActive(false); });
        }
        StageMap();
    }

    public void StageMap()
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
                switch (data.State)
                {
                    case StageState.Lock:
                        stageImage.color = new Color(0.6f, 0f, 0f);
                        break;
                    case StageState.Clear:
                        stageImage.color = Color.blue;
                        break;
                    case StageState.Current:
                        stageImage.color = new Color(0.3f, 1f, 0.3f);
                        break;
                    case StageState.LockSpecial:
                        stageImage.color = Color.yellow;
                        break;
                    case StageState.OpenSpecial:
                        stageImage.color = Color.yellow;
                        break;
                    case StageState.LockBoss:
                        stageImage.color = new Color(0.86f, 0.63f, 0.86f);
                        break;
                    case StageState.OpenBoss:
                        stageImage.color = new Color(0.86f, 0.63f, 0.86f);
                        break;
                }
            }
            stageButton.onClick.RemoveAllListeners();
            stageButton.onClick.AddListener(() => OnClickStageButton(_currentChapter, data.Stage, data.type));
        }
    }


    private void OnClickStageButton(int chapter, int stage, StageType type)
    {
        if (type == StageType.Normal || type == StageType.Boss)
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

            if (type == StageType.Event)
            {
                if (rewardPopup != null)
                {
                    rewardPopup.SetRelicReward(OnRewardSelect);
                }
            }
            else if (type == StageType.Maintenance)
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
                    StageMap();
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
                    StageMap();
                });
            }
        }
    }

    public void ShowReward(StageType type)
    {
        if (popupTypeText != null) popupTypeText.text = type == StageType.Event ? "EVENT" : "MAINTENANCE";

        if (rewardPopup != null)
        {
            if (type == StageType.Event) rewardPopup.SetRelicReward(OnRewardSelect);
            // else if (type == StageType.Maintenance) rewardPopup.SetMaintenanceReward(RepairRampart, RandomReward);
        }
    }
    
    public void OnOpenSettingUI()
    {
        Service.Get<UIManager>()?.OpenOption();
    }
    
    // 추후 무한모드 제작시 이용 가능성 정도는 있음
    #region  챕터

    
    // [Header("챕터 설정")]
    // [SerializeField] private GameObject chapterSelectPrefab;
    // [SerializeField] private Transform chapterSelect;
    //
    // [Header("스테이지 설정")]
    // [SerializeField] private GameObject stageSelectPrefab;
    // [SerializeField] private Transform stageSelect;
    
    // public void ChapterButtons()
    // {
    //     if (chapterSelect != null) chapterSelect.gameObject.SetActive(true);
    //     if (stageSelect != null) stageSelect.gameObject.SetActive(false);
    //     
    //     foreach (Transform chapter in chapterSelect) DestroyImmediate(chapter.gameObject);
    //     
    //     var chapterList = Service.Get<DataManager>()?.MapTable.data.Select(x => x.CHAPTER).Distinct().OrderBy(chapter => chapter).ToList();
    //
    //     foreach (var chapter in chapterList)
    //     {
    //         GameObject chapterButtonObj = Instantiate(chapterSelectPrefab, chapterSelect);
    //         
    //         Button chapterButton = chapterButtonObj.GetComponent<Button>();
    //         Image chapterImage = chapterButtonObj.GetComponent<Image>();
    //         TextMeshProUGUI chapterText = chapterButtonObj.GetComponentInChildren<TextMeshProUGUI>();
    //
    //         if (chapterText != null) chapterText.text = $"{chapter} Chapter";
    //
    //         if (chapter > _currentChapter)
    //         {
    //             if (chapterButton != null) chapterButton.interactable = false;
    //             if (chapterImage != null) chapterImage.color = new Color(0.6f, 0f, 0f);
    //         }
    //         else if (chapter < _currentChapter)
    //         {
    //             if (chapterButton != null) chapterButton.interactable = true;
    //             if (chapterImage != null) chapterImage.color = Color.blue;
    //         }
    //         else
    //         {
    //             if (chapterButton != null) chapterButton.interactable = true;
    //             if (chapterImage != null) chapterImage.color = new Color(0.3f, 1f, 0.3f);
    //         }
    //         if (chapterButton != null)
    //         {
    //             chapterButton.onClick.RemoveAllListeners();
    //             chapterButton.onClick.AddListener(() => ShowStages(chapter));
    //         }
    //     }
    // }

    #endregion
    #region 스테이지

    // public void ShowStages(int chapter)
    // {
    //     _selectChapter = chapter;
    //     
    //     if (chapterSelect != null) chapterSelect.gameObject.SetActive(false);
    //     if (stageSelect != null) stageSelect.gameObject.SetActive(true);
    //     
    //     foreach (Transform stage in stageSelect) DestroyImmediate(stage.gameObject);
    //     
    //     var stageList = Service.Get<DataManager>()?.MapTable.data.Where(x => x.CHAPTER == chapter).Select(x => x.STAGE).Distinct().OrderBy(stage => stage).ToList();
    //
    //     int bossStageInChapter = 0;
    //     if (stageList != null &&  stageList.Count > 0)
    //     {
    //         bossStageInChapter = stageList.Max();
    //     }
    //
    //     foreach (var stage in stageList)
    //     {
    //         GameObject stageButtonObj = Instantiate(stageSelectPrefab, stageSelect);
    //         
    //         Button stageButton = stageButtonObj.GetComponent<Button>();
    //         Image stageImage = stageButtonObj.GetComponent<Image>();
    //         TextMeshProUGUI stageText = stageButtonObj.GetComponentInChildren<TextMeshProUGUI>();
    //         
    //         StageState state = CurrentStageState(chapter, stage, bossStageInChapter);
    //         
    //         if (stageText != null) stageText.text = $"{_selectChapter} - {stage} Stage";
    //
    //         if (stageButton != null)
    //         {
    //             stageButton.interactable = state != StageState.Lock;
    //
    //             if (stageImage != null)
    //             {
    //                 switch (state)
    //                 {
    //                     case StageState.Lock:
    //                         stageImage.color = new Color(0.6f, 0f, 0f);
    //                         break;
    //                     case StageState.Clear:
    //                         stageImage.color = Color.blue;
    //                         break;
    //                     case StageState.Current:
    //                         stageImage.color = new Color(0.3f, 1f, 0.3f);
    //                         break;
    //                     case StageState.Special:
    //                         stageImage.color = Color.yellow;
    //                         break;
    //                     case StageState.Boss:
    //                         stageImage.color = new Color(0.86f, 0.63f, 0.86f);
    //                         break;
    //                 }
    //             }
    //             
    //             stageButton.onClick.RemoveAllListeners();
    //             stageButton.onClick.AddListener(() => { _selectStage = stage; OnNextScene();});
    //         }
    //     }
    // }
    //
    // private StageState CurrentStageState(int chapter, int stage, int bossStage)
    // {
    //     if (chapter > _currentChapter || (chapter == _currentChapter && stage > _currentStage)) return StageState.Lock;
    //     else if (chapter < _currentChapter || (chapter == _currentChapter && stage < _currentStage)) return StageState.Clear;
    //     else if (stage == bossStage) return StageState.Boss;
    //     
    //     return StageState.Current;
    // }

    #endregion
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

public enum StageType
{
    Tutorial,
    Normal,
    Event,
    Maintenance,
    Boss
}
