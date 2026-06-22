using UnityEngine;

public class CC : BaseEffect
{
    public CC(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }
    
    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        // 플레이어 정지
        BaseSkill.StartDuration(target, skill, skill.SKILL_AB_01);
    }
}
