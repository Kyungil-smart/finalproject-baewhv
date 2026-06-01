using UnityEngine;

public class GameOverState : IState
{
    private GameManager _manager;
    
    public GameOverState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
        Service.Get<MonsterSpawnManager>()?.StopAllCoroutines();
        Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.OnGameDefeat();
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}
