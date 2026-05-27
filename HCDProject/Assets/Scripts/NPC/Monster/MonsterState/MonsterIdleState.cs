using UnityEngine;

public class MonsterIdleState : IState
{
    private BaseMonster _controller;

    public MonsterIdleState(BaseMonster controller)
    {
        _controller = controller;
    }
    
    public void Enter()
    {
        _controller.SetCurrentTarget(_controller.FindTarget());
    }
    
    public void Update()
    {
        _controller.CurrentState.Value = EStateType.Chase;
    }
    
    public void Exit()
    {
        
    }
}
