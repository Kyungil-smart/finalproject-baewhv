using UnityEngine;

public class PlayerRelics : MonoBehaviour
{
    private BaseCharacter _character;

    private void Awake()
    {
        _character = GetComponent<BaseCharacter>();
    }

    public void TryMagicBow(ITargetable target, Skill skill) // 마법 활
    {
        float rate = _character.PlayerStat._doubleAtkRate;
        if (rate <= 0) return;

        if(Random.value < rate)
        {
            target.SetDamage((int)(_character.Stats._attackPower * skill.SKILL_AB_01));
        }
    }
}
