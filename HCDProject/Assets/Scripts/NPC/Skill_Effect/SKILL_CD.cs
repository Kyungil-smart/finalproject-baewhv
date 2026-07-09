using UnityEngine;

public class SKILL_CD : BaseEffect
{
    public SKILL_CD(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        if (target.GetTargetObject.TryGetComponent(out BaseCharacter character))
        {
            character.ActiveSkillCoolCount -= skill.SKILL_AB_01;
            if (character.ActiveSkillCoolCount < 0f)
            {
                character.ActiveSkillCoolCount = 0f;
            }
        }
    }
}
