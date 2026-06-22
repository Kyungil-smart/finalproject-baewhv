using Unity.Burst.CompilerServices;
using UnityEngine;

public class HP : BaseEffect
{
    public HP(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }
    
    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill, float value)
    {
        if (skill.SKILL_AT == ETargetType.ALLY)
        {
            target.SetHeal((int)value);
        }
        else if(skill.SKILL_AT == ETargetType.ENEMY)
        {
            target.SetDamage((int)value);
        }
    }
}
