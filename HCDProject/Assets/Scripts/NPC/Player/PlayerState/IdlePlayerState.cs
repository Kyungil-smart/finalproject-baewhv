using System.Collections.Generic;
using UnityEngine;

public class IdlePlayerState : PlayerBaseState
{
    public IdlePlayerState(BaseCharacter owner) : base(owner)
    {
        
    }
    public override void Enter()
    {
        // TODO : 대기 애니메이션(있다면)
        
    }
    public override void Exit()
    {
        // TODO : 대기 애니메이션 정지
    }
    public override void Update()
    {
        if (_owner.isCC) return;

        float homeDist = Vector2.Distance(_owner.transform.position, _owner.homePosition);
        if (_owner.GetCurrentTarget == null)
        {
            if (homeDist > 0.2f)
            {
                _owner.Movement.Move(_owner.homePosition);
            }
            else
            {
                _owner.Movement.MoveFalse();
            }
        }
    }

    public override void FixedUpdate()
    {
        if (_owner.isCC) return;

        ITargetable target = _owner.FindTarget(_owner.SkillTargetIndex);
        if (target != null)
        {
            _owner.SetCurrentTarget(target);
            float surfaceDist = Vector2.Distance(_owner.transform.position,
                _owner.GetCurrentTarget.GetTargetObject.transform.position)
                - _owner.GetRadius() - _owner.GetCurrentTarget.GetRadius();
            if (_owner.FindType == EFindType.LowestHp || surfaceDist <= _owner.CurrentSkillRange)
            {
                _owner.state.ChangeState(_owner.attack);
            }
            else
            {
                _owner.state.ChangeState(_owner.chase);
            }
        }
    }
}
