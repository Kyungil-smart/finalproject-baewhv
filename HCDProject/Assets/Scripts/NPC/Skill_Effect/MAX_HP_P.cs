using UnityEngine;

public class MAX_HP_P : BaseEffect
{
    public MAX_HP_P(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        if (user as BaseCharacter)
        {
            var characters = Service.Get<PlayerManager>()?.Characters;
            foreach (BaseCharacter chr in characters)
            {
                if (!chr._isDead)
                    chr.SetHeal((int)(chr.Stats._maxHp * skill.SKILL_AB_01 / 100));
                else
                {
                    Service.Get<PlayerManager>()?.ImmediateRevive(chr);
                    Debug.Log($"[부활] {chr.gameObject.name} 즉시 부활!");
                }
            } 
        }
        else if (user as BaseMonster)
        {
            target.SetHeal((int)(user.Stats._maxHp * skill.SKILL_AB_01));
        }
    }
}
