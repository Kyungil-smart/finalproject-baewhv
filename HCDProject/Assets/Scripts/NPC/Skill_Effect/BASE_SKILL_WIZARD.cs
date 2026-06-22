using UnityEngine;

public class BASE_SKILL_WIZARD : BaseEffect
{
    public BASE_SKILL_WIZARD(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        Vector2 center = target.GetTargetObject.transform.position;
        int damage = (int)(user.Stats._attackPower * skill.SKILL_AB_02);
        BaseSkill.StartDotField(center, skill, damage);
    }
}
