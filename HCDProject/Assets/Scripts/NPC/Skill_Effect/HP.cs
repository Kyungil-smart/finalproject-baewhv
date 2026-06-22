using JetBrains.Annotations;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class HP : BaseEffect
{
    public HP(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        if (skill.SKILL_ABT_02 == ESkillAbilityType.ATTACK_BASE)
        {
            if (skill.SKILL_AT == ETargetType.ALLY)
            {
                // 사제 힐 로직
                target.SetHeal((int)(user.Stats._attackPower * skill.SKILL_AB_02));
            }

            return;
        }

        if (skill.SKILL_AT == ETargetType.ALLY)
        {
            target.SetHeal((int)skill.SKILL_AB_01);
        }
        else if (skill.SKILL_AT == ETargetType.ENEMY)
        {
            target.SetDamage((int)skill.SKILL_AB_01 * -1);
        }
    }
}
