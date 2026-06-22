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

        stats._attackPower += (int)skill.SKILL_AB_01;
    }
}
