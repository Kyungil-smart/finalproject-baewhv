using UnityEngine;
using UnityEngine.AI;

public class MonsterChaseState : IState
{
    private BaseMonster _controller;
    private Vector3 _currentTarget;

    public MonsterChaseState(BaseMonster controller)
    {
        _controller = controller;
    }
    
    public void Enter()
    {
    }

    public void Update()
    {
        if ((Vector2.Distance(_controller.transform.position, _controller.Target)
             + _controller.GetRadius() 
             + _controller.GetCurrentTarget.GetRadius()) <= _controller.skills[0].SKILL_IS)
        {
            _controller.CurrentState.Value = EStateType.Attack;
            return;
        }

        if (_currentTarget != _controller.Target)
        {
            _currentTarget = _controller.Target;
            _controller.Movement.Move(_controller.Target);
        }
    }

    public void Exit()
    {
        _controller.Movement.Stop();
    }
}
