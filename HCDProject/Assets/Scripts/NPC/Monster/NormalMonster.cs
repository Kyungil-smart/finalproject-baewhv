using UnityEngine;

public class NormalMonster : BaseMonster
{
    private void Awake()
    {
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
        
        ChangeState(EStateType.Idle);
    }
}
