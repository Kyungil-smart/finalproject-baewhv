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
        coolCount = 0;
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

        if (dist > _owner.stat._attackRange) // 공격 범위 벗어났으면
        {
            _owner.state.ChangeState(_owner.chase);
            return;
        }

        if (coolCount <= _owner.stat._attackSpeed)
        {
            coolCount += Time.deltaTime;
        }

        else
        {
            // 공격
            coolCount = 0;
        }
    }
}
