using UnityEngine;

public class DAMAGE_TARGET_MAX_HP_P : BaseEffect
{
    public DAMAGE_TARGET_MAX_HP_P(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }
    
    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        BaseSkill.StartDuration(target, skill, skill.SKILL_AB_01);
    }
}
