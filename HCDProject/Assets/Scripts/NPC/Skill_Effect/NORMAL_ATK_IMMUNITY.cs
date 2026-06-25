using UnityEngine;

public class NORMAL_ATK_IMMUNITY : BaseEffect
{
    public NORMAL_ATK_IMMUNITY(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        BaseSkill.isNormalImmunity = true;
    }
}
