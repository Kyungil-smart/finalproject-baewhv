using System;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class BaseMonster : BaseController
{
    #region State
    private protected StateMachine State;
    [field:SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }
    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }
    public MonsterAttackState AttackState { get; protected set; }
    public MonsterDieState DieState { get; protected set; }
    #endregion
    
    public override void SetCurrentTarget(ITargetable target)
    {
        _currentTarget = target;
    }

    protected override void Awake()
    {
        base.Awake();
        
        CurrentState = new();
    }
    
    protected void OnEnable()
    {
        CurrentState.AddListener(ChangeState);
    }

    protected void OnDisable()
    {
        CurrentState.RemoveListener(ChangeState);
    }

    protected virtual void Update()
    {
        State?.Update();
    }
    
    private void ChangeState(EStateType state)
    {
        switch (state)
        {
            case EStateType.Idle:
                State.ChangeState(IdleState);
                break;
            case EStateType.Chase:
                State.ChangeState(ChaseState);
                break;
            case EStateType.Attack:
                State.ChangeState(AttackState);
                break;
            case EStateType.Die:
                State.ChangeState(DieState);
                break;
        }
    }

    public float DistanceToTarget(Transform target)
    {
        return Vector2.Distance(transform.position, target.position);
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Stats._chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Stats._attackRange);
    }
}
