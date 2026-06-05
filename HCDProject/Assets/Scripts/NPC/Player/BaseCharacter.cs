using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// 모든 직업군이 공통으로 가질 클래스
public class BaseCharacter : BaseController
{
    private StateMachine _stateMachine;

    private SpawnPlayerState _spawnPlayerState;

    private IdlePlayerState _idlePlayerState;

    private ChasePlayerState _chasePlayerState;

    private AttackPlayerState _attackPlayerState;

    private DiePlayerState _diePlayerState;

    [SerializeField]private Vector3 _homePosition; // 지정된 위치
    [SerializeField] private Vector3 _spawnPosition; // 스폰 및 부활

    private int _skillTargetIndex; // 지정된 타겟에 대한 스킬 인덱스

    bool _isFirstCombat = true; // 전사 첫번째 전투

    bool _isSpawning = true;

    public bool _isDead;

    [SerializeField] private float _reviveTime; // 캐릭터 부활시간
    
    private PlayerStats _playerStats;

    public bool IsSpawning
    {
        get => _isSpawning;
        set => _isSpawning = value;
    }
    
    public float ReviveTime => _reviveTime;

    private EFindType _findType;

    public float SkillCoolTime => skills[_skillTargetIndex].SKILL_TIME;
    
    public float CurrentSkillRange => skills[_skillTargetIndex].SKILL_IS;
    
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

    public void SetNavMeshActive(bool isActive)
    {
        if (isActive)
        {
            this.Movement.Agent.enabled = isActive;
            this.Movement.Agent.isStopped = false;
        }

        else
        {
            this.Movement.Agent.isStopped = true;
            this.Movement.Agent.enabled = isActive;
        }
    }

    public void CompleteFirstCombat() // 전사 첫번째 전투
    {
        if (!_isFirstCombat) return;
        _isFirstCombat = false;
        _findType = EFindType.Nearest;
        Debug.Log($"[전환] {gameObject.name} 첫 전투 완료 → FindType: {_findType}");
    }

    [field: SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }

    public override void SetCurrentTarget(ITargetable target)
    {
        _currentTarget = target;
    }

    public Vector3 spawnPosition
    {
        get => _spawnPosition;
        set => _spawnPosition = value;
    }

    public Vector3 homePosition
    {
        get => _homePosition;

        set => _homePosition = value;
    }
    #region stateMachine
    public StateMachine state => _stateMachine;
    
    public AttackPlayerState attack => _attackPlayerState;
    
    public IdlePlayerState idle => _idlePlayerState;

    public SpawnPlayerState spawn => _spawnPlayerState;

    public ChasePlayerState chase => _chasePlayerState;

    public DiePlayerState die => _diePlayerState;
    #endregion

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

    protected override void OnEnable()
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

    public void BindHpUI(UnityAction<float> action)
    {
        int maxValue = _stats._maxHp;
        CurrentHp = new RatioIntValue(maxValue);
        CurrentHp.AddRatioListener(action);
        CurrentHp.AddListener(CheckDeath);
    }

    public void Init(CharacterRawData data, PlayerStats stat)
    {
        _playerStats = stat;

        _stats = new CharacterStats
        {
            _maxHp = data.HP,
            _attackPower = data.ATK,
            _defense = data.DEF,
            _moveSpeed = (int)data.MOVE_SPEED,
            _attackSpeed = data.ATK_SPEED,
            _critRate = data.CRI_RATE,
            _critDamage = data.CRI_DMAGE
        };

        skills.Clear();
        var skillTable = Service.Get<DataManager>().PlayerActiveSkillTable.data;
        var atkData = skillTable.Find(s => s.SKILL_ID == data.ATK_ID);
        if (atkData != null) skills.Add(new Skill(atkData));

        var skillData = skillTable.Find(s => s.SKILL_ID == data.SKILL_ID);
        if (skillData != null) skills.Add(new Skill(skillData));
        _isFirstCombat = _playerStats._hasFirstCombat;
        _findType = _playerStats._initFindType;
        CurrentHp.Value = _stats._maxHp;
        Movement.Agent.speed = _stats._moveSpeed;
        _stateMachine.ChangeState(_spawnPlayerState);
        Debug.Log($"[캐릭터초기화] {gameObject.name} / FindType: {_findType}");
    }

    public override int UseCritDamage(int baseDamage)
    {
        float critValue = UnityEngine.Random.value;

        int finalCrit = Mathf.CeilToInt(baseDamage * Stats._critDamage);

        if (Stats._critRate >= critValue)
        {
            return finalCrit;
        }
        return baseDamage;
    }

    public override void UseSkill(int index)
    {
        // 현재 스킬의 n번째 스킬이 범위인지, 단일인지
        float totalDamage = (_stats._attackPower * skills[index].SKILL_ABILLITY);
        if (skills[index].SKILL_TYPE == ESkillType.SINGLE_TARGET)
        {
            if (skills[index].SKILL_AT == ETargetType.ALLY)
            {
                int healAmount = UseCritDamage((int)totalDamage);
                Debug.Log($"[힐] {gameObject.name}({GetInstanceID()}) → " +
                    $"{GetCurrentTarget.GetTargetObject.name}({GetCurrentTarget.GetTargetObject.GetInstanceID()}) / 힐량: {healAmount}");
                GetCurrentTarget.SetHeal(healAmount);
            }

            else
            {
                GetCurrentTarget.SetDamage(UseCritDamage((int)totalDamage));
            }
        }
        
        if (skills[index].SKILL_TYPE == ESkillType.ATTACK_OF_SCOPE)
        {
            AttackRange(index, (int)totalDamage);
        }
    }

    public ITargetable FindTarget(int index)
    {
        if (_isSpawning) return null;
        //if (skills == null || skills.Count == 0) return null;
        List<ITargetable> targets = Detect(skills[index].SKILL_IS, skills[index].SKILL_AT);
        ITargetable nearest = null;

        if (FindType == EFindType.LowestHp)
        {
            float lowestRatio = float.MaxValue;
            foreach (ITargetable t in targets)
            {
                if (t is BaseCharacter ally)
                {
                    float ratio = (float)ally.CurrentHp.Value / ally.Stats._maxHp;
                    if (ratio < lowestRatio && ratio < 1.0f)
                    {
                        lowestRatio = ratio;
                        nearest = t;
                    }
                }
            }
            return nearest;
        }

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
        _isSpawning = true;
        Movement.Agent.enabled = true;
        this.Movement.Agent.Warp(spawnPosition);
        CurrentHp.Value = Stats._maxHp;

        _isFirstCombat = _playerStats._hasFirstCombat;
        Debug.Log($"before: {_findType}");
        _findType = _playerStats._initFindType;
        Debug.Log($"after: {_findType}");
    }

    protected new void OnDrawGizmos()
    {
        if (skills == null || skills.Count == 0) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, CurrentSkillRange);
    }
}
