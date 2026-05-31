using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    private bool isLoading = false;
    
    public GameState currentState = GameState.Ready;
    private float sortTime = 3;
    private int totalWave = 3;
    private Rampart _wall;
    private Coroutine _gameRoutine;

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
    }

    private void Start()
    {
        Service.Get<DataManager>()?.InitData(()=>{Debug.Log("초기 데이터 받기 성공");});
    }

    private void OnEnable()
    {
        CurrentState.AddListener(ChangeState);

        CurrentState.Value = GameState.Ready;
    }

    private void OnDisable()
    {
        CurrentState.RemoveListener(ChangeState);
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
        }
    }
    
    public void EnterStage(int chapter, int stage)
    {
        isLoading = true;
        
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

    public void Spawn(int chapter, int stage, int wave)
    {
        if (isLoading) return;
        
        MapRawData waveData = Service.Get<DataManager>()?.MapTable.data.Find(x => x.CHAPTER == chapter  && x.STAGE == stage && x.WAVE == wave);
        if (waveData == null) return;

        if (!string.IsNullOrEmpty(waveData.SPAWN_MONSTER_ID_01))
        {
            string address = waveData.SPAWN_MONSTER_ID_01.Trim();
            GameObject prefab = Service.Get<MonsterManager>().GetMonsterPrefab(address);
            
            for (int i = 0; i < waveData.SPAWN_MONSTER_COUNT_01; i++)
            {
                GameObject obj = Instantiate(prefab, UnityEngine.Random.insideUnitSphere * 3f, Quaternion.identity);
                
                MonsterRawData stat = Service.Get<DataManager>()?.MonsterTable.data.Find(x => x.MONSTER_ID == waveData.SPAWN_MONSTER_ID_01.Trim());
                obj.AddComponent<MonsterStatus>().InitStatus(stat);
            }
        }

        if (!string.IsNullOrEmpty(waveData.SPAWN_MONSTER_ID_02))
        {
            string address = waveData.SPAWN_MONSTER_ID_02.Trim();
            GameObject prefab = Service.Get<MonsterManager>().GetMonsterPrefab(address);

            for (int i = 0; i < waveData.SPAWN_MONSTER_COUNT; i++)
            {
                GameObject obj = Instantiate(prefab, UnityEngine.Random.insideUnitSphere * 3f, Quaternion.identity);
                
                MonsterRawData stat = Service.Get<DataManager>()?.MonsterTable.data.Find(x => x.MONSTER_ID == waveData.SPAWN_MONSTER_ID_02.Trim());
                obj.AddComponent<MonsterStatus>().InitStatus(stat);
            }
        }
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