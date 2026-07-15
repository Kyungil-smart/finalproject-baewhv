using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

public class GameManager : BaseManager<GameManager>
{
    #region State

    private StateMachine _state;
    [field: SerializeField] public ObserveValue<GameState> CurrentState { get; private set; }

    private UnityAction _endNarrativeAction;
    public StoryStageRawData beforeStageData { get; private set; }
    public StoryStageRawData currentStageData { get; private set; }

    public ReadyState ReadyState { get; protected set; }
    public SortState SortState { get; protected set; }
    public WaveState WaveState { get; protected set; }
    public ClearState ClearState { get; protected set; }
    public GameOverState GameOverState { get; protected set; }
    public NarrativeState NarrativeState { get; protected set; }

    #endregion

    private int _currentChapter = 1;
    private int _currentStage = 1;

    public int MaxChapter { get; private set; }
    public int MaxStage { get; private set; }
    private bool _isGameAllClear; 
    
    public int CurrentChapter
    {
        get => _currentChapter;
        private set => _currentChapter = value;
    }

    public int CurrentStage
    {
        get => _currentStage;
        private set
        {
            _currentStage = value;
            OnStageChange?.Invoke(value);
        }
    }
    public UnityAction<int> OnStageChange;

    public bool isLoading = false;
    private bool isReady = false;

    public Rampart _wall;
    private string _wallAddress = "Rampart";
    [field: SerializeField] public RatioIntValue CurrentHp { get; set; }

    public HashSet<string> ids = new HashSet<string>();

    private void Awake()
    {
        base.Awake();

        if (IsManagerDestroy) return;

        isReady = true;

        _state = new();
        CurrentState = new();
        CurrentHp = new RatioIntValue(-1);
        
        var rampartData = Service.Get<DataManager>()?.StaticValueTable.data.Find(x => x.VARIABLE_NAME == "CASTLE_HP");
        if (rampartData != null)
        {
            if (int.TryParse(rampartData.VARIABLE_VALUE, out int value))
            {
                CurrentHp = new RatioIntValue(value);
                CurrentHp.Value = CurrentHp.MaxValue;
            }
        }

        ReadyState = new(this);
        SortState = new(this);
        WaveState = new(this);
        ClearState = new(this);
        GameOverState = new(this);
        NarrativeState = new(this);

        LoadSaveGame();

        CheckMaxStage();
    }

    private void OnEnable()
    {
        if (isReady && !IsManagerDestroy)
        {
            CurrentState.AddListener(ChangeState);
        }
    }

    private void OnDisable()
    {
        if (isReady && !IsManagerDestroy)
        {
            CurrentState?.RemoveListener(ChangeState);
            CurrentHp?.RemoveListener(WallHpChange);
        }
    }
    
    private void Update()
    {
        _state?.Update();
    }

    private void ChangeState(GameState state)
    {
        switch (state)
        {
            case GameState.Ready:
                _state.ChangeState(ReadyState);
                break;
            case GameState.Sort:
                _state.ChangeState(SortState);
                break;
            case GameState.Wave:
                _state.ChangeState(WaveState);
                break;
            case GameState.Clear:
                _state.ChangeState(ClearState);
                break;
            case GameState.GameOver:
                _state.ChangeState(GameOverState);
                break;
            case GameState.Narrative:
                _state.ChangeState(NarrativeState);
                break;
        }
    }

    public int ChangeSpeed()
    {
        int gameSpeed = Service.Get<TimeManager>().ChangeSpeed();
        return gameSpeed;
    }

    private EStageType CheckStageType(StoryStageRawData data)
    {
        string tableType = data.STAGE_TYPE.ToUpper();
        if (tableType == "TUTORIAL") return EStageType.TUTORIAL;
        else if (tableType == "EVENT") return EStageType.EVENT;
        else if (tableType == "NORMAL_F") return EStageType.NORMAL_F;
        else if (tableType == "MAINTENANCE") return EStageType.MAINTENANCE;
        else if (tableType == "BOSS_F") return EStageType.BOSS_F;
        else return EStageType.BOSS_F;
    }

