using System;
using UnityEngine;

public class BASE_SKILL_WARRIOR : BaseEffect
{
    public BASE_SKILL_WARRIOR(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        target.SetDamage((int)(user.CurrentStats._attackPower * skill.SKILL_AB_02), skill);
    }
}
