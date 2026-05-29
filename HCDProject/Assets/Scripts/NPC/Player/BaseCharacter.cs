using System.Collections.Generic;
using UnityEngine;

// 모든 직업군이 공통으로 가질 클래스
public class BaseCharacter : BaseController
{
    private StateMachine _stateMachine;

    private SpawnPlayerState _spawnPlayerState;

    private IdlePlayerState _idlePlayerState;

    private ChasePlayerState _chasePlayerState;

    private AttackPlayerState _attackPlayerState;

    private DiePlayerState _diePlayerState;

    private Vector3 _homePosition; // 지정된 위치

    private int _skillTargetIndex; // 지정된 타겟에 대한 스킬 인덱스

    bool _isFirstCombat = true; // 전사 첫번째 전투

    [SerializeField] private float _reviveTime; // 캐릭터 부활시간

    private bool _isDead;

    public float ReviveTime => _reviveTime;

    private EFindType _findType;

    public int SkillCoolTime => skills[_skillTargetIndex].coolTime;
    
    public int CurrentSkillRange => skills[_skillTargetIndex].skillRange;
    
    public EFindType FindType
    {
        get => _findType;
        set => _findType = value;
    }

    public int SkillTargetIndex
    {
        get => _skillTargetIndex;
        set => _skillTargetIndex = value;
    }

    public void CompleteFirstCombat() // 전사 첫번째 전투
    {
        if (!_isFirstCombat) return;
        _isFirstCombat = false;
        _findType = EFindType.Nearest;
        Debug.Log($"[전환] {gameObject.name} 첫 전투 완료 → FindType: {_findType}");
    }

    [field: SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }


    [SerializeField] private CharacterBaseData _baseData;

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

    public Vector3 homePosition
    {
        get => _homePosition;

        set => _homePosition = value;
    }

    public StateMachine state => _stateMachine;
    
    public AttackPlayerState attack => _attackPlayerState;
    
    public IdlePlayerState idle => _idlePlayerState;

    public SpawnPlayerState spawn => _spawnPlayerState;

    public ChasePlayerState chase => _chasePlayerState;

    public DiePlayerState die => _diePlayerState;

    protected override void Awake()
    {
        base.Awake();
        _stateMachine = new StateMachine();
        _spawnPlayerState = new SpawnPlayerState(this);
        _idlePlayerState = new IdlePlayerState(this);
        _chasePlayerState = new ChasePlayerState(this);
        _attackPlayerState = new AttackPlayerState(this);
        _diePlayerState = new DiePlayerState(this);
    }

    /*private void Start()
    {
        // 테스트용: SO가 인스펙터에 할당돼 있으면 Init 호출
        if (_baseData != null)
        {
            _homePosition = transform.position;
            Init(_baseData);
        }
    }*/

    protected void OnEnable()
    {
        CurrentHp.AddListener(CheckDeath);
    }

    protected void OnDisable()
    {
        CurrentHp.RemoveListener(CheckDeath);
    }

    protected virtual void Update()
    {
        _stateMachine?.Update();
    }

    public void Init(CharacterBaseData data)
    {
        _baseData = data;

        _stats = new CharacterStats
        {
            _maxHp = data._hp,
            _attackPower = data._attackPower,
            _defense = data._defense,
            _moveSpeed = data._moveSpeed,
            _attackSpeed = data._attackSpeed,
            _critRate = data._critRate,
            _critDamage = data._critDamage,
            _attackRange = data._attackRange,
            _chaseRange = data._chaseRange
        };
        _findType = EFindType.Farthest;
        CurrentHp.Value = _stats._maxHp;
        Movement.Agent.speed = _stats._moveSpeed;
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

    public ITargetable FindTarget(int index)
    {
        List<ITargetable> targets = Detect(Stats._chaseRange, skills[index].TargetType);
        Debug.Log($"[탐색] FindType: {_findType} | 탐지 수: {targets.Count}");

        ITargetable nearest = null;

        float nearestDis = float.MaxValue;
        float fartDis = float.MinValue;

        foreach (ITargetable target in targets)
        {
            float dis = (this.transform.position - target.GetTargetObject.transform.position).sqrMagnitude;

            switch (FindType)
            {
                case EFindType.Nearest:
                    if (dis < nearestDis)
                    {
                        nearestDis = dis;
                        nearest = target;
                    }
                    break;
                case EFindType.Farthest:
                    if (dis > fartDis)
                    {
                        fartDis = dis;
                        nearest = target;
                    }
                    break;
            }

        }
        return nearest;
    }

    private void CheckDeath(int value)
    {
        if (_isDead) return;

        if (value <= 0)
        {
            _isDead = true;
            Debug.Log($"[사망] {gameObject.name} | HP: {value}");
            this.state.ChangeState(this.die);
        }
    }

    public void Revive()
    {
        _isDead = false;
        CurrentHp.Value = Stats._maxHp;

        _isFirstCombat = true;
        _findType = EFindType.Farthest;
    }
}
