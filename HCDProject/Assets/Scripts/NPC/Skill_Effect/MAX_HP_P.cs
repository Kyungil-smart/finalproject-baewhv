using UnityEngine;

public class MAX_HP_P : BaseEffect
{
    public MAX_HP_P(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        if (user is BaseCharacter)
        {
            var characters = Service.Get<PlayerManager>()?.Characters;
            foreach (BaseCharacter chr in characters)
            {
                if (!chr._isDead)
                    chr.SetHeal((int)(chr.CurrentStats._maxHp * skill.SKILL_AB_01 / 100));
                else
                {
                    Service.Get<PlayerManager>()?.ImmediateRevive(chr);
                    
                }
            } 
        }
        else if (user is BaseMonster)
        {
            user.SetHeal((int)(user.CurrentStats._maxHp * skill.SKILL_AB_01));
        }
    }
}
