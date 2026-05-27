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

        if (dist > _owner.Stats._attackRange) // 공격 범위 벗어났으면
        {
            _owner.state.ChangeState(_owner.chase);
            return;
        }

        if (coolCount <= _owner.Stats._attackSpeed)
        {
            coolCount += Time.deltaTime;
        }

        else
        {
            _owner.UseSkill(0); // 공격
            coolCount = 0;
        }
    }
}
