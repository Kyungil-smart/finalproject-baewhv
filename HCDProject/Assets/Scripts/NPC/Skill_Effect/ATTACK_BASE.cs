using UnityEngine;

public class ATTACK_BASE : BaseEffect
{
    public ATTACK_BASE(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }
    
    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        if (BaseSkill.isIgnoreDef)
        {
            target.GetTargetObject.TryGetComponent(out BaseController targetObject);
            if (targetObject == null) return;

            var stat = targetObject.Stats;
            
            target.SetDamage((int)(user.Stats._attackPower * skill.SKILL_AB_01) + stat._defense, skill);
        }
        else
        {
            for (int i = 0; i < BaseSkill.attackHitCount; i++)
            {
                target.SetDamage((int)(user.Stats._attackPower * skill.SKILL_AB_01), skill);
            }
        }
    }
}
