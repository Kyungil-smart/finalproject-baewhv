using UnityEngine;
using UnityEngine.AI;

public class MonsterChaseState : IState
{
    private BaseMonster _controller;
    private float _timer;

    public MonsterChaseState(BaseMonster controller)
    {
        _controller = controller;
    }
    
    public void Enter()
    {
        _timer = 0f;
    }

    public void Update()
    {
        if (_controller.GetCurrentTarget == null)
        {
            _controller.Movement.DownMove();
        }
        else
        {
            if (_controller.DistanceToTarget(_controller.GetCurrentTarget.GetTargetObject.transform) > _controller.Stats._attackRange)
            {
                _controller.Movement.Move(_controller.GetCurrentTarget.GetTargetObject.transform.position);
            }
            else
            {
                _controller.CurrentState.Value = EStateType.Attack;
                return;
            }
        }
        
        _timer += Time.deltaTime;
        if (_timer >= 0.2f)
        {
            _timer = 0f;
            _controller.SetCurrentTarget(_controller.Detect(_controller.Stats._chaseRange, (int)ETargetType.Enemy));
        }
    }

    public void Exit()
    {
        _controller.Movement.Stop();
    }
}
