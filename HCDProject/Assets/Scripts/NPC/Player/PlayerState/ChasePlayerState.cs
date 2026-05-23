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
        
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        if (_owner.currentTarget == null)
        {
            _owner.state.ChangeState(_owner.idle);
            return;
        }

        float dist = Vector3.Distance(_owner.transform.position, _owner.currentTarget.position);

        _owner.transform.position = Vector3.MoveTowards(_owner.transform.position,
            _owner.currentTarget.position,
            _owner.stat._moveSpeed * Time.deltaTime);

        if (dist <= _owner.stat._attackRange)
        {
            _owner.state.ChangeState(_owner.attack);
        }
    }
}
