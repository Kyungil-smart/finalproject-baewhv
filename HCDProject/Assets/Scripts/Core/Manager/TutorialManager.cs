using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Tutorial;
using Unity.VisualScripting;
using UnityEngine.Localization.Components;

public class TutorialManager : BaseManager<TutorialManager>
{
    [SerializeField] private GameObject touchShield;
    [SerializeField]private Image background;
    [SerializeField] private GameObject touchField;
    [SerializeField] private TutorialScriptsSO scripts;
    [SerializeField] private RectTransform arrowUI;
    [SerializeField] private Transform uiStorage;
    [SerializeField] private TutorialMessageBox message;

    [SerializeField] private GameObject centerMessageObject;
    [SerializeField] private LocalizeStringEvent centerMessage;

    private int currentWave;
    private BaseCharacter _baseCharacter;

    private Dictionary<string, TutorialRawData> datas = new();
    private Dictionary<string, TutorialHelper> HighLightUIList = new();
    private Dictionary<string, UnityAction<string>> category = new();
    public UnityAction nextActions;

    private HashSet<string> tutorialSettingDone = new ();

    private GameState currentState;
    private UnityEvent<ETutorialOccur, int> startPoint = new();

    private string targetName;
    private int targetID;
    private bool findBoss;

    public bool pauseWave { get; private set; }
    public bool pauseClear { get; private set; }

    private void Start()
    {
        foreach (TutorialRawData d in Service.Get<DataManager>().TutorialTable.data)
        {
            datas[d.TUTORIAL_ID] = d;
            ETutorialOccur type = Enum.Parse<ETutorialOccur>(d.OCCUR);
            if (type != ETutorialOccur.NONE)
            {
                startPoint.AddListener((t, v) =>
                {
                    string id = d.TUTORIAL_ID;
                    AddTutorialStartPoint(t, v, id);
                });
                if (type == ETutorialOccur.BOSS_ENTRESS)
                {
                    targetID = d.OCCUR_VALUE;
                    targetName = Service.Get<DataManager>().MonsterTable.data
                        .Find(x => x.MONSTER_ID == targetID.ToString()).MONSTER_NAME;
                }
            }
        }

        Service.Get<GameManager>()?.CurrentState.AddListener(OnChangeGameStateType);
        Service.Get<MonsterSpawnManager>()?.currentWave.AddListener(OnChangeWave);
        Service.Get<UIManager>().GetUI<IngamePopupController>().GetRewardPopup.AddListener(OpenLevelUpPopup);
        Service.Get<UIManager>().GetUI<IngamePopupController>().GetClearPopup.AddListener(OpenResultPopup);
        Service.Get<UIManager>().GetUI<IngameBottomUIController>().HideSkill();
        Service.Get<UIManager>().GetUI<IngameBottomUIController>().isSortMode.AddListener(OnSortEnd);
        
        
        
        //Service.Get<UIManager>().GetUI<IngameBottomUIController>().isSortMode.AddListener();
        category["DIM_ON"] = DimOn;
        category["DIM_OFF"] = DimOff;
        category["SHOW_ARROW"] = ShowArrow;
        category["HIDE_ARROW"] = HideArrow;
        category["CLEAR_HIGHLIGHT"] = DeleteAllGlow;
        category["REMOVE_HIGHLIGHT"] = RemoveGlow;
        category["ADD_STONE_SLOT"] = AddStoneSlot;
        category["PAUSE_TIME"] = PauseTime;
        category["RESUME_TIME"] = ResumeTime;
        category["SET_LEVELUP_REWARD"] = SetLevelUpReward;
        category["SET_RELIC_REWARD"] = SetRelicReward;
        category["SPAWN_CHARACTER"] = SetSpawnCharacter;
        category["SELECT_REWARD"] = SelectReward;
        category["PAUSE_WAVE"] = PauseWave;
        category["RESUME_WAVE"] = ResumeWave;
        category["SHOW_SKILL"] = ShowSkill;
        category["USE_SKILL"] = UseSkill;
        category["PAUSE_CLEAR"] = PauseClear;
        category["RESUME_CLEAR"] = ResumeClear;
        category["SET_STONE"] = SetStone;
    }

