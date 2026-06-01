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
        Debug.Log("상태: Idle 진입");
    }

    public void Exit()
    {
        // TODO : 대기 애니메이션 정지
    }

    public void Update()
    {
        if (_owner.GetCurrentTarget == null)
        {
            float dist = Vector2.Distance(_owner.transform.position, _owner.homePosition);
            if(dist > 0.2f)
            {
                _owner.Movement.Move(_owner.homePosition);
            }
        }

        ITargetable target = _owner.FindTarget(_owner.SkillTargetIndex);

        if (target != null)
        {
            _owner.SetCurrentTarget(target);
            _owner.state.ChangeState(_owner.chase);
        }
    }
}
