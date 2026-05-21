using UnityEngine;

public abstract class BaseMonster : MonoBehaviour
{
    [field:SerializeField] public MonsterData MonsterData { get; set; }
    
    private protected StateMachine State;
    [field:SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }
    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }

    public Transform player;
    public LayerMask LayerMask { get; private set; }

    protected Collider2D[] _colliders = new Collider2D[4];
    protected ContactFilter2D _filter;

    protected virtual void Awake()
    {
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
    
    public Transform DetectPlayer(float range, out float minDistance)
    {
        int count = Physics2D.OverlapCircle(transform.position,
            range,
            _filter,
            _colliders);

        Transform target = null;
        float minDistanceSqrt = range * range;
        
        for (int i = 0; i < count; i++)
        {
            float distance = DistanceToPlayer(_colliders[i].transform);

            if (minDistanceSqrt > distance)
            {
                minDistanceSqrt = distance;
                target = _colliders[i].transform;
            }
        }
        
        minDistance = Mathf.Sqrt(minDistanceSqrt);

        return target;
    }

    public float DistanceToPlayer(Transform target)
    {
        return Vector2.Distance(transform.position, target.position);
    }
}
