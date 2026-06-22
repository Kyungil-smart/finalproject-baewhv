using UnityEngine;

public class ATK : BaseEffect
{
    public ATK(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        
    }
}