    private StageState CurrentStageState(int chapter, int stage, EStageType type)
    {
        if (chapter < _currentChapter || (chapter == _currentChapter && stage < _currentStage)) return StageState.Clear;

        if (chapter > _currentChapter || (chapter == _currentChapter && stage > _currentStage))
        {
            switch (type)
            {
                case EStageType.EVENT:
                    return StageState.LockSpecial;
                case EStageType.MAINTENANCE:
                    return StageState.LockSpecial;
                case EStageType.BOSS_F:
                    return StageState.LockBoss;
                default:
                    return StageState.Lock;
            }
        }

        switch (type)
        {
            case EStageType.EVENT:
                return StageState.OpenSpecial;
            case EStageType.MAINTENANCE:
                return StageState.OpenSpecial;
            case EStageType.BOSS_F:
                return StageState.OpenBoss;
            default:
                return StageState.Current;
        }
    }


    private void WallHpChange(int hp)
    {
        if (hp <= 0 && CurrentState.Value != GameState.GameOver)
        {
            CurrentState.Value = GameState.GameOver;
        }
    }

    private void LoadSaveGame()
    {
        var dataManager = Service.Get<DataManager>();
        if (dataManager == null) return;

        var saveData = dataManager.LoadSaveData;

        var rampartData = dataManager.StaticValueTable.data.Find(x => x.VARIABLE_NAME == "CASTLE_HP");
        if (rampartData != null && int.TryParse(rampartData.VARIABLE_VALUE, out var hp))
        {
            CurrentHp.MaxValue = hp;
        }
        
        if (saveData == null)
        {
            CurrentChapter = 1;
            CurrentStage = 1;
            CurrentHp.Value = CurrentHp.MaxValue;
            return;
        }

        CurrentChapter = saveData.chapter;
        CurrentStage = saveData.stage;

        if (saveData.wallMaxHp > 0) CurrentHp.MaxValue = saveData.wallMaxHp;
        if (saveData.wallHp > 0) CurrentHp.Value = saveData.wallHp;
        
        CurrentHp.Value = saveData.wallHp;
    }

    public void SaveGame(Dictionary<string, int> rewardDatas)
    {
        Service.Get<DataManager>()?.SaveGameData(CurrentChapter, CurrentStage, CurrentHp.Value, CurrentHp.MaxValue, rewardDatas);
    }

