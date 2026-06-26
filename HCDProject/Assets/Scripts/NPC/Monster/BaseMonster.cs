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
        ResetTarget();

        State?.Update();
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
