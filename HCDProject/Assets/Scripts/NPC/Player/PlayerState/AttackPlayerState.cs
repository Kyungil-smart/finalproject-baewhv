using UnityEngine;

public class AttackPlayerState : IState
{
    private BaseCharacter _owner;
    
    float coolCount;

    public AttackPlayerState(BaseCharacter owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        Debug.Log("상태: Attack 진입");
        coolCount = 0;
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

        float dist = Vector3.Distance(_owner.transform.position,
            _owner.GetCurrentTarget.GetTargetObject.transform.position);


        if (dist > _owner.CurrentSkillRange) // 공격 범위 벗어났으면
        {
            _owner.state.ChangeState(_owner.chase);
            return;
        }

        if (coolCount <= _owner.SkillCoolTime)
        {
            coolCount += Time.deltaTime;
        }

        else
        {
            Debug.Log($"[공격] {_owner.gameObject.name}이(가) 스킬 {_owner.SkillTargetIndex}번 발동");
            _owner.UseSkill(_owner.SkillTargetIndex); // 공격
            _owner.CompleteFirstCombat();
            coolCount = 0;
        }
    }
}
