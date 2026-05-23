using System.Collections.Generic;
using UnityEngine;

public class IdlePlayerState : IState
{
    private BaseCharacter _owner;


    public IdlePlayerState(BaseCharacter owner)
    {
        _owner = owner;
        
    }

    public void Enter()
    {
        // TODO : 대기 애니메이션(있다면)
    }

    public void Exit()
    {
        // TODO : 대기 애니메이션 정지
    }

    public void Update()
    {
        ITargetable target = _owner.FindTarget();

        if (target != null)
        {
            _owner.SetCurrentTarget(target);
            _owner.state.ChangeState(_owner.chase);
        }
    }
}