    private void Update()
    {
        if (!_baseCharacter || currentState != GameState.Wave || findBoss) return;
         List<Collider2D> Colliders = new List<Collider2D>(); 
         int count = Physics2D.OverlapCircle(_baseCharacter.transform.position,
             2.0f, _baseCharacter.EnemyFilter, Colliders);
         for (int i = 0; i < count; i++)
         {
             if (!Colliders[i].TryGetComponent(out ITargetable target)) continue;
             if (target.GetTargetObject.name == targetName)
             {
                 startPoint?.Invoke(ETutorialOccur.BOSS_ENTRESS, targetID);
                 findBoss = true;
             }
         }
         
    }

    #region BindListener

    private void OnChangeGameStateType(GameState state)
    {
        currentState = state;
        switch (state)
        {
            case GameState.Sort:
                startPoint?.Invoke(ETutorialOccur.SORT_ENTER, currentWave);
                break;
            case GameState.Wave:
                startPoint?.Invoke(ETutorialOccur.WAVE_ENTER, currentWave);
                break;
            case GameState.Clear:
                startPoint?.Invoke(ETutorialOccur.STAGE_CLEAR, 0);
                break;
        }
        if(!_baseCharacter)
            _baseCharacter = Service.Get<PlayerManager>().Characters[0];
    }

    private void OpenLevelUpPopup(bool isActive)
    {
        if (currentState == GameState.Clear || currentState == GameState.Narrative)
        {
            startPoint?.Invoke(ETutorialOccur.RELIC_REWARD, 0);
        }
        else
        {
            startPoint?.Invoke(ETutorialOccur.LEVEL_UP, Service.Get<PlayerManager>().GetLevel);
        }
    }

    private void OpenResultPopup(bool isActive)
    {
        if (isActive)
            startPoint?.Invoke(ETutorialOccur.RESULT, 0);
    }

    private void OnChangeWave(int value)
    {
        currentWave = value;
    }

    private void OnSortEnd(bool value)
    {
        if (!value)
        {
            Debug.Log($"sort end{currentWave}");
            startPoint?.Invoke(ETutorialOccur.SORT_END, currentWave);
        }
    }

    #endregion


    private void AddTutorialStartPoint(ETutorialOccur type, int occur, string entry)
    {
        var e = Enum.Parse<ETutorialOccur>(datas[entry].OCCUR);
        if (type != Enum.Parse<ETutorialOccur>(datas[entry].OCCUR)) return;

        if (occur == datas[entry].OCCUR_VALUE)
            StartTutorial(entry);
    }

    private void StartTutorial(string entry)
    {
        if(!tutorialSettingDone.Add(entry)) return;

        touchShield.SetActive(true);
        var data = datas[entry];
        
        
        
        Debug.Log(entry);

        string[] highlight = data.HIGHLIGHT.Split(';');
        foreach (var str in highlight)
        {
            if (string.IsNullOrEmpty(str)) continue;
            GameObject go = null;
            int.TryParse(str.Substring(str.Length - 1),out var num);
            if (str.Contains("CHARACTER_SLOT"))
                go = Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetSlots[num].gameObject;
            else if (str.Contains("CHARACTER_SORT_SLOT"))
                go = Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetSlots[num].GetStones;
            else if (str.Contains("STONE"))
                go = Service.Get<SortManager>().GetRailA[num].gameObject;
            else if (str == "COMBO_TEXT")
                go = Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetComboText;
            else if (str == "START_BUTTON")
                go = Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetStartButton;
            else if (str == "REWARD_SLOT_0")
                go = Service.Get<UIManager>().GetUI<IngamePopupController>().GetRewardPopup.GetButtonUI.gameObject;
            CopyNGlow(str, go, str.Contains("STONE"));
        }
        
        string[] categories = data.CATEGORY.Split(';');
        string[] categorie_values = data.CATEGORY_VALUE.Split(';');
        for (int i = 0; i < categorie_values.Length; i++)
        {
            if (category.ContainsKey(categories[i]))
            {
                category[categories[i]].Invoke(categorie_values[i]);
            }
        }

        if (string.IsNullOrEmpty(data.MESSAGE_TYPE))
        {
            HideMessage();
        }
        else
        {
            switch (Enum.Parse<ETutorialMessageType>(data.MESSAGE_TYPE))
            {
                case ETutorialMessageType.CENTER:
                    ShowCenterMessage(data.TEXT_ID);
                    break;
                case ETutorialMessageType.MESSAGE_TOP:
                    ShowMessage(data.NAME_ID, data.TEXT_ID);
                    break;
                case ETutorialMessageType.MESSAGE_BOTTOM:
                    ShowMessage(data.NAME_ID, data.TEXT_ID, false);
                    break;
            }
        }


        SetNextTutorial(data.NEXT_ID, Enum.Parse<ETutorialNextType>(data.NEXT_TYPE), data.NEXT_TYPE_VALUE);
    }

