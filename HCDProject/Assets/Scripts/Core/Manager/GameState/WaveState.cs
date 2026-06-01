using UnityEngine;

public class WaveState : IState
{
    private GameManager _manager;
    private MonsterSpawnManager _spawnManager;
    private int totalWave = 3;
    
    public WaveState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
        _spawnManager = Service.Get<MonsterSpawnManager>();
        
        Service.Get<UIManager>().GetUI<IngameBottomUIController>().SetBattlePhase();
        
        Service.Get<GameManager>()?._wall.currentHp.AddListener(WallHpChange);

        if (_spawnManager != null)
        {
            _spawnManager.monsterCount.AddListener(MonsterCountChange);
            _spawnManager.WaveStart();
        }
    }

    public void Update()
    {
    }

    public void Exit()
    {
        Service.Get<GameManager>()?._wall.currentHp.RemoveListener(WallHpChange);
        if (_spawnManager != null) _spawnManager.monsterCount.RemoveListener(MonsterCountChange);
    }

    private void MonsterCountChange(int count)
    {
        if (count > 0) return;

        if (_spawnManager.currentWave.Value >= totalWave)
        {
            _manager.ClearStage();
            _manager.CurrentState.Value = GameState.Clear;
        }
        else _manager.CurrentState.Value = GameState.Sort;
    }

    private void WallHpChange(int hp)
    {
        if (hp <= 0)
        {
            _manager.CurrentState.Value = GameState.GameOver;
        }
    }
}
