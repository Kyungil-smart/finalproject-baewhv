using UnityEngine;

public class BaseEffect
{
    protected BaseSkill BaseSkill;
    
    public virtual void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
    }
}
