using UnityEngine;

public class GameOverState : IState
{
    private GameManager _manager;
    
    public GameOverState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
        Service.Get<MonsterSpawnManager>()?.StopAllCoroutines();
        Debug.Log("스테이지 실패, 게임오버");
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}
