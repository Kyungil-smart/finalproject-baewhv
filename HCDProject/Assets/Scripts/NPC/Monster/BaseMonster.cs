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
    public MonsterNearAttackState NearAttackState { get; protected set; }
    #endregion

    public LayerMask LayerMask { get; private set; }

    protected Collider2D[] _colliders = new Collider2D[5];
    protected ContactFilter2D _filter;
    
    public override void SetCurrentTarget(ITargetable target)
    {
        _currentTarget = target;
        CurrentState.Value = target != null ? EStateType.Chase : EStateType.Idle;
    }

    protected override void Awake()
    {
        base.Awake();
        
        CurrentState = new();
        LayerMask = LayerMask.GetMask("Player");
        _filter = new ContactFilter2D();
        
        _filter.useLayerMask = true;
        _filter.SetLayerMask(LayerMask);
        _filter.useTriggers = false;
    }
    
    protected void OnEnable()
    {
        CurrentState.AddListener(ChangeState);
    }

    protected void OnDisable()
    {
        CurrentState.RemoveListener(ChangeState);
    }
    
    public override void SetDamage()
    {
        
    }

    public override void SetHeal()
    {
        
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
            case EStateType.NearAttack:
                State.ChangeState(NearAttackState);
                break;
            /*
            case EStateType.Die:
                _state.ChangeState(DieState);
                break;
            */
        }
    }

    public void Detect(float range)
    {
        int count = Physics2D.OverlapCircle(transform.position,
            range,
            _filter,
            _colliders);

        GameObject target = null;
        float minDistanceSqr = range * range;
        
        for (int i = 0; i < count; i++)
        {
            float distanceSqr = (transform.position - _colliders[i].transform.position).sqrMagnitude;

            if (minDistanceSqr > distanceSqr)
            {
                minDistanceSqr = distanceSqr;
                target = _colliders[i].gameObject;
            }
        }

        GetTargetObject = target;
    }

    public float DistanceToPlayer(Transform target)
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
