using UnityEngine;

public class BaseMonster : MonoBehaviour
{
    [SerializeField] private MonsterData monsterData;
    
    private protected StateMachine State;

    [field:SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }

    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }
    
    private protected void ChangeState(EStateType state)
    {
        switch (state)
        {
            case EStateType.Idle:
                State.ChangeState(IdleState);
                break;
            case EStateType.Chase:
                State.ChangeState(ChaseState);
                break;
            /*
            case EStateType.Attack:
                _state.ChangeState(AttackState);
                break;
            case EStateType.Die:
                _state.ChangeState(DieState);
                break;
            */
        }
    }
}
