using Unity.Multiplayer.PlayMode;
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
        _controller.Movement.Agent.isStopped = false;
        GameObject obj = _controller.GetCurrentTarget.GetTargetObject;
        if (obj.GetComponent<BaseController>() != null)
        {
            _controller.Movement.Move(obj.transform.position);
        }
        else
        {
            // 아래로 보내는 로직
            // _controller.Movement.Move(obj.transform.position);
        }
    }

    public void Update()
    {
        if (_controller.Movement.IsMove == false)
        {
            if (Vector2.Distance(_controller.transform.position,
                    _controller.GetCurrentTarget.GetTargetObject.transform.position) <= _controller.Stats._attackRange)
            {
                _controller.CurrentState.Value = EStateType.NearAttack;
            }
            else
            {
                _controller.Movement.Move(_controller.GetCurrentTarget.GetTargetObject.transform.position);
            }
        }
    }

    public void Exit()
    {
        _controller.Movement.Agent.isStopped = true;
    }
}
