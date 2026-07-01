using UnityEngine;

public class ATTACK_BASE : BaseEffect
{
    public ATTACK_BASE(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }
    
    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        int skillID = int.Parse(skill.SKILL_ID.Trim());
        if (skillID >= 6035 && skillID <= 6048)
        {
            Service.Get<EffectManager>().SpawnEffect(skill.SKILL_FX, target.GetTargetObject.transform.position, Quaternion.identity);
        }
        
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
