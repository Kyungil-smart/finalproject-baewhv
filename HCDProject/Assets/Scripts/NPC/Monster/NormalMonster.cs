using UnityEngine;

public class NormalMonster : BaseMonster
{
    protected override void Awake()
    {
        base.Awake();
        
        Init();
    }

    private void Init()
    {
        State = new StateMachine();
        IdleState = new MonsterIdleState(this);
        ChaseState = new MonsterChaseState(this);
        AttackState = new MonsterAttackState(this);
        DieState = new MonsterDieState(this);
    }
}
