using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController : MonoBehaviour, ITargetable
{
    [SerializeField] private CharacterStats _stats;
    public CharacterStats Stats => _stats;

    protected ObserveValue<int> CurrentHp = new ObserveValue<int>();

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
    
    private List<ITargetable> _targets = new List<ITargetable>();

    protected virtual void Awake()
    {
        GetTargetObject = gameObject;
        Movement = GetComponent<CharacterMovement>();
        CurrentHp.Value = _stats._maxHp;
        
        EnemyFilter.useLayerMask = true;
        EnemyFilter.useTriggers = false;
        AllyFilter.useLayerMask = true;
        AllyFilter.useTriggers = false;
    }

    public void UseSkill(int index)
    {
        List<ITargetable> skillTargets = Detect(skills[index].skillRange, skills[index].TargetType);
        
        if (skillTargets == null) return;

        foreach (ITargetable target in skillTargets)
        {
            if (skills[index].TargetType == ETargetType.Enemy)
            {
                int finalDamage = UseCritDamage(skills[index].skillDamage);
                target.SetDamage(finalDamage);
            }
            else
            {
                target.SetHeal(skills[index].skillDamage);
            }
        }
    }

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

    public void SetDamage(int damage)
    {
        int def = Mathf.Max(damage - _stats._defense, 0);
        CurrentHp.Value -= def;
    }

    public void SetHeal(int heal)
    {
        int overHp = Mathf.Min(_stats._maxHp, CurrentHp.Value + heal);

        CurrentHp.Value = overHp;
    }
}
