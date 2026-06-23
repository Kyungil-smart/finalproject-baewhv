using UnityEngine;

public class SKILL_CD : BaseEffect
{
    public SKILL_CD(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        target.GetTargetObject.TryGetComponent(out BaseCharacter character);

        character.AttackTimer -= skill.SKILL_AB_01;
        if (character.AttackTimer < 0f)
        {
            character.AttackTimer = 0f;
        }
    }
}
