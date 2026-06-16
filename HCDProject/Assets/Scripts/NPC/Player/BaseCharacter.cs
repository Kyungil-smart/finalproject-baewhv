using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

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
    [SerializeField] private HPBarUI _hpBar; // 캐릭터 개별 HP바

    private SpriteRenderer _characterRenderer;

    private int _skillTargetIndex; // 지정된 타겟에 대한 스킬 인덱스

    private bool _isFirstCombat = true; // 전사 첫번째 전투

    private bool _isSpawning = true;

    private bool _isBuffActive = false; // 버프 판별

    public bool _isDead;

    private ObserveValue<bool> IsAlived; // 부활 ui 연동

    private float _activeSkillCoolCount; // 액티브스킬쿨타임

    private RatioFloatValue _activeSkillCoolValue; // 액티브 스킬게이지 비율

    private float _attackTimer; // 누적 카운트

    [SerializeField] private float _reviveTime; // 캐릭터 부활시간

    private PlayerStats _playerStats;

    public float AttackTimer // 누적 카운트 프로퍼티
    {
        get => _attackTimer;
        set => _attackTimer = value;
    }

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
        if (isActive) // 활성화
        {
            this.Movement.Agent.enabled = isActive;
            this.Movement.Agent.isStopped = false;
        }

        else // 비활성화
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

    public PlayerStats PlayerStat
    {
        get => _playerStats;
        set => _playerStats = value;
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
        _characterRenderer = GetComponentInChildren<SpriteRenderer>();
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
        float result = Mathf.Clamp(_activeSkillCoolCount, 0, ActiveSkillCoolTime);
        if (_activeSkillCoolValue == null) return;
        _activeSkillCoolValue.Value = result;
    }

    public void BindHpUI(UnityAction<float> action) // 슬롯 HP, 캐릭터 HP바 UI 구독
    {
        int maxValue = _stats._maxHp;
        CurrentHp = new RatioIntValue(maxValue);
        CurrentHp.AddRatioListener(action);
        CurrentHp.AddListener(CheckDeath);
        if (_hpBar != null)
            CurrentHp.AddRatioListener(_hpBar.SetHPBar);
        CurrentHp.Invoke();
    }

    public void BindSkillUI(UnityAction<float> action) // 스킬 게이지 UI 구독
    {
        float maxValue = skills[1].SKILL_TIME;
        _activeSkillCoolValue = new RatioFloatValue(maxValue, _activeSkillCoolCount);
        _activeSkillCoolValue.AddRatioListener(action);
    }

    public void BindDeathUI(UnityAction<bool> action) // 사망판정 UI 구독
    {
        IsAlived = new ObserveValue<bool>();
        IsAlived.AddListener(action);
        IsAlived.Value = true;
    }

    #region Init()
    public void Init(CharacterRawData data, PlayerStats stat)
    {
        _playerStats = stat;

        gameObject.name = data.CHARACTER_NAME; // 플레이어 디버그용
        Color color;
        switch (data.CHARACTER_ID)
        {
            case "3000":
                ColorUtility.TryParseHtmlString("#BC3F3F", out color);
                break;
            case "3001":
                ColorUtility.TryParseHtmlString("#A25FA6", out color);
                break;
            case "3002":
                ColorUtility.TryParseHtmlString("#EEE83B", out color);
                break;
            case "3003":
                ColorUtility.TryParseHtmlString("#59C8FF", out color);
                break;
            default:
                color = Color.white;
                Debug.LogWarning($"{data.CHARACTER_ID}의 색이 추가되지 않았습니다.");
                break;
        }
        _characterRenderer.color = color;

        _stats = new CharacterStats
        {
            _maxHp = data.HP,
            _attackPower = data.ATK,
            _defense = data.DEF,
            _moveSpeed = data.MOVE_SPEED,
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
        _playerStats._doubleAtkRate = data.DOUBLE_ATK_RATE;
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
        float totalDamage = CalculateDamage(index);

        switch (skills[index].SKILL_TYPE, skills[index].SKILL_AT) // 스킬타입과, 스킬대상 비교
        {
            case (ESkillType.SINGLE_TARGET, ETargetType.SELF): // 궁수버프
                SetBuff();
                break;

            case (ESkillType.SINGLE_TARGET, ETargetType.ALLY): // 아군 힐
                GetCurrentTarget.SetHeal(UseCritDamage((int)totalDamage));
                break;

            case (ESkillType.SINGLE_TARGET, ETargetType.ENEMY): // 적에게 단일공격
                if (_isBuffActive)
                {
                    for (int i = 0; i < 3; i++) // 궁수가 버프를 받았다면 3번공격
                        GetCurrentTarget.SetDamage(UseCritDamage((int)totalDamage));
                }
                else // 그외 단일공격
                {
                    GetCurrentTarget.SetDamage(UseCritDamage((int)totalDamage));
                    if (UnityEngine.Random.value < _playerStats._doubleAtkRate) // 궁수유물적용시 연속공격
                    {
                        GetCurrentTarget.SetDamage(UseCritDamage((int)totalDamage));
                    }
                }
                break;

            case (ESkillType.ATTACK_OF_SCOPE, ETargetType.ENEMY): // 적에게 범위스킬
                AttackRangeBox(index, (int)totalDamage);
                break;

            case (ESkillType.ALL_TARGET, ETargetType.ALLY): // 아군에게 전체스킬
                var characters = Service.Get<PlayerManager>()?.Characters;
                if (characters == null) return;
                foreach (BaseCharacter chr in characters)
                {
                    if (!chr._isDead)
                    {
                        chr.SetHeal((int)totalDamage);
                        Debug.Log($"[힐] {chr.gameObject.name} → {(int)totalDamage} 회복");
                    }
                    else
                    {
                        Service.Get<PlayerManager>()?.ImmediateRevive(chr);
                        Debug.Log($"[부활] {chr.gameObject.name} 즉시 부활!");
                    }
                }
                break;

            case (ESkillType.ALL_TARGET, ETargetType.ENEMY): // TODO : 유물구현시
                // 추후 구현 (유물 마법사)
                break;
        }
    }
    // 데미지 계산만 담당하는 별도 메서드
    private float CalculateDamage(int index)
    {
        switch (skills[index].SKILL_DT)
        {
            case ESkillDamageType.ATTACK_BASE:
                return _stats._attackPower * skills[index].SKILL_ABILLITY;
            case ESkillDamageType.TRUE_DAMAGE:
                return skills[index].SKILL_ABILLITY;
            case ESkillDamageType.SKILL_DAMAGE_P:
                return 0f; // 추후 구현예정
            default:
                return 0f;
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

    public void TryUseActiveSkill() // 액티브 스킬 발동
    {
        if (_activeSkillCoolCount < ActiveSkillCoolTime) return;

        if (skills[1].SKILL_AT == ETargetType.ENEMY && GetCurrentTarget == null) return;

        Debug.Log($"[액티브 스킬] {gameObject.name} 발동!");
        switch (_playerStats._activeSkillBehavior)
        {
            case EActiveSkillBehavior.DotField:
                Vector2 fieldCenter = GetCurrentTarget.GetTargetObject.transform.position;
                StartCoroutine(DotFieldCoroutine(fieldCenter, (int)CalculateDamage(1)));
                break;

            case EActiveSkillBehavior.Instant:
            default:
                UseSkill(1);
                break;
        }
        _activeSkillCoolCount = 0;
    }

    public void TryDotFieldSkill() // 마법사 액티브호출
    {
        if (_activeSkillCoolCount >= ActiveSkillCoolTime)
        {
            if (GetCurrentTarget == null) return;

            Vector2 fieldCenter = GetCurrentTarget.GetTargetObject.transform.position;

            StartCoroutine(DotFieldCoroutine(fieldCenter, (int)CalculateDamage(1)));
            _activeSkillCoolCount = 0;
        }
    }

    private IEnumerator DotFieldCoroutine(Vector2 fieldCenter, int damage) // 장판 도트딜
    {
        float elapsed = 0f;

        while (elapsed < 5f)
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
        List<ITargetable> targets = Detect(skills[index].SKILL_IS + GetRadius(), skills[index].SKILL_AT);
        ITargetable nearest = null;
        if (FindType == EFindType.LowestHp)
        {
            float lowestRatio = float.MaxValue;
            foreach (ITargetable t in targets)
            {
                if (!this.Movement.CanReach(t.GetTargetObject.transform.position)) continue;
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
            if (!this.Movement.CanReach(target.GetTargetObject.transform.position)) continue;
            float dis = (this.transform.position - target.GetTargetObject.transform.position).magnitude
                - GetRadius() - target.GetRadius();

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
            if (IsAlived != null) IsAlived.Value = false;
            Debug.Log($"[사망] {gameObject.name} | HP: {value}");
            this.state.ChangeState(this.die);
        }
    }

    public void Revive()
    {
        _isDead = false;
        if (IsAlived != null) IsAlived.Value = true;
        _isSpawning = true;
        Movement.Agent.enabled = true;
        this.Movement.Agent.Warp(spawnPosition);
        CurrentHp.Value = Stats._maxHp;

        _isFirstCombat = _playerStats._hasFirstCombat;
        _findType = _playerStats._initFindType;
    }

    protected new void OnDrawGizmos()
    {
        if (skills == null || skills.Count == 0) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, CurrentSkillRange + GetRadius());
    }
}
