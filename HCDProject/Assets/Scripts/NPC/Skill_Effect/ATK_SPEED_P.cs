using UnityEngine;

public class ATK_SPEED_P : BaseEffect
{
    public ATK_SPEED_P(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }
    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        if (skill.SKILL_AT == ETargetType.SELF)
        {
            BaseSkill.StartDuration(target, skill);
        }
    }
}
