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
    private PlayerStateMachine _stateMachine;

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

    private BaseSkill _baseSkillData;

    private int _skillTargetIndex; // 지정된 타겟에 대한 스킬 인덱스

    private bool _isFirstCombat = true; // 전사 첫번째 전투

    private bool _isSpawning = true;

    public bool _isDead;

    private ObserveValue<bool> IsAlived; // 부활 ui 연동

    private float _activeSkillCoolCount; // 액티브스킬쿨타임

    private RatioFloatValue _activeSkillCoolValue; // 액티브 스킬게이지 비율

    private float _attackTimer; // 누적 카운트

    [SerializeField] private float _reviveTime; // 캐릭터 부활시간

    private PlayerStats _playerStats;

    public bool isCC = false;
    
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
    public PlayerStateMachine state => _stateMachine;

    public AttackPlayerState attack => _attackPlayerState;

    public IdlePlayerState idle => _idlePlayerState;

    public SpawnPlayerState spawn => _spawnPlayerState;

    public ChasePlayerState chase => _chasePlayerState;

    public DiePlayerState die => _diePlayerState;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        _stateMachine = new PlayerStateMachine();
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

    public void FixedUpdate()
    {
        _stateMachine?.FixedUpdate();
    }

    protected virtual void Update()
    {
        _stateMachine?.Update();
        if (!isCC) _activeSkillCoolCount += Time.deltaTime;
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
            _critDamage = data.CRI_DMAGE,
            _chaseRange = data.ACCESS_AREA
        };

        skills.Clear();
        var skillTable = Service.Get<DataManager>().SkillTable.data;
        var atkData = skillTable.Find(s => s.SKILL_ID == data.ATK_ID);
        if (atkData != null) skills.Add(new Skill(atkData));

        var skillData = skillTable.Find(s => s.SKILL_ID == data.SKILL_ID);
        if (skillData != null) skills.Add(new Skill(skillData));
        _baseSkillData = GetComponent<BaseSkill>(); // 베이스 스킬 갖고옴
        _baseSkillData.skills = this.skills;
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
        if (index == 0
        || skills[index].SKILL_ABT_01 == ESkillAbilityType.BASE_SKILL_WARIOR
        || skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK_SPEED_P
        || skills[index].SKILL_ABT_01 == ESkillAbilityType.MAX_HP_P
        || skills[index].SKILL_ABT_01 == ESkillAbilityType.BASE_SKILL_WIZARD)
        {
            _baseSkillData.UseSkill(index);
            return;
        }

        float totalDamage = CalculateDamage(index);

        switch (skills[index].SKILL_TYPE, skills[index].SKILL_AT) // 스킬타입과, 스킬대상 비교
        {
            /*case (ESkillType.ALL_TARGET, ETargetType.ALLY): // 아군에게 전체스킬
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
                break;*/

            case (ESkillType.ALL_TARGET, ETargetType.ENEMY): // TODO : 유물구현시
                                                             // (유물 마법사)
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

    public void TryUseActiveSkill() // 액티브 스킬 발동
    {
        if (this.isCC) return;
        if (_activeSkillCoolCount < ActiveSkillCoolTime) return;

        if (skills[1].SKILL_AT == ETargetType.ENEMY && GetCurrentTarget == null) return;

        Debug.Log($"[액티브 스킬] {gameObject.name} 발동!");
        switch (_playerStats._activeSkillBehavior)
        {
            case EActiveSkillBehavior.DotField:
                UseSkill(1);
                if (skills.Find(s => s.SKILL_ID == "6509") != null)
                {
                    float earthquakeBonus = Service.Get<RelicManager>()?
                    .GetTotalRelicBonus("WIZARD", "SKILL_UPGRADE_DAMAGE_P") ?? 0f;
                    //FireEarthquake(Mathf.CeilToInt( * earthquakeBonus / 100f));
                }
                break;

            case EActiveSkillBehavior.Instant:
            default:
                UseSkill(1);
                break;
        }
        _activeSkillCoolCount = 0;
    }

    public void FireRainOfArrows(Vector2 center, int damage) // 궁수 화살비 데미지호출
    {
        Debug.Log($"[화살비 발사] 중심: {center} / 데미지: {damage} / 반경: {skills[2].SKILL_RANGE_X}");
        int count = Physics2D.OverlapCircle(center, skills[2].SKILL_RANGE_X, EnemyFilter, Colliders);
        for (int i = 0; i < count; i++)
        {
            if (Colliders[i].TryGetComponent(out ITargetable target))
            {
                target.SetDamage(damage);
                Debug.Log($"[화살비 적중] {target.GetTargetObject.name}");
            }
        }
    }

    public void FireEarthquake(int damage) // 마법사 유물 지진마법
    {
        Debug.Log($"[지진 마법 발동]데미지 : {damage}");
        int count = Physics2D.OverlapCircle(transform.position, 100f, EnemyFilter, Colliders);
        for (int i = 0; i < count; i++)
        {
            if (Colliders[i].TryGetComponent(out ITargetable target))
            {
                target.SetDamage(damage);
                Debug.Log($"[지진 적중] {target.GetTargetObject.name}");
            }
        }
    }

    /*public void TryDotFieldSkill() // 마법사 액티브호출
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
    }*/
    public ITargetable FindTarget(int index)
    {
        if (_isSpawning) return null;
        ETargetType targetType = skills[index].SKILL_AT;
        List<ITargetable> targets = new List<ITargetable>();
        ITargetable nearest = null;
        switch (targetType)
        {
            case ETargetType.ENEMY:
                targets = Detect(_stats._chaseRange + GetRadius(), skills[index].SKILL_AT);
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

            case ETargetType.ALLY:
                for (int i = 0; i < Service.Get<PlayerManager>().Characters.Length; i++)
                {
                    if (Service.Get<PlayerManager>().Characters[i].TryGetComponent(out ITargetable target))
                    {
                        if (Service.Get<PlayerManager>().Characters[i]._isDead == true) continue;

                        targets.Add(target);
                    }
                }
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
                break;
            case ETargetType.SELF:
                return this;
        }
        return null;
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
}
