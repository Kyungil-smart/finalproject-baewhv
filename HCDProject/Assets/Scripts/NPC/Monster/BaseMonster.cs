using System;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class BaseMonster : BaseController
{
    [SerializeField] private int monsterID;
    
    #region State
    private protected StateMachine State;
    [field:SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }
    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }
    public MonsterAttackState AttackState { get; protected set; }
    public MonsterDieState DieState { get; protected set; }
    #endregion

    private float _timer;
    
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
    
    protected void OnEnable()
    {
        CurrentState.AddListener(ChangeState);
        CurrentHp.AddListener(CheckDeath);
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
        if (monsterID == 0) return;
        
        if (value <= 0)
        {
            MonsterSpawnManager.Instance.DespawnMonster(monsterID, gameObject);
        }
    }
}
