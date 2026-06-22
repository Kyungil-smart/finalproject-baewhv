using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class BaseMonster : BaseController
{
    [field: SerializeField] public MonsterRawData Stat { get; private set; }
    public int MonsterID { get; private set; }
    public int PrefabIndex { get; set; }

    #region State

    private protected StateMachine State;
    [field: SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }
    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }
    public MonsterAttackState AttackState { get; protected set; }
    public MonsterDieState DieState { get; protected set; }

    #endregion

    public Vector3 Target { get; set; }

    [SerializeField] private HPBarUI hpBarCanvas;
    private bool _isActive;

    private float _durateTimer;

    protected BaseCharacter[] _characters;

    public void InitStatus(MonsterRawData data)
    {
        if (Stat == data) return;

        Stat = data;
        InitSkill(data);

        // 몬스터의 기본 데이터 초기화
        // 데이터를 받아와서 사용할 위치
        MonsterID = int.Parse(Stat.MONSTER_ID) - 1000;
        gameObject.name = Stat.MONSTER_NAME;
        CurrentHp.MaxValue = Stat.HP;
        CurrentHp.Value = CurrentHp.MaxValue;
        _stats._maxHp = Stat.HP;
        _stats._attackPower = Stat.ATK;
        _stats._defense = Stat.DEF;
        _stats._moveSpeed = Stat.MOVE_SPEED;
        Movement.Agent.speed = Stat.MOVE_SPEED;
    }

    private void InitSkill(MonsterRawData data)
    {
        BaseSkill.skills.Clear();

        // 몬스터의 공격(스킬) 데이터 초기화
        var skillDataTable = Service.Get<DataManager>().SkillTable.data;
        SkillRawData atkData = skillDataTable.Find(x => x.SKILL_ID == data.ATK_ID);
        if (atkData != null) BaseSkill.skills.Add(new Skill(atkData));
        
        SkillRawData skillData = skillDataTable.Find(x => x.SKILL_ID == data.SKILL_ID);
        if (skillData != null) BaseSkill.skills.Add(new Skill(skillData));
    }

    public override void SetCurrentTarget(ITargetable target)
    {
        _currentTarget = target;
    }

    protected override void Awake()
    {
        base.Awake();

        CurrentState = new();
    }

    protected void Start()
    {
        _characters = Service.Get<PlayerManager>().Characters;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _isActive = false;

        if (CurrentHp.MaxValue != 0 && CurrentHp.Value <= 0)
        {
            CurrentHp.Value = CurrentHp.MaxValue;
        }

        CurrentState.AddListener(ChangeState);
        CurrentHp.AddListener(OnCheckDeath);
        CurrentHp.AddListener(OnAttacked);
        
        if (hpBarCanvas != null)
        {
            hpBarCanvas.gameObject.SetActive(_isActive);
            CurrentHp.AddRatioListener(hpBarCanvas.SetHPBar);
        }

        State.ChangeState(IdleState);
    }

    protected void OnDisable()
    {
        CurrentState.RemoveListener(ChangeState);
        CurrentHp.RemoveListener(OnCheckDeath);
        CurrentHp.RemoveListener(OnAttacked);
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

    public override void UseSkill(int index)
    {
        /*
        if (GetCurrentTarget == null) return;

        if (skills[index].ATK_TYPE == EAtkType.NORMAL)
        {
            GetCurrentTarget.SetDamage(Stat.ATK);
        }
        else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.HP)
        {
            RangeDetect(skills[index]);

            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                if (Colliders[i].TryGetComponent(out ITargetable target))
                {
                    target.SetHeal((int)skills[index].SKILL_AB_01);
                }
            }
        }
        else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK)
        {
            RangeDetect(skills[index]);

            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                if (Colliders[i].TryGetComponent(out BaseMonster controller))
                {
                    controller.Stat.ATK += (int)skills[index].SKILL_AB_01;
                }
            }
        }
        */
    }

    public void ReSkill(int index)
    {
        /*
        if (GetCurrentTarget == null) return;

        if (skills[index].SKILL_AT == ETargetType.ENEMY)
        {
            if (skills[index].SKILL_TYPE == ESkillType.SINGLE_TARGET)
            {
                if (skills[index].ATK_TYPE == EAtkType.NORMAL)
                {
                    // 기본 공격
                    int damage = Stat.ATK * (int)skills[index].SKILL_AB_01;
                    GetCurrentTarget.SetDamage(damage);
                }
                else if (skills[index].ATK_TYPE == EAtkType.SKILL)
                {
                    if (skills[index].SKILL_ABT_01 == ESkillAbilityType.DAMAGE_TARGET_MAX_HP_P)
                    {
                        // 체력 퍼센트 감소 스킬 (3챕터 중간보스 스킬)

                        BaseController target = GetCurrentTarget.GetTargetObject.GetComponent<BaseController>();
                        int percentDamage = target.Stats._maxHp * ((int)skills[index].SKILL_AB_01 / 100);
                        
                        StartCoroutine(DotAttack(index, percentDamage));
                    }
                    else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK_MULT)
                    {
                        // 투사체 발사 - 경로상 모든 플레이어 데미지 (3챕터 메인보스 스킬)
                    }
                }
            }
            else if (skills[index].SKILL_TYPE == ESkillType.ALL_TARGET)
            {
                if (skills[index].SKILL_ABT_01 == ESkillAbilityType.HP)
                {
                    // 플레이어 체력 감소
                    RangeDetect(skills[index]);

                    if (count <= 0) return;

                    for (int i = 0; i < count; i++)
                    {
                        if (Colliders[i].TryGetComponent(out BaseCharacter target))
                        {
                            target.SetDamage((int)skills[index].SKILL_AB_01);
                        }
                    }
                }

                else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK_SPEED_P ||
                         skills[index].SKILL_ABT_01 == ESkillAbilityType.CC)
                {
                    // 플레이어 이동속도, 공격속도 감소 or 플레이어 CC
                    RangeDetect(skills[index]);

                    if (count <= 0) return;

                    StartCoroutine(SkillDurate(index, count));
                }

                else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.SKILL_CD)
                {
                    // 플레이어 스킬의 쿨타임 증가
                    RangeDetect(skills[index]);

                    if (count <= 0) return;

                    SkillCoolDown(index, count);
                }
            }
        }
        else if (skills[index].SKILL_AT == ETargetType.ALLY)
        {
            if (skills[index].SKILL_ABT_01 == ESkillAbilityType.HP)
            {
                // 몬스터의 체력 회복
                RangeDetect(skills[index]);

                if (count <= 0) return;

                for (int i = 0; i < count; i++)
                {
                    if (Colliders[i].TryGetComponent(out ITargetable target))
                    {
                        target.SetHeal((int)skills[index].SKILL_AB_01);
                    }
                }
            }

            else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK)
            {
                // 몬스터 공격력 증가
                RangeDetect(skills[index]);

                if (count <= 0) return;

                for (int i = 0; i < count; i++)
                {
                    if (Colliders[i].TryGetComponent(out BaseMonster controller))
                    {
                        controller.Stat.ATK += (int)skills[index].SKILL_AB_01;
                    }
                }
            }
        }
        else if (skills[index].SKILL_AT == ETargetType.SELF)
        {
            if (skills[index].SKILL_ABT_01 == ESkillAbilityType.NORMAL_ATK_IMMUNITY)
            {
                
            }
        }
        */
    }
    
    private IEnumerator DotAttack(int index, int damage)
    {
        _durateTimer = 0f;
        
        while (_durateTimer < BaseSkill.skills[index].SKILL_DURATION)
        {
            GetCurrentTarget.SetDamage(damage);
            
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator SkillDurate(int index, int count)
    {
        float originMoveSpeed = 0f;
        float originAttackSpeed = 0f;
        
        for (int i = 0; i < count; i++)
        {
            if (Colliders[i].TryGetComponent(out BaseCharacter target))
            {
                if (BaseSkill.skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK_SPEED_P)
                {
                    var stat = target.Stats;
                    
                    originMoveSpeed = stat._moveSpeed;
                    originAttackSpeed = stat._attackSpeed;
                    
                    stat._moveSpeed = stat._moveSpeed + (stat._moveSpeed * (BaseSkill.skills[index].SKILL_AB_01 / 100));
                    stat._attackSpeed = stat._attackSpeed + (stat._attackSpeed * (BaseSkill.skills[index].SKILL_AB_02 / 100));
                }
                
                else if (BaseSkill.skills[index].SKILL_ABT_01 == ESkillAbilityType.CC)
                {
                    // 모든 플레이어 CC 이동정지 및 공격 정지 상태
                }
            }
        }
            
        yield return new WaitForSeconds(BaseSkill.skills[index].SKILL_DURATION);
        
        for (int i = 0; i < count; i++)
        {
            if (Colliders[i].TryGetComponent(out BaseCharacter target))
            {
                if (BaseSkill.skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK_SPEED_P)
                {
                    var stat = target.Stats;
                    stat._moveSpeed = originMoveSpeed;
                    stat._attackSpeed = originAttackSpeed;
                }
                
                else if (BaseSkill.skills[index].SKILL_ABT_01 == ESkillAbilityType.CC)
                {
                    // 모든 플레이어 CC 이동정지 및 공격 정지 상태
                }
            }
        }
    }

    private void SkillCoolDown(int index, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (Colliders[i].TryGetComponent(out BaseCharacter target))
            {
                target.AttackTimer -= BaseSkill.skills[index].SKILL_AB_01;
                if (target.AttackTimer < 0)
                {
                    target.AttackTimer = 0f;
                }
            }
        }
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
            Target = new Vector2(transform.position.x, GetCurrentTarget.GetTargetObject.transform.position.y);
        }
    }

    private void OnCheckDeath(int value)
    {
        if (value <= 0)
        {
            CurrentState.Value = EStateType.Die;
        }
    }

    private void OnAttacked(int value)
    {
        if (value < _stats._maxHp)
        {
            if (!_isActive)
            {
                _isActive = true;
                hpBarCanvas.gameObject.SetActive(_isActive);
                CurrentHp.Value = CurrentHp.Value;
            }
        }
    }
}
