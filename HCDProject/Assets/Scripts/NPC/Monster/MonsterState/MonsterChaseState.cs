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
        _controller.SetCurrentTarget(_controller.Detect(_controller.Stats._chaseRange));
    }

    public void Update()
    {
        if (_controller.GetTargetObject == null)
        {
            _controller.Movement.DownMove();
        }
        else
        {
            if (Vector2.Distance(_controller.transform.position,
                    _controller.GetTargetObject.transform.position) <= _controller.Stats._attackRange)
            {
                _controller.CurrentState.Value = EStateType.NearAttack;
            }
            else
            {
                _controller.Movement.Move(_controller.GetTargetObject.transform.position);
            }
        }
        
        _timer += Time.deltaTime;
        if (_timer >= 0.2f)
        {
            _timer = 0f;
            _controller.SetCurrentTarget(_controller.Detect(_controller.Stats._chaseRange));
        }
    }

    public void Exit()
    {
        _controller.Movement.Stop();
    }
}
