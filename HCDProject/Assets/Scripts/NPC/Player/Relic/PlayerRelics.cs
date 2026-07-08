using System.Collections;
using UnityEngine;

public class PlayerRelics : MonoBehaviour
{
    private BaseCharacter _character;
    private string _jobType;
    private float _shieldDuration;
    private void Awake()
    {
        _character = GetComponent<BaseCharacter>();
    }

    public void Init(string jobType)
    {
        _jobType = jobType;
        _shieldDuration = Service.Get<RelicManager>()?
        .GetTotalRelicBonus(_jobType, "SHIELD_DURATION") ?? 0f;
    }

    public void TryMagicBow(ITargetable target, Skill skill) // 마법 활
    {
        float rate = _character.PlayerStat._doubleAtkRate;
        if (rate <= 0) return;

        if(Random.value < rate)
        {
            target.SetDamage((int)(_character.CurrentStats._attackPower * skill.SKILL_AB_01), skill);
        }
    }
    public void TryShield() // 보호막 마법
    {
        if (_shieldDuration <= 0) return;

        var characters = Service.Get<PlayerManager>()?.Characters;
        if (characters == null) return;
        foreach(BaseCharacter chr in characters)
        {
            if (chr._isDead) continue;
            StartCoroutine(ShieldCor(chr, _shieldDuration));
        }
        //Debug.Log($"[보호막] {_shieldDuration}초 동안 모든 아군 무적!");
    }

    private IEnumerator ShieldCor(BaseCharacter target, float duration)
    {
        target.isInvincible = true;
        yield return new WaitForSeconds(duration);
        target.isInvincible = false;
    }
}
