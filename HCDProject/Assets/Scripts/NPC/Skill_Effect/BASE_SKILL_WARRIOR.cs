using System;
using UnityEngine;

public class BASE_SKILL_WARRIOR : BaseEffect
{
    public BASE_SKILL_WARRIOR(BaseSkill baseSkill)
    {
        BaseSkill = baseSkill;
    }

    public override void ApplyEffect(BaseController user, ITargetable target, Skill skill)
    {
        if (target == null) return;
        Vector2 dir = (target.GetTargetObject.transform.position - user.transform.position).normalized;
        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        Vector2 bestDir = Vector2.right;
        float bestDot = float.MinValue;

        foreach (Vector2 candidate in directions)
        {
            float dot = Vector2.Dot(dir, candidate);
            if (dot > bestDot)
            {
                bestDir = candidate;
                bestDot = dot;
            }
        }
        Vector2 node = (Vector2)user.transform.position + bestDir * (skill.SKILL_RANGE_X * 0.5f);

        int count = Physics2D.OverlapBox(node, new Vector2(skill.SKILL_RANGE_X, skill.SKILL_RANGE_Y),
            0f, user.EnemyFilter, user.Colliders);
        Debug.Log($"[전사 스킬] 범위 공격 → {count}명 적중");
        for (int i = 0; i < count; i++)
        {
            if (user.Colliders[i].TryGetComponent(out ITargetable targets))
            {
                targets.SetDamage((int)(user.Stats._attackPower * skill.SKILL_AB_02));
            }
        }
    }
}
