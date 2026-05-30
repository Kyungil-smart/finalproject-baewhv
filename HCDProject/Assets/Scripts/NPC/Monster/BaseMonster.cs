using System;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class BaseMonster : BaseController
{
    [field:SerializeField] public MonsterRawData Stat { get; private set;}
    [field:SerializeField] public int MonsterID { get; set; }
    
    #region State
    private protected StateMachine State;
    [field:SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }
    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }
    public MonsterAttackState AttackState { get; protected set; }
    public MonsterDieState DieState { get; protected set; }
    #endregion

    private float _timer;
    
    public void InitStatus(MonsterRawData data)
    {
        Stat = data;
        gameObject.name = Stat.MONSTER_NAME;
        

        // 데이터를 받아와서 사용할 위치
        // CurrentHp.Value = data.HP;
        // Stat.ATK = data.ATK;
        // Movement.Agent.speed = Stat.MOVE_SPEED;
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

    public ITargetable FindTarget()
    {
        List<ITargetable> targets = Detect(Stats._chaseRange, ETargetType.Enemy);

        ITargetable player = null;
        ITargetable wall = null;
        
        float playerDis = float.MaxValue;
        float wallDis = float.MaxValue;

        foreach (ITargetable target in targets)
        {
            float dis = (transform.position - target.GetTargetObject.transform.position).sqrMagnitude;

            if (target is BaseCharacter)
            {
                if (dis < playerDis)
                {
                    playerDis = dis;
                    player = target;
                }
            }
            else if (target is Rampart)
            {
                if (dis < wallDis)
                {
                    wallDis = dis;
                    wall = target;
                }
            }
        }

        return player ?? wall;
    }

    private void ResetTarget()
    {
        _timer += Time.deltaTime;

        if (_timer <= 0.2f) return;
        
        _timer = 0f;
        SetCurrentTarget(FindTarget());
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
