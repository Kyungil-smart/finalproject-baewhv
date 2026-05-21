using UnityEngine;

public class NormalMonster : BaseMonster
{
    protected override void Awake()
    {
        base.Awake();
        
        Init();
    }
    
    private void OnEnable()
    {
        CurrentState.AddListener(ChangeState);
    }

    private void OnDisable()
    {
        CurrentState.RemoveListener(ChangeState);
    }

    private void Init()
    {
        State = new StateMachine();
        IdleState = new MonsterIdleState(this);
        ChaseState = new MonsterChaseState(this);
        NearAttackState = new MonsterNearAttackState(this);

        State.ChangeState(IdleState);
    }
}
