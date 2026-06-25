using UnityEngine;

public class DAMAGE_REDUCTION_P : BaseEffect
{
    public DAMAGE_REDUCTION_P(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        BaseSkill.isReduction = true;
    }
}
