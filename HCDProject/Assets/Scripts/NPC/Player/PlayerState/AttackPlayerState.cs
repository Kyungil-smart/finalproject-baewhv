using UnityEngine;

public class AttackPlayerState : PlayerBaseState
{
    public AttackPlayerState(BaseCharacter owner) : base(owner)
    {
        
    }
    public override void Enter()
    {
        /*Debug.Log($"[{_owner.gameObject.name}] Attack 진입 " +
              $"/ FindType: {_owner.FindType} " +
              $"/ 타겟: {_owner.GetCurrentTarget?.GetTargetObject.name}");*/
        _owner.AttackTimer = 0;
    }
    public override void Exit()
    {
        
    }
    public override void Update()
    {
        if (_owner.isCC) return;

        if (_owner.GetCurrentTarget == null || !_owner.GetCurrentTarget.IsAlive()) // 타겟이 없다면?
        {
            _owner.SetCurrentTarget(null);
            _owner.state.ChangeState(_owner.idle);
            return;
        }

        float dist = Vector3.Distance(_owner.transform.position,
                _owner.GetCurrentTarget.GetTargetObject.transform.position)
                - _owner.GetRadius() - _owner.GetCurrentTarget.GetRadius();
        if (dist > _owner.CurrentSkillRange) // 공격 범위 벗어났으면
        {
            _owner.state.ChangeState(_owner.chase);
            return;
        }

        if (_owner.AttackTimer <= _owner.SkillCoolTime)
        {
            _owner.AttackTimer += Time.deltaTime;
        }
        else
        {
            /*Debug.Log($"[공격] {_owner.gameObject.name}이(가) " +
          $"{_owner.BaseSkill.skills[_owner.SkillTargetIndex].SKILL_TYPE} / " +
          $"{_owner.BaseSkill.skills[_owner.SkillTargetIndex].SKILL_AT} 발동");*/
            _owner.UseSkill(_owner.SkillTargetIndex); // 공격
            _owner.CompleteFirstCombat();
            _owner.AttackTimer = 0;
            if (_owner.FindType == EFindType.LowestHp)
            {
                ITargetable newTarget = _owner.FindTarget(_owner.SkillTargetIndex);
                if (newTarget == null)  // 힐 필요한 아군 없음 → Idle 복귀
                {
                    _owner.SetCurrentTarget(null);
                    _owner.state.ChangeState(_owner.idle);
                    return;
                }
                _owner.SetCurrentTarget(newTarget);  // 새 타겟으로 교체
            }
        }
    }
}
