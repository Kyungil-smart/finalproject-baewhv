using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController : MonoBehaviour, ITargetable
{
    [SerializeField] private CharacterStats _stats;
    public CharacterStats Stats => _stats;
    
    [SerializeField] List<Skill> skills = new List<Skill>();

    protected ESkillType SkillIndex;
    
    public CharacterMovement Movement { get; private set; }
    
    protected ITargetable _currentTarget;

    public ITargetable GetCurrentTarget => _currentTarget;

    public abstract void SetCurrentTarget(ITargetable target);

    public GameObject GetTargetObject { get; set; }
    
    protected List<Collider2D> Colliders = new List<Collider2D>(10);
    protected ContactFilter2D Filter;
    

    protected virtual void Awake()
    {
        GetTargetObject = gameObject;
        Movement = GetComponent<CharacterMovement>();
        
        Filter = new ContactFilter2D();
        
        Filter.useLayerMask = true;
        Filter.SetLayerMask(LayerMask.GetMask("Player"));
        Filter.useTriggers = false;
    }

    public void UseSkill(int index)
    {
        ITargetable skillTarget = Detect(skills[index].skillRange);

        if (skillTarget == null) return;

        skillTarget.SetDamage();
    }
    
    public ITargetable Detect(float range)
    {
        int count = Physics2D.OverlapCircle(transform.position,
            range,
            Filter,
            Colliders);

        if (count > 0)
        {
            GameObject target = null;
            float minDistanceSqr = range * range;
        
            for (int i = 0; i < count; i++)
            {
                float distanceSqr = (transform.position - Colliders[i].transform.position).sqrMagnitude;

                if (minDistanceSqr > distanceSqr)
                {
                    minDistanceSqr = distanceSqr;
                    target = Colliders[i].gameObject;
                }
            }
            return target?.GetComponent<ITargetable>();
        }
        return null;
    }

    public abstract void SetDamage();

    public abstract void SetHeal();
}
