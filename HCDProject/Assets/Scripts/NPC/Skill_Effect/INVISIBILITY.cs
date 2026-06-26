using UnityEngine;

public class INVISIBILITY : BaseEffect
{
    public INVISIBILITY(BaseSkill baseSKill)
    {
        BaseSkill = baseSKill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        BaseSkill.StartDuration(target, skill, skill.SKILL_AB_01);
    }
}
