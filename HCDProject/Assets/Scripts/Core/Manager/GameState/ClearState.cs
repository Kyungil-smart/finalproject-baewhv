using UnityEngine;

public class ClearState : IState
{
    private GameManager _manager;
    
    public ClearState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
        Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.OnGameClear();
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}
