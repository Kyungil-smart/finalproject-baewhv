using UnityEngine;

public class NormalMonster : BaseMonster
{
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        State = new StateMachine();
        IdleState = new MonsterIdleState(this);
        ChaseState = new MonsterChaseState(this);
        
        ChangeState(EStateType.Idle);
    }
}
