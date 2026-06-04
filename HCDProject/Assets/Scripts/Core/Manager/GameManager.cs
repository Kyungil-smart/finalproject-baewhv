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
    
    public bool isLoading = false;
    
    private float sortTime = 3;
    private int totalWave = 3;
    public Rampart _wall;
    private string _wallAddress = "Rampart";
    [SerializeField] private int _currentWallHp = -1;
    private Coroutine _gameRoutine;
    private CharacterRawData _characterRawData;

    private void Awake()
    {
        Service.Get<SceneController>()?.CreateSession();
        
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
        CurrentState.AddListener(ChangeState);

        CurrentState.Value = GameState.Ready;
    }

    private void OnDisable()
    {
        CurrentState.RemoveListener(ChangeState);
        _wall.CurrentHp.RemoveListener(WallHpChange);
    }

    private void Update()
    {
        _state?.Update();

        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            var character = Service.Get<DataManager>()?.CharacterTable.data.Find(x => x.CHARACTER_ID == "3000");
        
            if (character != null) Debug.Log($"id : {character.CHARACTER_ID}, name : {character.CHARACTER_NAME} , atk : {character.ATK}");
            
            var stageMonster = Service.Get<DataManager>()?.MapTable.data.FindAll(x => x.CHAPTER == 1 && x.STAGE == 1);

            if (stageMonster != null)
            {
                foreach (var stage in stageMonster)
                {
                    Debug.Log($"stageMonster : {stage.SPAWN_MONSTER_ID_01}, {stage.SPAWN_MONSTER_ID_02}, {stage.SPAWN_MONSTER_COUNT_03}");
                }
            }
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
    
    
    private void WallHpChange(int hp)
    {
        if (hp <= 0 && CurrentState.Value != GameState.GameOver)
        {
            CurrentState.Value = GameState.GameOver;
        }
    }
    
    public void EnterStage(int chapter, int stage)
    {
        isLoading = true;

        SpawnWall();
        
        List<MapRawData> currentStage = Service.Get<DataManager>()?.MapTable.data.FindAll(x => x.CHAPTER == chapter && x.STAGE == stage);
        
        HashSet<string> ids = new HashSet<string>();
        
        foreach (var data in currentStage)
        {
            if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_01)) ids.Add(data.SPAWN_MONSTER_ID_01.Trim());
            if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_02)) ids.Add(data.SPAWN_MONSTER_ID_02.Trim());
        }
        
        Service.Get<MonsterManager>()?.StageMonster(new List<string>(ids), () =>
        {
            isLoading = false;
        
            foreach (var id in ids)
            {
                Debug.Log($"로딩 성공 : {id}");
            }
        });
    }
    
    
    private void SpawnWall()
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
                    else _currentWallHp = _wall.CurrentHp.Value;
                    
                    if (_wall.CurrentHp != null) _wall.CurrentHp.AddListener(WallHpChange);

                    var wallHpUi = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();

                    // if (wallHpUi != null)
                    // {
                    //     wallHpUi.SetWallHP(_wall.currentHp.Value, _wall.MaxHp);
                    //     _wall.currentHp.AddListener(wallHpUi.UpdateWallHP);
                    // }
                }
            }
        };
    }

    public void ClearStage()
    {
        if (_wall != null)
        {
            _currentWallHp = _wall.CurrentHp.Value;
            
            _wall.CurrentHp.RemoveListener(WallHpChange);
            
            var wallHpUi = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();
            if (wallHpUi != null)
            {
                // _wall.currentHp.RemoveListener(wallHpUi.UpdateWallHP);
            }
            

            Addressables.ReleaseInstance(_wall.gameObject);
            _wall = null;
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