using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSkill : MonoBehaviour
{
    private Dictionary<ESkillAbilityType, BaseEffect> _effects = new Dictionary<ESkillAbilityType, BaseEffect>();

    private BaseController _controller;

    public List<Skill> skills = new List<Skill>();
    public List<Collider2D> Colliders = new List<Collider2D>(10);    
    public int count;

    public bool isDurationActive;

    #region skillList
    
    private ATTACK_BASE _attackBase;
    private HP _hp;
    
    #endregion
    

    private void Awake()
    {
        _controller = GetComponent<BaseController>();

        isDurationActive = false;
        count = 0;

        #region skillListInit

        _attackBase = new ATTACK_BASE(this);
        _hp = new HP(this);

        #endregion
    }

    public void UseSkill(int index)
    {
        if (_controller.GetCurrentTarget == null) return;
        
        if (!_effects.ContainsKey(skills[index].SKILL_ABT_01)) InitEffect(skills[index].SKILL_ABT_01);

        if (skills[index].SKILL_TYPE == ESkillType.SINGLE_TARGET)
        {
            SetTarget(skills[index]);
            
            _effects[skills[index].SKILL_ABT_01].ApplyEffect(_controller, _controller.GetCurrentTarget, skills[index]);
        }
        else if (skills[index].SKILL_TYPE == ESkillType.ALL_TARGET)
        {
            RangeDetect(skills[index]);

            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                if (Colliders[i].TryGetComponent(out ITargetable target))
                {
                    _controller.SetCurrentTarget(target);
                    _effects[skills[index].SKILL_ABT_01].ApplyEffect(_controller, _controller.GetCurrentTarget, skills[index]);
                }
            }
        }
    }

    private void InitEffect(ESkillAbilityType type)
    {
        var effect = new BaseEffect();
        
        switch (type)
        {
            case ESkillAbilityType.ATTACK_BASE:
                effect = _attackBase;
                break;
            case ESkillAbilityType.HP:
                effect = _hp;
                break;
        }
        
        _effects.Add(type, effect);
    }
    
    public void RangeDetect(Skill skill)
    {
        ContactFilter2D filter = new ContactFilter2D();

        if (skill.SKILL_AT == ETargetType.ENEMY)
        {
            filter = _controller.EnemyFilter;
        }
        else if (skill.SKILL_AT == ETargetType.ALLY)
        {
            filter = _controller.AllyFilter;
        }

        count = Physics2D.OverlapCircle(transform.position, skill.SKILL_IS, filter, Colliders);
    }

    public void SetTarget(Skill skill)
    {
        if (skill.SKILL_AT == ETargetType.SELF)
        {
            _controller.SetCurrentTarget(_controller);
        }
        else if (skill.SKILL_AT == ETargetType.ALLY)
        {
            // 사제 타겟 체크 로직
        }
    }

    public void StartDuration(ITargetable target, Skill skill)
    {
        StartCoroutine(DurationCor(target, skill));
    }

    private IEnumerator DurationCor(ITargetable target, Skill skill)
    {
        float originAttackSpeed = 0f;

        target.GetTargetObject.TryGetComponent(out BaseController targetObject);

        var stat = _controller.Stats;
        
        originAttackSpeed = stat._attackSpeed;
        
        stat._attackSpeed += stat._attackSpeed * (skill.SKILL_DURATION / 100f);

        yield return new WaitForSeconds(skill.SKILL_DURATION);

        stat._attackSpeed = originAttackSpeed;
    }
}
