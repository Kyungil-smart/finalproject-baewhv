using System;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class BaseMonster : BaseController
{
    [field:SerializeField] public MonsterRawData Stat { get; private set;}
    public int MonsterID { get; private set; }
    
    #region State
    private protected StateMachine State;
    [field:SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }
    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }
    public MonsterAttackState AttackState { get; protected set; }
    public MonsterDieState DieState { get; protected set; }
    #endregion

    private float _timer;
    public Vector3 Target { get; set; }
    
    protected BaseCharacter[] _characters;
    
    public void InitStatus(MonsterRawData data)
    {
        Stat = data;
        
        // 데이터를 받아와서 사용할 위치
        MonsterID = int.Parse(Stat.MONSTER_ID) - 1000;
        gameObject.name = Stat.MONSTER_NAME;
        CurrentHp.Value = data.HP;
        Movement.Agent.speed = Stat.MOVE_SPEED;
        
        // Stat.ATK = data.ATK;
    }
    
    public override void SetCurrentTarget(ITargetable target)
    {
        _currentTarget = target;
    }

    protected override void Awake()
    {
        base.Awake();
        
        CurrentState = new();
        
        _timer = 0f;
    }

    protected void Start()
    {
        _characters = Service.Get<PlayerManager>().Characters;
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        CurrentState.AddListener(ChangeState);
        CurrentHp.AddListener(CheckDeath);
        
        State.ChangeState(IdleState);
    }

    protected void OnDisable()
    {
        CurrentState.RemoveListener(ChangeState);
        CurrentHp.RemoveListener(CheckDeath);
    }

    protected virtual void Update()
    {
        State?.Update();
        
        ResetTarget();
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
    
    private ITargetable FindTarget()
    {
        ITargetable target = Service.Get<GameManager>()._wall;
        
        float minDistance = float.MaxValue;
        
        foreach (BaseCharacter player in _characters)
        {
            if (player._isDead) continue;
            
            float dis = (transform.position - player.transform.position).sqrMagnitude;

            if (dis < minDistance)
            {
                minDistance = dis;
                target = player;
            }
        }

        return target;
    }
    

    private void ResetTarget()
    {
        SetCurrentTarget(FindTarget());

        if (GetCurrentTarget is BaseCharacter)
        {
            Target = GetCurrentTarget.GetTargetObject.transform.position;
        }
        else if (GetCurrentTarget is Rampart)
        {
            Target = new Vector3(transform.position.x, GetCurrentTarget.GetTargetObject.transform.position.y, transform.position.z);
        }
    }

    public float DistanceToTarget(Transform target)
    {
        return Vector2.Distance(transform.position, target.position);
    }
    
    private void CheckDeath(int value)
    {
        if (value <= 0)
        {
            CurrentState.Value = EStateType.Die;
        }
    }
}
