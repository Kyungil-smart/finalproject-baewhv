using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController : MonoBehaviour, ITargetable
{
    [SerializeField] private CharacterStats _stats;
    public CharacterStats Stats => _stats;

    private ObserveValue<int> _currentHp;

    [SerializeField] List<Skill> skills = new List<Skill>();

    protected ESkillType SkillIndex;
    
    public CharacterMovement Movement { get; private set; }
    
    protected ITargetable _currentTarget;

    public ITargetable GetCurrentTarget => _currentTarget;

    public abstract void SetCurrentTarget(ITargetable target);

    public GameObject GetTargetObject { get; set; }
    
    protected List<Collider2D> Colliders = new List<Collider2D>(10);
    [SerializeField] protected ContactFilter2D EnemyFilter;
    [SerializeField] protected ContactFilter2D AllyFilter;

    protected virtual void Awake()
    {
        GetTargetObject = gameObject;
        Movement = GetComponent<CharacterMovement>();
        
        EnemyFilter.useLayerMask = true;
        EnemyFilter.useTriggers = false;
        AllyFilter.useLayerMask = true;
        AllyFilter.useTriggers = false;
    }

    public void UseSkill(int index)
    {
        ITargetable skillTarget = Detect(skills[index].skillRange, skills[index].TargetType);
        if (skillTarget == null) return;

        if (skills[index].TargetType == ETargetType.Enemy)
        {
            int finalDamage = UseCritDamage(skills[index].skillDamage);
            skillTarget.SetDamage(finalDamage);
        }

        else
        {
           skillTarget.SetHeal(skills[index].skillDamage);
        }
    }

    public virtual int UseCritDamage(int baseDamage) // 플레이어 크리티컬 적용
    {
        return baseDamage;
    }
    
    public ITargetable Detect(float range, ETargetType targetType)
    {
        int count = Physics2D.OverlapCircle(transform.position,
            range,
            targetType == 0 ? EnemyFilter : AllyFilter,
            Colliders);
        
        Collider2D target = null;
        float minDistanceSqr = range * range;

        for (int i = 0; i < count; i++)
        {
            float distanceSqr = (transform.position - Colliders[i].transform.position).sqrMagnitude;

            if (minDistanceSqr > distanceSqr)
            {
                minDistanceSqr = distanceSqr;
                target = Colliders[i];
            }
        }

        if (target != null && target.TryGetComponent(out ITargetable targetable))
        {
            return targetable;
        }

        return null;
    }

    public void SetDamage(int damage)
    {
        int def = Mathf.Max(damage - _stats._defense, 0);
        _currentHp.Value -= def;
    }

    public void CheckDeath(int value)
    {
        if (value <= 0)
        {
            // 사망처리
        }
    }

    public void SetHeal(int heal)
    {
        int overHp = Mathf.Min(_stats._maxHp, _currentHp.Value + heal);

        _currentHp.Value = overHp;
    }
}
