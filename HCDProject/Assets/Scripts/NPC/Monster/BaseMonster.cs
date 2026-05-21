using UnityEngine;

public class BaseMonster : MonoBehaviour
{
    [SerializeField] private MonsterData monsterData;
    
    private protected StateMachine State;

    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }
    
    public void ChangeState(EStateType state)
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
