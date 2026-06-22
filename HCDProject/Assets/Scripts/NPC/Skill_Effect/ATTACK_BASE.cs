using UnityEngine;

public class ATTACK_BASE : BaseEffect
{
    public ATTACK_BASE(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }
    
    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        for (int i = 0; i < BaseSkill.attackHitCount; i++)
        {
            target.SetDamage((int)(user.Stats._attackPower * skill.SKILL_AB_01));
        }
    }
}
