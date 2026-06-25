using UnityEngine;

public class ReadyState : IState
{
    private GameManager _manager;
    
    public ReadyState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
        _manager.StartNarrative();
        
        var playerManager = Service.Get<PlayerManager>();
        if (playerManager != null && playerManager.isAllSpawn != null)
        {
            playerManager.isAllSpawn.Value = false;
        }
    }

    public void Update()
    {
        var playerManager = Service.Get<PlayerManager>();
        
        if (playerManager == null || !playerManager.IsPrefabLoaded) return;
        
        if (playerManager != null)
        {
            if (Service.Get<PlayerManager>().isAllSpawn.Value) _manager.CurrentState.Value = GameState.Sort;
        }
    }

    public void Exit()
    {
        _manager.SpawnWall();
    }
}
