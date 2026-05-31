using UnityEngine;

public class ReadyState : IState
{
    private GameManager _manager;
    
    public ReadyState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
    }

    public void Update()
    {
        if (Service.Get<SortManager>() != null)
        {
            // if (Service.Get<PlayerManager>().isAllSpawn.Value) _manager.CurrentState.Value = GameState.Sort;
        }
    }

    public void Exit()
    {
    }
}
