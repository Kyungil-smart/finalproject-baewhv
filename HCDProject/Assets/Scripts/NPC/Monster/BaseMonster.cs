using System;
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
    
    private float _timer;
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
        
        _timer = 0f;
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

    public float DistanceToTarget(Transform target)
    {
        return Vector2.Distance(transform.position, target.position);
    }
    
    private void CheckDeath(int value)
    {
        if (value <= 0)
        {
            CurrentState.Value = EStateType.Die;
        }
    }
}
