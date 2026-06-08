using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// 모든 직업군이 공통으로 가질 클래스
public class BaseCharacter : BaseController
{
    #region State
    private StateMachine _stateMachine;

    private SpawnPlayerState _spawnPlayerState;

    private IdlePlayerState _idlePlayerState;

    private ChasePlayerState _chasePlayerState;

    private AttackPlayerState _attackPlayerState;

    private DiePlayerState _diePlayerState;
    #endregion

    [SerializeField] private Vector3 _homePosition; // 지정된 위치
    [SerializeField] private Vector3 _spawnPosition; // 스폰 및 부활

    private int _skillTargetIndex; // 지정된 타겟에 대한 스킬 인덱스

    private bool _isFirstCombat = true; // 전사 첫번째 전투

    private bool _isSpawning = true;

    private bool _isBuffActive = false; // 버프 판별

    public bool _isDead;

    private float _activeSkillCoolCount = 999f; // 액티브스킬쿨타임(임시로 999초)

    [SerializeField] private float _reviveTime; // 캐릭터 부활시간

    private PlayerStats _playerStats;

    public bool IsSpawning
    {
        get => _isSpawning;
        set => _isSpawning = value;
    }

    public float ReviveTime => _reviveTime;

    private EFindType _findType;

    public float SkillCoolTime => skills[_skillTargetIndex].SKILL_TIME > 0 ?
        skills[_skillTargetIndex].SKILL_TIME : _stats._attackSpeed; // 노말공격 쿨타임
    public float ActiveSkillCoolTime => skills.Count > 1 ? skills[1].SKILL_TIME : 0f; // 액티브 쿨타임
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
        _activeSkillCoolCount += Time.deltaTime;
    }

    public void BindHpUI(UnityAction<float> action)
    {
        int maxValue = _stats._maxHp;
        CurrentHp = new RatioIntValue(maxValue);
        CurrentHp.AddRatioListener(action);
        CurrentHp.AddListener(CheckDeath);
    }

    #region Init()
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
        var skillTable = Service.Get<DataManager>().PlayerSkillTable.data;
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
    #endregion

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

        if (skills[index].ATK_TYPE == EAtkType.BUFF) { SetBuff(); return; } // 버프스킬

        if (skills[index].SKILL_TYPE == ESkillType.SINGLE_TARGET) // 단일대상
        {
            if (skills[index].SKILL_AT == ETargetType.ALLY)
            {
                int healAmount = UseCritDamage((int)totalDamage);
                GetCurrentTarget.SetHeal(healAmount);
            }

            else
            {
                if (_isBuffActive) // 버프상태라면?
                {
                    for (int i = 0; i < 3; i++) // 3번 공격(궁수)
                    {
                        GetCurrentTarget.SetDamage(UseCritDamage((int)totalDamage));
                    }
                }
                else
                {
                    GetCurrentTarget.SetDamage(UseCritDamage((int)totalDamage));
                } 
            }
        }
        if (skills[index].SKILL_TYPE == ESkillType.ATTACK_OF_SCOPE) // 범위 공격
        {
            if (skills[index].ATK_TYPE == EAtkType.SKILL)
            {
                AttackRangeBox(index, (int)totalDamage);
                // 마법사 범위공격
            }
        }

        if (skills[index].SKILL_TYPE == ESkillType.ALL_TARGET) // 전체 공격
        {
            if (skills[index].SKILL_AT == ETargetType.ALLY)
            {
                var characters = Service.Get<PlayerManager>()?.Characters;
                if (characters == null) return;
                foreach (BaseCharacter chr in characters)
                {
                    if (!chr._isDead) 
                    {
                        float heal = chr.Stats._maxHp * 0.15f;
                        chr.SetHeal((int)heal);
                        Debug.Log($"[힐] {chr.gameObject.name} → {(int)heal} 회복");
                    }

                    else
                    {
                        Service.Get<PlayerManager>()?.ImmediateRevive(chr);
                        Debug.Log($"[부활] {chr.gameObject.name} 즉시 부활!");
                    }
                }
            }
        }
    }

    public void SetBuff() // 궁수 공속버프
    {
        if (_isBuffActive) return;
        _isBuffActive = true;
        float originSpeed = _stats._attackSpeed;
        _stats._attackSpeed *= 0.7f;
        Debug.Log($"[궁수 버프] 발동! 공속: {_stats._attackSpeed}");
        StartCoroutine(BuffCoroutine(originSpeed));
    }

    private IEnumerator BuffCoroutine(float originSpeed) // 궁수 버프 코루틴
    {
        yield return YieldContainer.WaitForSeconds(7f);
        _isBuffActive = false;
        _stats._attackSpeed = originSpeed;
        Debug.Log($"[궁수 버프] 종료! 공속 복구: {originSpeed}");
    }

    public void TryUseActiveSkill() // 액티브 스킬
    {
        if (_activeSkillCoolCount >= ActiveSkillCoolTime)
        {
            Debug.Log($"[액티브 스킬] {gameObject.name} 발동!");
            UseSkill(1);
            _activeSkillCoolCount = 0;
        }
    }

    public void TryDotFieldSkill() // 마법사 액티브호출
    {
        if (_activeSkillCoolCount >= ActiveSkillCoolTime)
        {
            if (GetCurrentTarget == null) return;

            Vector2 fieldCenter = GetCurrentTarget.GetTargetObject.transform.position;
            float damage = _stats._attackPower * skills[1].SKILL_ABILLITY;

            StartCoroutine(DotFieldCoroutine(fieldCenter, (int)damage));
            _activeSkillCoolCount = 0;
        }
    }

    private IEnumerator DotFieldCoroutine(Vector2 fieldCenter, int damage) // 장판 도트딜
    {
        float elapsed = 0f;

        while(elapsed < 5f)
        {
            int count = Physics2D.OverlapCircle(fieldCenter, skills[1].SKILL_RANGE_X, EnemyFilter, Colliders);
            for (int i = 0; i < count; i++)
            {
                if (Colliders[i].TryGetComponent(out ITargetable target))
                {
                    target.SetDamage(damage);
                }
            }
            Debug.Log($"[마법사 장판] {elapsed}초 틱 → {count}명 적중");
            yield return YieldContainer.WaitForSeconds(1f);
            elapsed += 1f;
        }
    }

    public void AttackRangeBox(int index, int damage) // 전사 액티브 스킬
    {
        if (GetCurrentTarget == null) return;
        Vector2 dir = (GetCurrentTarget.GetTargetObject.transform.position - transform.position).normalized;
        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        Vector2 bestDir = Vector2.right;
        float bestDot = float.MinValue;

        foreach (Vector2 candidate in directions)
        {
            float dot = Vector2.Dot(dir, candidate);
            if (dot > bestDot)
            {
                bestDir = candidate;
                bestDot = dot;
            }
        }
        Vector2 node = (Vector2)transform.position + bestDir * (skills[index].SKILL_RANGE_X * 0.5f);

        int count = Physics2D.OverlapBox(node, new Vector2(skills[index].SKILL_RANGE_X, skills[index].SKILL_RANGE_Y),
            0f, EnemyFilter, Colliders);
        Debug.Log($"[전사 스킬] 범위 공격 → {count}명 적중");
        for (int i = 0; i < count; i++)
        {
            if (Colliders[i].TryGetComponent(out ITargetable target))
            {
                target.SetDamage(damage);
            }
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
