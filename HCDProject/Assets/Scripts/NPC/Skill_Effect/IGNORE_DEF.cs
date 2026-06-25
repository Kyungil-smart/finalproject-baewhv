using UnityEngine;

public class IGNORE_DEF : BaseEffect
{
    public IGNORE_DEF(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        BaseSkill.isIgnoreDef = true;
    }
}
