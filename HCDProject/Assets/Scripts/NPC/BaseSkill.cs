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
    public int attackHitCount = 1; // 궁수 3타 카운트

    public bool isDurationActive;

    #region skillList
    
    private ATTACK_BASE _attackBase;
    private HP _hp;
    private BASE_SKILL_WARRIOR _baseSkillWarrior; // 워리어 액티브
    private ATK_SPEED_P _atkSpeedP;
    private MAX_HP_P _maxHpP;
    private BASE_SKILL_WIZARD _baseSkillWizard;
    
    #endregion
    
    private void Awake()
    {
        _controller = GetComponent<BaseController>();

        isDurationActive = false;
        count = 0;

        #region skillListInit

        _attackBase = new ATTACK_BASE(this);
        _hp = new HP(this);
        _baseSkillWarrior = new BASE_SKILL_WARRIOR(this); // 워리어 액티브
        _atkSpeedP = new ATK_SPEED_P(this);
        _maxHpP = new MAX_HP_P(this);
        _baseSkillWizard = new BASE_SKILL_WIZARD(this);

        #endregion
    }

    public void UseSkill(int index)
    {
        if (_controller.GetCurrentTarget == null) return;

        // ABT_01이 NONE이면 실행할 효과 없음
        if (skills[index].SKILL_ABT_01 == ESkillAbilityType.NONE) return;

        // 효과 등록
        if (!_effects.ContainsKey(skills[index].SKILL_ABT_01)) InitEffect(skills[index].SKILL_ABT_01);

        if (skills[index].SKILL_ABT_01 == ESkillAbilityType.MAX_HP_P
            || skills[index].SKILL_ABT_01 == ESkillAbilityType.BASE_SKILL_WIZARD)
        {
            _effects[skills[index].SKILL_ABT_01].ApplyEffect(
                _controller, _controller.GetCurrentTarget, skills[index]);
            return;
        }
        if (skills[index].SKILL_TYPE == ESkillType.SINGLE_TARGET)
        {
            SetTarget(skills[index]);

            _effects[skills[index].SKILL_ABT_01].ApplyEffect(_controller,
                _controller.GetCurrentTarget, skills[index]);
        }
        else if (skills[index].SKILL_TYPE == ESkillType.ATTACK_OF_SCOPE || skills[index].SKILL_TYPE == ESkillType.ALL_TARGET)
        {
            Vector2 center = _controller.GetCurrentTarget.GetTargetObject.transform.position;
            RangeDetect(skills[index], _controller, _controller.GetCurrentTarget, center);
            
            if (count <= 0) return;
            
            for (int i = 0; i < count; i++)
            {
                if (Colliders[i].TryGetComponent(out ITargetable target))
                {
                    _controller.SetCurrentTarget(target);
                    _effects[skills[index].SKILL_ABT_01].ApplyEffect(
                        _controller, _controller.GetCurrentTarget, skills[index]);
                }
            }
        }
    }
    
    /*private float CalculateValue(int index)
    {
        Skill skill = skills[index];

        // Case 1: ABT_02가 ATTACK_BASE → 공격력 × AB_02 배수
        if (skill.SKILL_ABT_02 == ESkillAbilityType.ATTACK_BASE)
        {
            return _controller.Stats._attackPower * skill.SKILL_AB_02;
        }

        // Case 2: ABT_01이 ATTACK_BASE → 공격력 × AB_01 배수
        if (skill.SKILL_ABT_01 == ESkillAbilityType.ATTACK_BASE)
        {
            return _controller.Stats._attackPower * skill.SKILL_AB_01;
        }

        // Case 3: 그 외 → AB_01 값 그대로
        // 예: 궁수 버프(ATK_SPEED_P, 30), 힐러 액티브(MAX_HP_P, 15)
        return skill.SKILL_AB_01;
    }*/

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
            case ESkillAbilityType.BASE_SKILL_WARIOR: // 워리어 액티브 스킬추가
                effect = _baseSkillWarrior;
                break;
            case ESkillAbilityType.ATK_SPEED_P:
                effect = _atkSpeedP;
                break;
            case ESkillAbilityType.MAX_HP_P:
                effect = _maxHpP;
                break;
            case ESkillAbilityType.BASE_SKILL_WIZARD:
                effect = _baseSkillWizard;
                break;
        }
        
        _effects.Add(type, effect);
    }
    public void RangeDetect(Skill skill, BaseController user, ITargetable target, Vector2 center)
    {
        // 1. 필터 설정
        ContactFilter2D filter = new ContactFilter2D();
        if (skill.SKILL_AT == ETargetType.ENEMY)
            filter = _controller.EnemyFilter;
        else if (skill.SKILL_AT == ETargetType.ALLY)
            filter = _controller.AllyFilter;

        /*if (skill.SKILL_RANGE_TYPE == ERangeType.CIRCLE)
        {
            // 마법사 장판, 궁수 화살비: center 위치 기준 원형 탐지
            count = Physics2D.OverlapCircle(center, skill.SKILL_RANGE_X, filter, Colliders);
        }
        else if (skill.SKILL_RANGE_TYPE == ERangeType.BOX)
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
            count = Physics2D.OverlapBox(node,
                new Vector2(skill.SKILL_RANGE_X, skill.SKILL_RANGE_Y), 0f, filter, Colliders);
        }
        else // NONE: 시전자 위치 기준 (몬스터 ALL_TARGET 등)
        {
            count = Physics2D.OverlapCircle(user.transform.position, skill.SKILL_IS, filter, Colliders);
        }*/
    }
    public void SetTarget(Skill skill)
    {
        if (skill.SKILL_AT == ETargetType.SELF)
        {
            _controller.SetCurrentTarget(_controller);
        }
    }

    public void StartDotField(Vector2 center, Skill skill, int damage)
    {
        StartCoroutine(DotFieldCor(center, skill, damage));
    }

    private IEnumerator DotFieldCor(Vector2 center, Skill skill, int damage)
    {
        float elapsed = 0f;

        while (elapsed < skill.SKILL_DURATION)
        {
            RangeDetect(skill, _controller, null, center);

            for (int i = 0; i < count; i++)
            {
                if (Colliders[i].TryGetComponent(out ITargetable target))
                {
                    target.SetDamage(damage);
                }
            }

            Debug.Log($"[마법사 장판] {elapsed}초 틱 → {count}명 적중");
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }
    }

    public void StartDuration(ITargetable target, Skill skill, float value)
    {
        StartCoroutine(DurationCor(target, skill, value));
    }

    private IEnumerator DurationCor(ITargetable target, Skill skill, float value)
    {
        target.GetTargetObject.TryGetComponent(out BaseController targetObject);
        if (targetObject == null) yield break;

        var stat = targetObject.Stats;
        float originAttackSpeed = 0f;
        float originMoveSpeed = 0f;
        
        if (skill.SKILL_ABT_01 == ESkillAbilityType.ATK_SPEED_P)
        {
            originAttackSpeed = stat._attackSpeed;
            stat._attackSpeed *= (1f - value / 100f);
            
            if (skill.SKILL_ID == "6053")
            {
                originMoveSpeed = stat._moveSpeed;
                stat._moveSpeed *= (1f - value / 100f);
            }
            else if (skill.SKILL_ID == "6503")
            {
                // 궁수 버프(SELF)일 때만 3타 적용
                attackHitCount = 3;
            }
        }
        else if (skill.SKILL_ABT_01 == ESkillAbilityType.CC)
        {
            originMoveSpeed = stat._moveSpeed;
            
            targetObject.TryGetComponent(out BaseCharacter player);
            
            player.isCC = true;
            isDurationActive = true;
            stat._moveSpeed = 0f;
        }
        
        yield return new WaitForSeconds(skill.SKILL_DURATION);

        // 원상복구
        if (skill.SKILL_ABT_01 == ESkillAbilityType.ATK_SPEED_P)
        {
            stat._attackSpeed = originAttackSpeed;
            
            if (skill.SKILL_ID == "6053")
            {
                stat._moveSpeed = originMoveSpeed;
            }
            else if (skill.SKILL_ID == "6503")
            {
                attackHitCount = 1;
            }
        }
        else if (skill.SKILL_ABT_01 == ESkillAbilityType.CC)
        { 
            targetObject.TryGetComponent(out BaseCharacter player);
            
            player.isCC = false;
            isDurationActive = false;
            stat._moveSpeed = originMoveSpeed;
        }
    }

    public void OnCC(bool value)
    {
        if (value)
        {
            // 플레이어 액티브 스킬 UI 버튼 비활성화
            
        }
    }
}
