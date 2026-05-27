using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 모든 직업군이 공통으로 가질 클래스
public class BaseCharacter : BaseController
{
    private StateMachine _stateMachine;

    private SpawnPlayerState _spawnPlayerState;

    private IdlePlayerState _idlePlayerState;

    private ChasePlayerState _chasePlayerState;

    private AttackPlayerState _attackPlayerState;

    private Transform _currentBackup; // 탐지할 대상

    private Vector3 _homePosition; // 지정된 위치

    [field: SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }


    [SerializeField] private CharacterBaseData _baseData;

    private CharacterStats _currentStats;

    public override void SetCurrentTarget(ITargetable target)
    {
        _currentTarget = target;
            
            if (target == null)
            {
                state.ChangeState(this.spawn);
            }

            else
            {
                state.ChangeState(this.chase);
            }
        
    }

    public CharacterStats stat
    {
        get => _currentStats;
    }

    public Vector3 homePosition
    {
        get => _homePosition;
        
        set => _homePosition = value;
    }

    public StateMachine state
    {
        get => _stateMachine;
    }

    public AttackPlayerState attack
    {
        get => _attackPlayerState;
    }

    public IdlePlayerState idle
    {
        get => _idlePlayerState;
    }

    public SpawnPlayerState spawn
    {
        get => _spawnPlayerState;
    }

    public ChasePlayerState chase
    {
        get => _chasePlayerState;
    }
    

    protected override void Awake()
    {
        base.Awake();
        _stateMachine = new StateMachine();
        _spawnPlayerState = new SpawnPlayerState(this);
        _idlePlayerState = new IdlePlayerState(this);
        _chasePlayerState = new ChasePlayerState(this);
        _attackPlayerState = new AttackPlayerState(this);
    }

    private void Start()
    {
        // 테스트용: SO가 인스펙터에 할당돼 있으면 Init 호출
        if (_baseData != null)
        {
            _homePosition = transform.position;
            Init(_baseData);
        }
    }

    protected virtual void Update()
    {
        _stateMachine?.Update();
    }

    public void Init(CharacterBaseData data)
    {
        _baseData = data;

        _currentStats = new CharacterStats
        {
            _maxHp = data._hp,
            _attackPower = data._attackPower,
            _defense = data._defense,
            _moveSpeed = data._moveSpeed,
            _attackSpeed = data._attackSpeed,
            _critRate = data._critRate,
            _critDamage = data._critDamage,
            _attackRange = data._attackRange
        };
        _stateMachine.ChangeState(_spawnPlayerState);
    }

    public override int UseCritDamage(int baseDamage)
    {
        float critValue = Random.value;

        int finalCrit = Mathf.CeilToInt(baseDamage * Stats._critDamage);

        if (Stats._critRate >= critValue)
        {
            return finalCrit;
        }
        return baseDamage;
    }

    public ITargetable FindTarget()
    {
        List<ITargetable> targets = Detect(Stats._chaseRange, ETargetType.Enemy);

        ITargetable monster = null;

        float monsterDis = float.MaxValue;

        foreach (ITargetable target in targets)
        {
            float dis = (this.transform.position - target.GetTargetObject.transform.position).sqrMagnitude;

            if (target is BaseMonster)
            {
                if (dis < monsterDis)
                {
                    monsterDis = dis;
                    monster = target;
                }
            }
        }
        return monster;
    }
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Stats._chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Stats._attackRange);
    }

}
