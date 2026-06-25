using UnityEngine;

public class ATK_MULT : BaseEffect
{
    public ATK_MULT(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        target.SetDamage((int)(user.Stats._attackPower * skill.SKILL_AB_01));
    }
}