    public void EnterStage(int chapter, int stage)
    {
        _currentChapter = chapter;
        _currentStage = stage;

        if (_currentChapter == MaxChapter && _currentStage == MaxStage)
        {
            Service.Get<ResourcesManager>()?.LoadVideo("Player/HCD_Staffroll");
        }

        isLoading = true;

        CurrentState.Value = GameState.Ready;

        var stageStoryData = Service.Get<DataManager>()?.StoryStageTable.data
            .FirstOrDefault(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);

        if (stageStoryData == null) return;

        var type = CheckStageType(stageStoryData);

        if (type != EStageType.NORMAL_F && type != EStageType.BOSS_F && type != EStageType.TUTORIAL)
        {
            ids.Clear();
            CheckAndStartNarrative(stageStoryData, true, () => NextStageScene(stageStoryData, type));
            return;
        }

        List<MapRawData> currentStage = Service.Get<DataManager>()?.MapTable.data
            .FindAll(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);

        if (currentStage == null)
        {
            _currentChapter = 1;
            _currentStage = 1;

            currentStage = Service.Get<DataManager>()?.MapTable.data
                .FindAll(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);

            if (currentStage == null) return;
        }
        else
        {
            _currentChapter = chapter;
            _currentStage = stage;
        }

        ids = new HashSet<string>();

        if (currentStage != null)
        {
            foreach (var data in currentStage)
            {
                if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_01)) ids.Add(data.SPAWN_MONSTER_ID_01.Trim());
                if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_02)) ids.Add(data.SPAWN_MONSTER_ID_02.Trim());
                if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_03)) ids.Add(data.SPAWN_MONSTER_ID_03.Trim());
                if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_04)) ids.Add(data.SPAWN_MONSTER_ID_04.Trim());
                if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_05)) ids.Add(data.SPAWN_MONSTER_ID_05.Trim());
                if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_06)) ids.Add(data.SPAWN_MONSTER_ID_06.Trim());
                if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_07)) ids.Add(data.SPAWN_MONSTER_ID_07.Trim());
            }
        }

        CheckAndStartNarrative(stageStoryData, true, () => NextStageScene(stageStoryData, type));
    }

    private void NextStageScene(StoryStageRawData stageStoryData, EStageType type)
    {
        isLoading = true;

        CurrentState.Value = GameState.Ready;

        Service.Get<TimeManager>()?.ResetTimeScale();

        switch (type)
        {
            case EStageType.TUTORIAL:
                Service.Get<SceneController>()?.ChangeScene(SceneType.Tutorial);
                break;
            case EStageType.NORMAL_F:
                Service.Get<SceneController>()?.ChangeScene(SceneType.InGame);
                break;
            case EStageType.EVENT:
                break;
            case EStageType.MAINTENANCE:
                break;
            case EStageType.BOSS_F:
                Service.Get<SceneController>()?.ChangeScene(SceneType.InGame);
                break;
        }
    }

    public void CheckAndStartNarrative(StoryStageRawData stageStoryData, bool isBefore, UnityAction action)
    {
        _endNarrativeAction = action;
        if (stageStoryData != null && !string.IsNullOrEmpty(stageStoryData.STORY_ID))
        {
            Debug.Log("SetNarrative");
            CurrentState.Value = GameState.Narrative;
            Service.Get<NarrativeManager>()?.StartNarrative(stageStoryData, isBefore);
        }
        else NarrativeEnd();
    }

    public void CheckAndSaveBestStage()
    {
        Service.Get<DataManager>()?.CheckAndSaveBestStage(CurrentChapter, CurrentStage);
    }

    private void CheckMaxStage()
    {
        var stages = Service.Get<DataManager>()?.StoryStageTable.data;
        if (stages == null || stages.Count == 0) return;
        
        MaxChapter = stages.Max(x => x.CHAPTER);
        MaxStage = stages.Where(x => x.CHAPTER == MaxChapter).Max(x => x.STAGE);
    }

    public void SpawnWall()
    {
        if (_wall != null) return;

        Addressables.LoadAssetAsync<GameObject>(_wallAddress).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject wallPrefab = handle.Result;
                GameObject wallObj = Instantiate(wallPrefab);

                if (wallObj.TryGetComponent(out Rampart wall))
                {
                    _wall = wall;

                    if (Service.Get<RelicManager>() != null)
                    {
                        float maxHpPlus = Service.Get<RelicManager>().GetTotalRelicBonus("CASTLE", "MAX_HP");
                        CurrentHp.MaxValue += (int)maxHpPlus;
                    }

                    _wall.SetHp(CurrentHp);
                    
                    CurrentHp.RemoveListener(WallHpChange);
                    CurrentHp.AddListener(WallHpChange);
                }
            }
        };
    }

    public void RepairRampart()
    {
        var repairData = Service.Get<DataManager>()?.StaticValueTable.data.Find(x => x.VARIABLE_NAME == "CASTLE_HP_RECOVERY");

        if (repairData != null)
        {
            string value = repairData.VARIABLE_VALUE;
            
            CurrentHp.Value += int.Parse(value);
            if (CurrentHp.Value > CurrentHp.MaxValue) CurrentHp.Value = CurrentHp.MaxValue;
        }
    }

    public void ClearStage()
    {
        beforeStageData = Service.Get<DataManager>()?.StoryStageTable.data
            .FirstOrDefault(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);

        if (_wall != null)
        {
            Addressables.ReleaseInstance(_wall.gameObject);
            _wall = null;
        }

        bool isEndChapter = (beforeStageData != null && CheckStageType(beforeStageData) == EStageType.BOSS_F);
        
        CheckAndSaveBestStage();

        if (_currentChapter == MaxChapter && _currentStage == MaxStage)
        {
            Service.Get<ResourcesManager>()?.LoadVideo("Player/HCD_Staffroll");
            SaveGame(Service.Get<RelicManager>()?.MyRelics);
            _isGameAllClear = true;
        }

        NextStage();

        currentStageData = Service.Get<DataManager>()?.StoryStageTable.data
            .FirstOrDefault(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);

        bool isBattle = true;

        if (currentStageData != null)
        {
            EStageType nextEStageType = CheckStageType(currentStageData);
            if (nextEStageType == EStageType.EVENT || nextEStageType == EStageType.MAINTENANCE) isBattle = false;
        }

        if (_isGameAllClear) Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.OnNextButton(false);
        if (isEndChapter) Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.OnNextButton(false);
        else Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.OnNextButton(isBattle);

        CurrentState.Value = GameState.Clear;
    }

    private void NextStage()
    {
        var stageStoryData = Service.Get<DataManager>()?.StoryStageTable.data.Where(x => x.CHAPTER == _currentChapter)
            .ToList();
        if (stageStoryData != null)
        {
            int maxStage = stageStoryData.Max(x => x.STAGE);

            if (_currentStage >= maxStage)
            {
                _currentChapter++;
                CurrentStage = 1;
            }
            else CurrentStage++;
        }
    }

    public void NextBattle()
    {
        var stageStoryData = Service.Get<DataManager>()?.StoryStageTable.data
            .FirstOrDefault(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);
        
        if (stageStoryData != null)
        {
            var type = CheckStageType(stageStoryData);
            if (type == EStageType.NORMAL_F || type == EStageType.BOSS_F)
                CheckAndStartNarrative(stageStoryData, false, () => { NextStageScene(stageStoryData, type); });
            else
                NextStageScene(stageStoryData, type);
        }
    }

    public void EndStage()
    {
        CurrentState.Value = GameState.GameOver;

        if (_wall != null)
        {
            Addressables.ReleaseInstance(_wall.gameObject);
            _wall = null;
        }

        Service.Get<TimeManager>()?.ResetTimeScale();
    }

    public void RestartStage()
    {
        if (_wall != null)
        {
            Addressables.ReleaseInstance(_wall.gameObject);
            _wall = null;
        }

        Service.Get<TimeManager>()?.ResetTimeScale();

        int targetChapter = _currentChapter;
        int targetStage = _currentStage;
        
        LoadSaveGame();
        
        _currentChapter = targetChapter;
        _currentStage = targetStage;

        EnterStage(_currentChapter, _currentStage);
    }

    public void NarrativeEnd()
    {
        if (_isGameAllClear)
        {
            _isGameAllClear = false;
            _endNarrativeAction = null;

            StaffRollPlay();
            return;
        }
        
        _endNarrativeAction?.Invoke();
        _endNarrativeAction = null;
    }

    private void StaffRollPlay()
    {
        VideoClip staffRoll = Service.Get<ResourcesManager>()?.GetVideo("Player/HCD_Staffroll");
        
        Service.Get<VideoManager>().PlayVideo(staffRoll, () =>
        {
            Service.Get<TimeManager>()?.ResetTimeScale();
            Service.Get<SceneController>()?.ChangeScene(SceneType.Title);
        });
    }
}

public enum GameState
{
    Narrative,
    Ready,
    Wave,
    Sort,
    Clear,
    GameOver
}

public struct StageData
{
    public int Stage;
    public StageState State;
    public EStageType type;
}