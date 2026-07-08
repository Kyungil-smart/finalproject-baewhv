using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

    private string _playerSfx; // 플레이어 공격 효과음

    [SerializeField] private float _flickerInterval = 0.05f;
    [SerializeField] private int _flickerCycleCount = 3;
    private PlayerStats _playerStats;

    private Sequence _hitFlickerSeq;

    private int _previousHp;

    public bool isCC = false;

    public bool isBattle = false; // 배틀 중이면 스킬게이지 안오르게

    public string PlayerSfx => _playerSfx; // 플레이어 공격 사운드 프로퍼티

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

    private EFindType _findType;

    public float SkillCoolTime => BaseSkill.skills[_skillTargetIndex].SKILL_TIME > 0 ?
        BaseSkill.skills[_skillTargetIndex].SKILL_TIME : _currentStats._attackSpeed; // 노말공격 쿨타임
    public float ActiveSkillCoolTime => BaseSkill.skills.Count > 1 ? BaseSkill.skills[1].SKILL_TIME : 0f; // 액티브 쿨타임
    public float CurrentSkillRange => BaseSkill.skills[_skillTargetIndex].SKILL_IS;

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
        CurrentHp.AddListener(OnHpChanged);
    }

    protected void OnDisable()
    {
        CurrentHp.RemoveListener(CheckDeath);
        CurrentHp.RemoveListener(OnHpChanged);
        StopHitFlicker();
    }

    public void FixedUpdate()
    {
        _stateMachine?.FixedUpdate();
    }

    protected virtual void Update()
    {
        _stateMachine?.Update();
        if (!isCC && isBattle) _activeSkillCoolCount += Time.deltaTime;
        float result = Mathf.Clamp(_activeSkillCoolCount, 0, ActiveSkillCoolTime);
        if (_activeSkillCoolValue == null) return;
        _activeSkillCoolValue.Value = result;
    }
    public void BindHpUI(UnityAction<int, int> action) // 슬롯 HP, 캐릭터 HP바 UI 구독
    {
        int maxValue = _currentStats._maxHp;
        CurrentHp = new RatioIntValue(maxValue);
        CurrentHp.AddValuesListener(action);
        CurrentHp.AddListener(CheckDeath);
        CurrentHp.AddListener(OnHpChanged);
        if (_hpBar != null)
            CurrentHp.AddRatioListener(_hpBar.SetHPBar);
        CurrentHp.Invoke();
    }

    public void BindSkillUI(UnityAction<float> action) // 스킬 게이지 UI 구독
    {
        float maxValue = BaseSkill.skills[1].SKILL_TIME;
        _activeSkillCoolValue = new RatioFloatValue(maxValue, _activeSkillCoolCount);
        _activeSkillCoolValue.AddRatioListener(action);
    }

    public void BindDeathUI(UnityAction<bool> action) // 사망판정 UI 구독
    {
        IsAlived = new ObserveValue<bool>();
        IsAlived.AddListener(action);
        IsAlived.Value = true;
    }
    public void UpdateStats(CharacterStats currentStats, CharacterStats baseStats)
    {
        _currentStats = currentStats;
        _baseStats = baseStats;

        CurrentHp.MaxValue = _currentStats._maxHp;

        
        if (CurrentHp.Value > _currentStats._maxHp)
        {
            CurrentHp.Value = _currentStats._maxHp;
        }
    }
    private void OnHpChanged(int newHp)
    {
        if (_isDead) return;
        if (newHp < _previousHp)
        {
            PlayHitFlicker();
        }
        _previousHp = newHp;
    }

    private void PlayHitFlicker()
    {
        StopHitFlicker();
        _hitFlickerSeq = DOTween.Sequence();
        _hitFlickerSeq.AppendCallback(() => _characterRenderer.enabled = false);
        _hitFlickerSeq.AppendInterval(_flickerInterval);
        _hitFlickerSeq.AppendCallback(() => _characterRenderer.enabled = true);
        _hitFlickerSeq.AppendInterval(_flickerInterval);
        _hitFlickerSeq.SetLoops(_flickerCycleCount);
        _hitFlickerSeq.OnComplete(() => _characterRenderer.enabled = true);
    }

    private void StopHitFlicker()
    {
        if(_hitFlickerSeq != null && _hitFlickerSeq.IsActive())
        {
            _hitFlickerSeq.Kill();
        }

        if(_characterRenderer != null)
        {
            _characterRenderer.enabled = true;
        }
    }

    #region Init()
    public void Init(CharacterRawData data, PlayerStats stat)
    {
        _playerStats = stat;

        gameObject.name = data.CHARACTER_NAME; // 플레이어 디버그용
        if (data.CHARACTER_ID != "3000")
        {
            string address = "";
            switch(data.CHARACTER_ID)
            {
                case "3001": address = "Player/NoahOverride"; break;
                case "3002": address = "Player/AliceOverride"; break;
                case "3003": address = "Player/SpayneOverride"; break;
            }
            RuntimeAnimatorController controller = 
                Addressables.LoadAssetAsync<RuntimeAnimatorController>(address).WaitForCompletion();
            Movement.Anim.runtimeAnimatorController = controller;
        }

        if (data.CHARACTER_ID != null) // 플레이어 공격 효과음 초기화
        {
            switch (data.CHARACTER_ID)
            {
                case "3000": _playerSfx = "Sword"; break;
                case "3001": _playerSfx = "Arrow"; break;
                case "3002": _playerSfx = "Spell"; break;
                case "3003": _playerSfx = "Healing"; break;
            }
        }

        _baseStats = new CharacterStats
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
        
        _currentStats = _baseStats;

        BaseSkill.skills.Clear();
        var skillTable = Service.Get<DataManager>().SkillTable.data;
        var atkData = skillTable.Find(s => s.SKILL_ID == data.ATK_ID);
        if (atkData != null) BaseSkill.skills.Add(new Skill(atkData));

        var skillData = skillTable.Find(s => s.SKILL_ID == data.SKILL_ID);
        if (skillData != null) BaseSkill.skills.Add(new Skill(skillData));
        _baseSkillData = GetComponent<BaseSkill>(); // 베이스 스킬 갖고옴
        _baseSkillData.skills = BaseSkill.skills;
        _isFirstCombat = _playerStats._hasFirstCombat;
        _findType = _playerStats._initFindType;
        _playerStats._doubleAtkRate = data.DOUBLE_ATK_RATE;
        CurrentHp.Value = _currentStats._maxHp;
        _previousHp = CurrentHp.Value;
        Movement.Agent.speed = _currentStats._moveSpeed;
        _stateMachine.ChangeState(_spawnPlayerState);
        Debug.Log($"[캐릭터초기화] {gameObject.name} / FindType: {_findType}");
    }
    #endregion

    public override int UseCritDamage(int baseDamage)
    {
        float critValue = UnityEngine.Random.value;

        int finalCrit = Mathf.CeilToInt(baseDamage * CurrentStats._critDamage);

        if (CurrentStats._critRate >= critValue)
        {
            return finalCrit;
        }
        return baseDamage;
    }

    public override void UseSkill(int index)
    {
        _baseSkillData.UseSkill(index);
    }
    
    public void TryUseActiveSkill() // 액티브 스킬 발동
    {
        if (this.isCC) return;
        if (_activeSkillCoolCount < ActiveSkillCoolTime) return;

        if (BaseSkill.skills[1].SKILL_AT == ETargetType.ENEMY && GetCurrentTarget == null) return;

        Debug.Log($"[액티브 스킬] {gameObject.name} 발동!");
        switch (_playerStats._activeSkillBehavior)
        {
            case EActiveSkillBehavior.DotField:
                UseSkill(1);
                if (BaseSkill.skills.Find(s => s.SKILL_ID == "6509") != null)
                {
                    int skillDamage = (int)(_currentStats._attackPower * BaseSkill.skills[1].SKILL_AB_02);
                    float earthquakeBonus = Service.Get<RelicManager>()?
                    .GetTotalRelicBonus("WIZARD", "EARTHQUAKE_DAMAGE_P") ?? 0f;
                    FireEarthquake(Mathf.CeilToInt(skillDamage * earthquakeBonus / 100f));
                }
                break;

            case EActiveSkillBehavior.Instant:
            default:
                UseSkill(1);
                break;
        }
        _activeSkillCoolCount = 0;

        GetComponent<PlayerRelics>()?.TryShield();
    }

    public void FireRainOfArrows(Vector2 center, int damage) // 궁수 화살비 데미지호출
    {
        Debug.Log($"[화살비 발사] 중심: {center} / 데미지: {damage} / 반경: {BaseSkill.skills[2].SKILL_RANGE_X}");
        int count = Physics2D.OverlapCircle(center, BaseSkill.skills[2].SKILL_RANGE_X, EnemyFilter, Colliders);
        for (int i = 0; i < count; i++)
        {
            if (Colliders[i].TryGetComponent(out ITargetable target))
            {
                target.SetDamage(damage, BaseSkill.skills[2]);
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
                target.SetDamage(damage, BaseSkill.skills[2]);
                Debug.Log($"[지진 적중] {target.GetTargetObject.name}");
            }
        }
    }
    public ITargetable FindTarget(int index)
    {
        if (_isSpawning) return null;
        ETargetType targetType = BaseSkill.skills[index].SKILL_AT;
        List<ITargetable> targets = new List<ITargetable>();
        ITargetable nearest = null;
        switch (targetType)
        {
            case ETargetType.ENEMY:
                targets = Detect(_currentStats._chaseRange + GetRadius(), BaseSkill.skills[index].SKILL_AT);
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
                            float ratio = (float)ally.CurrentHp.Value / ally.CurrentStats._maxHp;
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
            StopHitFlicker();
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
        CurrentHp.Value = CurrentStats._maxHp;
        _previousHp = CurrentHp.Value;

        _isFirstCombat = _playerStats._hasFirstCombat;
        _findType = _playerStats._initFindType;
    }
}
