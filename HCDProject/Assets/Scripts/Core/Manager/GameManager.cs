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
    
    public bool isLoading = false;
    private bool isReady = false;
    
    public Rampart _wall;
    private string _wallAddress = "Rampart";
    [SerializeField] private int _currentWallHp = -1;
    private Coroutine _gameRoutine;
    private CharacterRawData _characterRawData;

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
                Service.Get<DataManager>()?.SelectStageReward(randomRewards[1].CLEAR_REWARD_ID);
                Debug.Log($"{randomRewards[1].CLEAR_REWARD_ID}");
            }
    }

    List<StageClearRewardRawData> randomRewards;

    public void OpenRewardUi()
    {
        randomRewards = Service.Get<DataManager>().GetStageRandomRewards();
        Debug.Log($"뽑힌 카드 {randomRewards.Count}개");

        for (int i = 0; i < randomRewards.Count; i++)
        {
            string rewardId = randomRewards[i].CLEAR_REWARD_ID;
            int currentCount = Service.Get<DataManager>().CurrentStageRewardCount(rewardId);
            int maxCount = randomRewards[i].MAX_CLEAR_REWARD_COUNT;
            Debug.Log($"리워드 {i} : {rewardId}: {currentCount} / {maxCount}");
            
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

    public List<StageData> GetStageDataList(int currentChapter)
    {
        var stageData = Service.Get<DataManager>()?.MapTable.data.Where(x => x.CHAPTER == currentChapter).Select(x => x.STAGE).Distinct().OrderBy(stage => stage).ToList();
        
        int bossStageInChapter = 0;
        if (stageData != null && stageData.Count > 0)
        {
            bossStageInChapter  = stageData.Max();
        }

        List<StageData> uiList = new();

        if (stageData != null)
        {
            foreach (var stageIndex in stageData)
            {
                uiList.Add(new StageData {Stage = stageIndex , State = CurrentStageState(currentChapter, stageIndex, bossStageInChapter)});
            }
        }
        return uiList;
    }
    
    
    private StageState CurrentStageState(int chapter, int stage, int bossStage)
    {
        if (chapter > _currentChapter || (chapter == _currentChapter && stage > _currentStage)) return StageState.Lock;
        else if (chapter < _currentChapter || (chapter == _currentChapter && stage < _currentStage)) return StageState.Clear;
        else if (stage == bossStage) return StageState.Boss;
        
        return StageState.Current;
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

        SpawnWall();
        
        List<MapRawData> currentStage = Service.Get<DataManager>()?.MapTable.data.FindAll(x => x.CHAPTER == _currentChapter && x.STAGE == _currentStage);
        
        ids = new HashSet<string>();
        
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
        
        if (_wall != null)
        {
            _currentWallHp = _wall.CurrentHp.Value;
            
            var wallHpUi = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();
            
            if (wallHpUi != null)
            {
                //_wall.CurrentHp.RemoveListener(wallHpUi.SetWallHP);
            }
            
            Addressables.ReleaseInstance(_wall.gameObject);
            _wall = null;
        }
        
        CurrentState.Value = GameState.Clear;
    }

    private void NextStage()
    {
        var stageData = Service.Get<DataManager>()?.MapTable.data.Where(x => x.CHAPTER == _currentChapter).Select(x => x.STAGE).Distinct().ToList();
        if (stageData != null)
        {
            int maxStage = stageData.Max();

            if (_currentStage >= maxStage)
            {
                _currentChapter++;
                _currentStage = 1;
            }
            else _currentStage++;
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
}