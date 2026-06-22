using UnityEngine;
using UnityEngine.AI;

public class MonsterChaseState : IState
{
    private BaseMonster _controller;
    private Vector3 _currentTarget;
    private float _timer;

    public MonsterChaseState(BaseMonster controller)
    {
        _controller = controller;
    }
    
    public void Enter()
    {
        _timer = 0f;
        _currentTarget = Vector3.zero;
    }

    public void Update()
    {
        if (Vector3.Distance(_controller.transform.position, _controller.Target)
              <= _controller.BaseSkill.skills[0].SKILL_IS + _controller.GetRadius() 
                                                          + _controller.GetCurrentTarget.GetRadius())
        {
            _controller.CurrentState.Value = EStateType.Attack;
            return;
        }
        
        _timer += Time.deltaTime;

        if (_timer >= 0.1f && _currentTarget != _controller.Target)
        {
            _timer = 0f;
            _currentTarget = _controller.Target;
            _controller.Movement.Move(_controller.Target);
        }
    }

    public void Exit()
    {
        _controller.Movement.Stop();
    }
}
