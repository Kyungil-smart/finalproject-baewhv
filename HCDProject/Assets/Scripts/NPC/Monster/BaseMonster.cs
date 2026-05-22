using System;
using Unity.Mathematics.Geometry;
using UnityEngine;

public abstract class BaseMonster : MonoBehaviour
{
    [field:SerializeField] public MonsterData MonsterData { get; set; }

    #region State
    private protected StateMachine State;
    [field:SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }
    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }
    public MonsterNearAttackState NearAttackState { get; protected set; }
    #endregion

    public Transform player;
    public LayerMask LayerMask { get; private set; }

    protected Collider2D[] _colliders = new Collider2D[5];
    protected ContactFilter2D _filter;

    protected virtual void Awake()
    {
        CurrentState = new();
        LayerMask = LayerMask.GetMask("Player");
        
        _filter = new ContactFilter2D();
        
        _filter.useLayerMask = true;
        _filter.SetLayerMask(LayerMask);
        _filter.useTriggers = false;
    }

    protected virtual void Update()
    {
        State?.Update();
    }
    
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
    
    public Transform DetectPlayer(float range)
    {
        int count = Physics2D.OverlapCircle(transform.position,
            range,
            _filter,
            _colliders);

        Transform target = null;
        float minDistance = range;
        
        for (int i = 0; i < count; i++)
        {
            float distance = DistanceToPlayer(_colliders[i].transform);

            if (minDistance > distance)
            {
                minDistance = distance;
                target = _colliders[i].transform;
            }
        }
        
        return target;
    }

    public float DistanceToPlayer(Transform target)
    {
        return Vector2.Distance(transform.position, target.position);
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, MonsterData.chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, MonsterData.attackRange);
    }
}
