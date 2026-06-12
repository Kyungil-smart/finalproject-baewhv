using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : BaseManager<GameManager>
{
    #region State
    private StateMachine _state;
    [field:SerializeField] public ObserveValue<GameState> CurrentState { get; private set; }

    public ReadyState ReadyState { get; protected set; }
    public SortState SortState { get; protected set; }
    public WaveState WaveState { get; protected set; }
    public ClearState ClearState { get; protected set; }
    public GameOverState GameOverState { get; protected set; }
    #endregion
    
    private int _currentChapter = 1;
    private int _currentStage = 1;
    
    public int CurrentChapter {get => _currentChapter; private set => _currentChapter = value; }
    public int CurrentStage { get => _currentStage; private set => _currentStage = value; }

    private int currentSpeedIndex = 0;
    private float[] timeScaleSpeeds = { 1f, 2f, 3f };
    
    public bool isLoading = false;
    private bool isReady = false;
    
    public Rampart _wall;
    private string _wallAddress = "Rampart";
    [SerializeField] private int _currentWallHp = -1;
    
    public HashSet<string> ids = new HashSet<string>();
    
    private void Awake()
    {
        if (Service.Get<GameManager>() != null && Service.Get<GameManager>() != this)
        {
            Destroy(gameObject);
            return;
        }

        isReady = true;
        
        base.Awake();

        _state = new();
        CurrentState = new();
        
        ReadyState = new(this);
        SortState = new(this);
        WaveState = new(this);
        ClearState = new(this);
        GameOverState = new(this);

        _currentWallHp = -1;
    }

    private void OnEnable()
    {
        if (isReady)
        {
            CurrentState.AddListener(ChangeState);
            CurrentState.Value = GameState.Ready;
        }
    }

    private void OnDisable()
    {
        CurrentState.RemoveListener(ChangeState);
        if (_wall != null && _wall.CurrentHp != null) _wall.CurrentHp.RemoveListener(WallHpChange);
    }

    private void Update()
    {
        _state?.Update();

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                ClearStage();
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                NarrativeEnd();
            }

            if (Keyboard.current.uKey.wasPressedThisFrame)
            {
                _wall.SetDamage(10);
            }
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
        }
    }

    public int ChangeSpeed()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % timeScaleSpeeds.Length;
        
        float gameSpeed = timeScaleSpeeds[currentSpeedIndex];
        
        Service.Get<TimeManager>().SetSpeed(gameSpeed);
        
        Debug.Log($"현재 타임스케일 {Time.timeScale}");
        
        return (int)gameSpeed;
    }

    public List<StageData> GetStageDataList(int currentChapter)
    {
        var stageStoryData = Service.Get<DataManager>()?.StoryStageTable.data.Where(x => x.CHAPTER == currentChapter).OrderBy(x => x.STAGE).ToList();
        
        List<StageData> uiList = new();

        if (stageStoryData != null)
        {
            foreach (var data in stageStoryData)
            {
                var type = CheckStageType(data);
                
                uiList.Add(new StageData
                {
                    Stage = data.STAGE,
                    State = CurrentStageState(currentChapter, data.STAGE, type),
                    type =  type
                });
            }
        }
        return uiList;
    }

    public void NarrativeEnd()
    {
        var stageStoryData = Service.Get<DataManager>()?.StoryStageTable.data.FirstOrDefault(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);

        if (stageStoryData == null) return;
        
        var type = CheckStageType(stageStoryData);

        if (type == StageType.Event || type == StageType.Maintenance)
        {
            Service.Get<SceneController>()?.ChangeScene(SceneType.StageSelect);
            Service.Get<UIManager>()?.GetUI<StageSelectUIController>()?.ShowReward(type);
        }
        else NextStageScene(null, type);
    }

    private StageType CheckStageType(StoryStageRawData data)
    {
        string tableType = data.STAGE_TYPE.ToUpper();
        if (tableType == "TUTORIAL") return StageType.Tutorial;
        else if (tableType == "EVENT") return StageType.Event;
        else if (tableType == "NORMAL_F") return StageType.Normal;
        else if (tableType == "MAINTANANCE") return StageType.Maintenance;
        else if (tableType == "BOSS_F") return StageType.Boss;
        else return StageType.Normal;
    }
    
    private StageState CurrentStageState(int chapter, int stage, StageType type)
    {
        if (chapter < _currentChapter || (chapter == _currentChapter && stage < _currentStage)) return StageState.Clear;

        if (chapter > _currentChapter || (chapter == _currentChapter && stage > _currentStage))
        {
            switch (type)
            {
                case StageType.Event:
                    return StageState.LockSpecial;
                case StageType.Maintenance:
                    return StageState.LockSpecial;
                case StageType.Boss:
                    return StageState.LockBoss;
                default:
                    return StageState.Lock;
            }
        }
        
        switch (type)
        {
            case StageType.Event:
                return StageState.OpenSpecial;
            case StageType.Maintenance:
                return StageState.OpenSpecial;
            case StageType.Boss:
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
    
    public void EnterStage(int chapter, int stage)
    {
        _currentChapter = chapter;
        _currentStage = stage;
        
        isLoading = true;

        CurrentState.Value = GameState.Ready;
        
        var stageStoryData = Service.Get<DataManager>()?.StoryStageTable.data.FirstOrDefault(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);

        if (stageStoryData == null) return;
        
        var type = CheckStageType(stageStoryData);

        if (type != StageType.Normal && type != StageType.Boss && type != StageType.Tutorial)
        {
            ids.Clear();
            NextStageScene(stageStoryData, type);
            return;
        }
        
        List<MapRawData> currentStage = Service.Get<DataManager>()?.MapTable.data.FindAll(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);
        
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
        NextStageScene(stageStoryData, type);
    }

    private void NextStageScene(StoryStageRawData stageStoryData, StageType type)
    {
        isLoading = true;
        CurrentState.Value = GameState.Ready;
        
        if (stageStoryData != null && !string.IsNullOrEmpty(stageStoryData.STORY_ID))
        {
            Service.Get<SceneController>()?.ChangeScene(SceneType.Narrative);
        }
        else
        {
            switch (type)
            {
                case StageType.Tutorial:
                    Service.Get<SceneController>()?.ChangeScene(SceneType.Tutorial);
                    SpawnWall();
                    break;
                case StageType.Normal:
                    Service.Get<SceneController>()?.ChangeScene(SceneType.InGame);
                    SpawnWall();
                    break;
                case StageType.Event:
                    break;
                case StageType.Maintenance:
                    break;
                case StageType.Boss:
                    Service.Get<SceneController>()?.ChangeScene(SceneType.InGame);
                    SpawnWall();
                    break;
            }
        }
    }
    
    public void SpawnWall()
    {
        Addressables.InstantiateAsync(_wallAddress).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject wallObj = handle.Result;

                if (wallObj.TryGetComponent(out Rampart wall))
                {
                    _wall = wall;

                    if (_currentWallHp != -1) _wall.SetHp(_currentWallHp);
                    else _currentWallHp = _wall.CurrentHp.MaxValue;
                    
                    if (_wall.CurrentHp != null) _wall.CurrentHp.AddListener(WallHpChange);

                    var wallHpUi = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();

                    if (wallHpUi != null && _wall != null)
                    {
                        wallHpUi.SetWallHP(_wall.CurrentHp.Value);
                        _wall.CurrentHp.AddRatioListener(wallHpUi.SetWallHP);
                    }
                }
            }
        };
    }

    public void ClearStage()
    {
        NextStage();
        
        var nextStageData = Service.Get<DataManager>()?.StoryStageTable.data.FirstOrDefault(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);

        bool isBattle = true;

        if (nextStageData != null)
        {
            StageType nextStageType = CheckStageType(nextStageData);
            if (nextStageType == StageType.Event || nextStageType == StageType.Maintenance)
            {
                isBattle = false;
            }
        }
        
        if (_wall != null)
        {
            _currentWallHp = _wall.CurrentHp.Value;
            
            Addressables.ReleaseInstance(_wall.gameObject);
            _wall = null;
        }

        Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.OnNextButton(isBattle);
        
        CurrentState.Value = GameState.Clear;
    }

    private void NextStage()
    {
        var stageStoryData = Service.Get<DataManager>()?.StoryStageTable.data.Where(x => x.CHAPTER == _currentChapter).ToList();
        if (stageStoryData != null)
        {
            int maxStage = stageStoryData.Max(x => x.STAGE);

            if (_currentStage >= maxStage)
            {
                _currentChapter++;
                _currentStage = 1;
            }
            else _currentStage++;
        }
    }

    public void NextBattle()
    {
        var stageStoryData = Service.Get<DataManager>()?.StoryStageTable.data.FirstOrDefault(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);

        if (stageStoryData != null)
        {
            var type = CheckStageType(stageStoryData);
            
            NextStageScene(stageStoryData, type);
        }
    }

    public void EndStage()
    {
        CurrentState.Value = GameState.GameOver;
    }
}

public enum GameState
{
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
    public StageType type;
}