using UnityEngine;

public class MonsterDieState : IState
{
    private BaseMonster _controller;

    public MonsterDieState(BaseMonster controller)
    {
        _controller = controller;
    }
    
    public void Enter()
    {
        
    }

    public void Update()
    {
        Service.Get<MonsterSpawnManager>().DespawnMonster(_controller.MonsterID, _controller.gameObject);
    }

    public void Exit()
    {
        
    }

}
