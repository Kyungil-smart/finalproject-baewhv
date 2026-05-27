using UnityEngine;
using UnityEngine.AI;

public class MonsterChaseState : IState
{
    private BaseMonster _controller;

    public MonsterChaseState(BaseMonster controller)
    {
        _controller = controller;
    }
    
    public void Enter()
    {
    }

    public void Update()
    {
        if (_controller.GetCurrentTarget == null)
        {
            _controller.Movement.DownMove();
            return;
        }

        if (_controller.DistanceToTarget(_controller.GetCurrentTarget.GetTargetObject.transform) <=
            _controller.Stats._attackRange)
        {
            _controller.CurrentState.Value = EStateType.Attack;
            return;
        }
        
        _controller.Movement.Move(_controller.GetCurrentTarget.GetTargetObject.transform.position);
    }

    public void Exit()
    {
        _controller.Movement.Stop();
    }
}