    private void SetNextTutorial(string next, ETutorialNextType type, string value)
    {
        Debug.Log($"{next} / {type} / {value}");
        if (string.IsNullOrEmpty(next))
        {
            nextActions = () =>
            {
                touchShield.SetActive(false);   
                DeleteAllGlow(null);
                HideMessage();
                Debug.Log("endTuto");
            };
        }
        else nextActions = () => StartTutorial(next);
        string[] key = value.Split(',');  
        switch (type)
        {
            case ETutorialNextType.TOUCH:
                touchField.SetActive(true);
                break;
            case ETutorialNextType.DRAG_STONE:
                HighLightUIList[key[0]].GetComponent<DragAndDropTutorial>().SetStartKey(key[0]);
                HighLightUIList[key[1]].GetComponent<TutorialHelper>().SetTargetKey(key[0]);
                break;
            case ETutorialNextType.TOUCH_HIGHLIGHT:
                var reward = HighLightUIList[value].GetComponent<RewardButtonUI>();
                reward.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (!reward.IsSelected)
                        reward.IsSelected = true;
                    else
                        reward.OnButtonInvoke();
                    nextActions?.Invoke();
                });
                break;
            case ETutorialNextType.NONE:
                touchShield.SetActive(false);   
                DeleteAllGlow(null);
                HideMessage();
                Debug.Log("NONE");
                break;
        }
    }

    private void DimOn(string temp)
    {
        background.DOFade(0.75f, 1).SetUpdate(true);
    }

    private void DimOff(string temp)
    {
        background.DOFade(0f, 1).SetUpdate(true);
    }

    private void AddStoneSlot(string temp)
    {
        string[] objects = temp.Split(',');
        
        DragAndDrop stone = HighLightUIList[objects[0]].RawObject.GetComponent<DragAndDrop>();
        GameObject slotRaw = HighLightUIList[objects[1]].RawObject;
        CharacterSlotUI slot = slotRaw.GetComponent<CharacterSlotUI>();
        Service.Get<SortManager>().ObjectDrop(slot, stone);
        DeleteAllGlow(objects[0]);
        HideArrow(null);
        CopyNGlow(objects[1], slotRaw);
    }


    private void CopyNGlow(string name, GameObject raw, bool isStone = false)
    {
        GameObject clone = Instantiate(raw, uiStorage, false);
        clone.GetComponent<Button>()?.onClick.RemoveAllListeners();
        RectTransform rawRt = (RectTransform)raw.transform;
        RectTransform rt = (RectTransform)clone.transform;
        rt.position = rawRt.position;
        rt.sizeDelta = rawRt.sizeDelta;
        rt.localScale = rawRt.localScale;
        HighLightUIList[name] = clone.GetComponent<TutorialHelper>();
        if (isStone)
            HighLightUIList[name].gameObject.AddComponent<DragAndDropTutorial>();
        HighLightUIList[name].SetTutorial(raw); 
        HighLightUIList[name].StartGlow();
    }

    private void RemoveGlow(string glowName)
    {
        if (!HighLightUIList.ContainsKey(glowName)) return;
        if (HighLightUIList[glowName].gameObject)
        {
            Destroy(HighLightUIList[glowName].gameObject);
        }

        HighLightUIList.Remove(glowName);
    }

    private void DeleteAllGlow(string t)
    {
        foreach (var temp in HighLightUIList.Values)
        {
            temp.gameObject.SetActive(false);
            Destroy(temp.gameObject);
        }
        HighLightUIList.Clear();
        foreach (Transform var in uiStorage)
        {
            Debug.Log(var.name);
            Destroy(var.gameObject);
        }
    }

    private Tween currentTween;


    private void ShowArrow(string data)
    {
        string[] objects = data.Split(',');
        Vector2 startPos = ((RectTransform)HighLightUIList[objects[0]].transform).localPosition;
        Vector2 endPos = ((RectTransform)HighLightUIList[objects[1]].transform).localPosition;
        arrowUI.gameObject.SetActive(true);
        arrowUI.anchoredPosition = (startPos + endPos)*0.5f;
        
        Vector2 dir = endPos - startPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrowUI.localRotation = Quaternion.Euler(0, 0, angle);

        float length = Vector2.Distance(startPos, endPos);
        arrowUI.sizeDelta = new Vector2(length, arrowUI.sizeDelta.y);

        currentTween = arrowUI.GetComponent<Image>().DOFade(0, 0.5f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        foreach (var temp in arrowUI.GetComponents<TutorialHelper>())
        {
            temp.StartGlow();
        }
    }

    private void HideArrow(string temp)
    {
        currentTween.Kill();
        arrowUI.gameObject.SetActive(false);
    }

    private void PauseTime(string temp)
    {
        Service.Get<TimeManager>().SaveTimeScale();
    }

    private void ResumeTime(string temp)
    {
        Service.Get<TimeManager>().LoadTimeScale();
    }

    private void PauseWave(string temp)
    {
        pauseWave = true;
    }

    private void ResumeWave(string temp)
    {
        pauseWave = false;
        Service.Get<SortManager>()?.FinishSortPhase();
    }

    private void PauseClear(string temp)
    {
        pauseClear = true;
    }

    private void ResumeClear(string temp)
    {
        pauseClear = false;
    }

    private void SetStone(string temp)
    {
        List<int> sortList = new List<int>{ 1, 0, 1, 0, 1, 0, 3, 2, 0, 1, 2, 3 };
        Service.Get<SortManager>().TutorialBlocks(sortList);
    }

    private void ShowSkill(string temp)
    {
        Service.Get<UIManager>()?.GetUI<IngameBottomUIController>().ShowSkill();
    }

    private void UseSkill(string temp)
    {
        int index = int.Parse(temp);
        _baseCharacter.TryUseActiveSkill();
    }

    private void SetLevelUpReward(string temp)
    {
        string[] key = temp.Split(',');

        var popup = HighLightUIList[key[1]].GetComponent<RewardButtonUI>(); 
        LevelRewardRawData d = Service.Get<DataManager>().LevelRewardTable.data.Find(x => x.LEVEL_ID == key[0]);
        popup.SetReward(d, x =>
        {
            Service.Get<PlayerManager>().ApplyLevelReward(d);
            Service.Get<DataManager>()?.SelectLevelReward(key[0]);
        }, 0);
    }

    private void SetRelicReward(string temp)
    {
        string[] key = temp.Split(',');
        
        var popup = HighLightUIList[key[1]].GetComponent<RewardButtonUI>(); 
        StageClearRewardRawData d = Service.Get<DataManager>().StageClearRewardTable.data
            .Find(x => x.CLEAR_REWARD_ID == key[0]);
        popup.SetReward(d, x =>
        {
            Debug.Log("SetReward");
            Service.Get<DataManager>()?.SelectStageReward(key[0]);
            Service.Get<GameManager>()?.SaveGame(Service.Get<RelicManager>()?.MyRelics);
        }, 0);
        ClearPopupUI clearPopup =Service.Get<UIManager>().GetUI<IngamePopupController>().GetClearPopup;
        popup.CopyElement(clearPopup.GetSelectedRelic);
    }

    private void SetSpawnCharacter(string temp)
    {
        int index = int.Parse(temp);
        Service.Get<PlayerManager>().SpawnSingleCharacter(index);
    }

    private void SelectReward(string temp)
    {
        int index = int.Parse(temp);
        Debug.Log("selected");
        //Service.Get<UIManager>().GetUI<IngamePopupController>().GetRewardPopup.OnButtonSelected(index);
    }

    private void ShowMessage(string name, string desc, bool isTopPosition = true)
    {
        Debug.Log("message");
        message.gameObject.SetActive(true);
        message.SetMessage(name, desc, isTopPosition);
    }

    private void HideMessage()
    {
        message.gameObject.SetActive(false);
    }

    private void ShowCenterMessage(string desc)
    {
        centerMessageObject.SetActive(true);
        centerMessage.SetEntry(desc);
        StartCoroutine(HideCenterMessage());
    }

    private IEnumerator HideCenterMessage()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        centerMessageObject.SetActive(false);
    }

    public void OnTouchNext()
    {
        touchField.SetActive(false);
        nextActions?.Invoke();
    }
}

namespace Tutorial
{
    public enum ETutorialNextType
    {
        NONE,
        TOUCH,
        DRAG_STONE,
        TOUCH_HIGHLIGHT
    }

    public enum ETutorialMessageType
    {
        CENTER,
        MESSAGE_TOP,
        MESSAGE_BOTTOM,
    }

    public enum ETutorialOccur
    {
        NONE,
        SORT_ENTER,
        SORT_END,
        WAVE_ENTER,
        LEVEL_UP,
        BOSS_ENTRESS,
        STAGE_CLEAR,
        RELIC_REWARD,
        RESULT
    }
}