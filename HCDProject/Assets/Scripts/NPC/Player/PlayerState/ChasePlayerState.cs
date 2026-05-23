using Unity.VisualScripting;
using UnityEngine;

public class ChasePlayerState : IState
{
    private BaseCharacter _owner;

    public ChasePlayerState(BaseCharacter owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Movement.Move(_owner.GetCurrentTarget.GetTargetObject.transform.position);
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        if (_owner.GetCurrentTarget == null)
        {
            _owner.state.ChangeState(_owner.idle);
            return;
        }

        if (!_owner.Movement.IsMove)
        {
            float dist = Vector3.Distance(_owner.transform.position,
                _owner.GetCurrentTarget.GetTargetObject.transform.position);
            if (dist <= _owner.stat._attackRange)
            {
                _owner.state.ChangeState(_owner.attack);
            }
            else
            {
                _owner.Movement.Move(_owner.GetCurrentTarget.GetTargetObject.transform.position);
            }
        }

    }
}
