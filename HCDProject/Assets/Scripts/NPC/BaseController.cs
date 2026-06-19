using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController : MonoBehaviour, ITargetable
{
    [SerializeField] protected CharacterStats _stats;

    public CharacterStats Stats
    {
        get => _stats;
        set
        {
            _stats = value;
        }
    }

    [SerializeField] protected RatioIntValue CurrentHp = new RatioIntValue(0);


    public bool IsAlive()
    {
        return CurrentHp.Value > 0;
    }

    public List<Skill> skills = new List<Skill>();

    protected ESkillSlot SkillIndex;

    public CharacterMovement Movement { get; private set; }

    protected ITargetable _currentTarget;
    public ITargetable GetCurrentTarget => _currentTarget;


    public abstract void SetCurrentTarget(ITargetable target);

    public GameObject GetTargetObject { get; set; }

    protected BaseSkill BaseSkill;
    public List<Collider2D> Colliders = new List<Collider2D>(10);
    [field:SerializeField] public ContactFilter2D EnemyFilter;
    [field:SerializeField] public ContactFilter2D AllyFilter;

    private List<ITargetable> _targets = new List<ITargetable>();

    private CircleCollider2D _baseCollider;

    protected virtual void Awake()
    {
        GetTargetObject = gameObject;
        Movement = GetComponent<CharacterMovement>();
        _baseCollider = GetComponent<CircleCollider2D>();
        BaseSkill = GetComponent<BaseSkill>();

        EnemyFilter.useLayerMask = true;
        EnemyFilter.useTriggers = false;
        AllyFilter.useLayerMask = true;
        AllyFilter.useTriggers = false;
    }

    protected virtual void OnEnable()
    {
        CurrentHp.Value = _stats._maxHp;
    }

    public abstract void UseSkill(int index);

    public virtual int UseCritDamage(int baseDamage) // 플레이어 크리티컬 적용
    {
        return baseDamage;
    }

    public List<ITargetable> Detect(float range, ETargetType targetType)
    {
        _targets.Clear();
        
        int count = Physics2D.OverlapCircle(transform.position,
            range,
            targetType == 0 ? EnemyFilter : AllyFilter,
            Colliders);

        for (int i = 0; i < count; i++)
        {
            if (Colliders[i].TryGetComponent(out ITargetable target))
            {
                _targets.Add(target);
            }
        }

        return _targets;
    }
    public void AttackRange(int index, int damage)
    {
        List<Collider2D> Colliders = new List<Collider2D>(); // 범위에 들어온 대상을 넣어둘 주머니

        ContactFilter2D filter;

        if (skills[index].SKILL_AT == ETargetType.ENEMY)
        {
            filter = EnemyFilter;
        }

        else
        {
            filter = AllyFilter;
        }

        int count = Physics2D.OverlapCircle(transform.position, skills[index].SKILL_IS,
            filter, Colliders); // 범위 들어온 대상을 체크하고, 그후 저장

        for (int i = 0; i < count; i++) // 대상을 하나 씩 체크
        {
            if (Colliders[i].TryGetComponent(out ITargetable target)) // ITargetable의 대상으로 꺼내온다.
            {
                int rangeDamage = UseCritDamage(damage); // 플레이어의 치명타 대상인지 체크
                target.SetDamage(rangeDamage); // 맞다면 범위공격을 전달
            }
        }
    }

    public void SetDamage(int damage)
    {
        int def = Mathf.Max(damage - _stats._defense, 0);
        
        CurrentHp.Value -= def;

        if (CurrentHp.Value < 0)
        {
            CurrentHp.Value = 0;
        }
    }

    public void SetHeal(int heal)
    {
        int overHp = Mathf.Min(_stats._maxHp, CurrentHp.Value + heal);

        CurrentHp.Value = overHp;
    }

    public void SetBuff(float buff)
    {
        _stats._attackSpeed = buff;
    }

    public float GetRadius()
    {
        return _baseCollider.radius;
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Stats._chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Stats._attackRange);
    }

}
