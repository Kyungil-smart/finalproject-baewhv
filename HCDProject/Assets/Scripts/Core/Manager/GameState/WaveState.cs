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
        if (_spawnManager != null && _spawnManager.currentWave.Value == 1) Service.Get<SoundManager>()?.PlayBgmSound("Battle_2");
        if (_spawnManager != null && _spawnManager.currentWave.Value == 2) Service.Get<SoundManager>()?.PlayBgmSound("Battle_3");
        
        if (_spawnManager != null) _spawnManager.monsterCount.RemoveListener(MonsterCountChange);
    }

    private void MonsterCountChange(int count)
    {
        if (count > 0) return;

        if (_spawnManager.currentWave.Value >= totalWave)
        {
            _manager.ClearStage();
        }
        else _manager.CurrentState.Value = GameState.Sort;
    }
}
