using UnityEngine;

public class ATK : BaseEffect
{
    public ATK(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        var stats = user.Stats;
        
        Service.Get<EffectManager>().SpawnEffect(skill.SKILL_FX, target.GetTargetObject.transform.position, Quaternion.identity);

        stats._attackPower += (int)skill.SKILL_AB_01;
    }
}
