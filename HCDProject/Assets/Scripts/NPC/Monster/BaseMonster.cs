using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class BaseMonster : BaseController
{
    [field:SerializeField] public MonsterRawData Stat { get; private set;}
    public int MonsterID { get; private set; }
    public int PrefabIndex { get; set; }
    
    #region State
    private protected StateMachine State;
    [field:SerializeField] public ObserveValue<EStateType> CurrentState { get; private set; }
    public MonsterIdleState IdleState { get; protected set; }
    public MonsterChaseState ChaseState { get; protected set; }
    public MonsterAttackState AttackState { get; protected set; }
    public MonsterDieState DieState { get; protected set; }
    #endregion
    
    public Vector3 Target { get; set; }
    
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
        CurrentHp.Value = Stat.HP;
        _stats._maxHp = Stat.HP;
        _stats._defense = Stat.DEF;
        Movement.Agent.speed = Stat.MOVE_SPEED;
    }
    
    private void InitSkill(MonsterRawData data)
    {
        skills.Clear();
        
        // 몬스터의 공격(스킬) 데이터 초기화
        var skillDataTable = Service.Get<DataManager>().MonsterSkillTable.data;
        MonsterSkillRawData atkData = skillDataTable.Find(x => x.SKILL_ID == data.ATK_ID);
        if (atkData != null) skills.Add(new Skill(atkData));

        MonsterSkillRawData skillData = skillDataTable.Find(x => x.SKILL_ID == data.SKILL_ID);
        if (skillData != null) skills.Add(new Skill(skillData));
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
        if (GetCurrentTarget == null) return;

        if (skills[index].ATK_TYPE == EAtkType.NORMAL)
        {
            GetCurrentTarget.SetDamage(Stat.ATK);
        } 
        else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.HP)
        {
            RangeDetect(index);
            
            if (Count <= 0) return;

            for (int i = 0; i < Count; i++)
            {
                if (Colliders[i].TryGetComponent(out ITargetable target))
                {
                    target.SetHeal((int)skills[index].SKILL_AB_01);
                }
            }
        }
        else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK)
        {
            RangeDetect(index);
            
            if (Count <= 0) return;

            for (int i = 0; i < Count; i++)
            {
                if (Colliders[i].TryGetComponent(out BaseMonster controller))
                {
                    controller.Stat.ATK += (int)skills[index].SKILL_AB_01;
                }
            }
        }
    }

    public void ReSkill(int index)
    {
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
                        // 모든 플레이어 체력 감소 스킬
                    }
                    else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK_MULT)
                    {
                        // 투사체 발사 (경로상 모든 플레이어 데미지)
                    }
                }
            }
            else if (skills[index].SKILL_TYPE == ESkillType.ALL_TARGET)
            {
                if (skills[index].SKILL_ABT_01 == ESkillAbilityType.HP)
                {
                    // 플레이어 체력 감소
                    RangeDetect(index);
            
                    if (Count <= 0) return;

                    for (int i = 0; i < Count; i++)
                    {
                        if (Colliders[i].TryGetComponent(out BaseCharacter target))
                        {
                            target.SetDamage((int)skills[index].SKILL_AB_01);
                        }
                    }
                }
                
                else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK_SPEED_P)
                {
                    // 플레이어 이동속도, 공격속도 감소
                    RangeDetect(index);

                    if (Count <= 0) return;

                    for (int i = 0; i < Count; i++)
                    {
                        if (Colliders[i].TryGetComponent(out BaseCharacter target))
                        {
                            var stat = target.Stats;
                            stat._moveSpeed *= 0.5f;
                            stat._attackSpeed *= 0.5f;
                        }
                    }
                }
            }
        }
        else if (skills[index].SKILL_AT == ETargetType.ALLY)
        {
            
        }
        else if (skills[index].SKILL_AT == ETargetType.SELF)
        {
            
        }
    }

    public void RangeDetect(int index)
    {
        ContactFilter2D filter = new ContactFilter2D();

        if (skills[index].SKILL_AT == ETargetType.ENEMY)
        {
            filter = EnemyFilter;
        }
        else if (skills[index].SKILL_AT == ETargetType.ALLY)
        {
            filter = AllyFilter;
        }

        Count = Physics2D.OverlapCircle(transform.position, skills[index].SKILL_IS, filter, Colliders);
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
            Target = new Vector3(transform.position.x, GetCurrentTarget.GetTargetObject.transform.position.y, transform.position.z);
        }
    }

    private IEnumerator SkillDurate(int index, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (Colliders[i].TryGetComponent(out BaseCharacter target))
            {
                if (skills[index].SKILL_ABT_01 == ESkillAbilityType.ATK_SPEED_P)
                {
                    var stat = target.Stats;
                    stat._moveSpeed *= 0.5f;
                    stat._attackSpeed *= 0.5f;
                }
                
                else if (skills[index].SKILL_ABT_01 == ESkillAbilityType.CC)
                {
                    
                }

            }
        }
            
        yield return new WaitForSeconds(skills[index].SKILL_DURATION);
    }

    private void CheckDeath(int value)
    {
        if (value <= 0)
        {
            CurrentState.Value = EStateType.Die;
        }
    }
}
