using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSkill : MonoBehaviour
{
    private Dictionary<ESkillAbilityType, BaseEffect> _effects = new Dictionary<ESkillAbilityType, BaseEffect>();

    private BaseController _controller;
    private PlayerRelics _playerRelics;

    public List<Skill> skills = new List<Skill>();
    public List<Collider2D> Colliders = new List<Collider2D>(10);    
    public int count;
    public int attackHitCount = 1; // 궁수 3타 카운트

    public bool isDurationActive;
    private float _durateTimer;

    public bool isNormalImmunity = false;
    public bool isIgnoreDef = false;
    public bool isReduction = false;

    #region skillList
    
    private ATTACK_BASE _attackBase;
    private HP _hp;
    private BASE_SKILL_WARRIOR _baseSkillWarrior; // 워리어 액티브
    private ATK_SPEED_P _atkSpeedP;
    private MAX_HP_P _maxHpP;
    private BASE_SKILL_WIZARD _baseSkillWizard;
    private CC _cc;
    private ATK _atk;
    private DAMAGE_TARGET_MAX_HP_P _damageTargetMaxHpP;
    private INVISIBILITY _invisibility;
    private ATK_MULT _atkMult;
    private NORMAL_ATK_IMMUNITY _normalAtkImmunity;
    private IGNORE_DEF _ignoreDef;
    private DAMAGE_REDUCTION_P _damageReductionP;
    private SKILL_CD _skillCd;
    
    #endregion
    
    private void Awake()
    {
        _controller = GetComponent<BaseController>();
        _playerRelics = GetComponent<PlayerRelics>();

        isDurationActive = false;
        count = 0;

        #region skillListInit

        _attackBase = new ATTACK_BASE(this);
        _hp = new HP(this);
        _baseSkillWarrior = new BASE_SKILL_WARRIOR(this); // 워리어 액티브
        _atkSpeedP = new ATK_SPEED_P(this);
        _maxHpP = new MAX_HP_P(this);
        _baseSkillWizard = new BASE_SKILL_WIZARD(this);
        _atk = new ATK(this);
        _cc = new CC(this);
        _damageTargetMaxHpP = new DAMAGE_TARGET_MAX_HP_P(this);
        _invisibility = new INVISIBILITY(this);
        _atkMult = new ATK_MULT(this);
        _normalAtkImmunity = new NORMAL_ATK_IMMUNITY(this);
        _ignoreDef = new IGNORE_DEF(this);
        _damageReductionP = new DAMAGE_REDUCTION_P(this);
        _skillCd = new SKILL_CD(this);

        #endregion
    }

    private void Update()
    {
        if (isDurationActive) _durateTimer += Time.deltaTime;
    }

    public void UseSkill(int index)
    {
        if (_controller.GetCurrentTarget == null && skills[index].SKILL_ABT_01 != ESkillAbilityType.MAX_HP_P) return;

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
            ITargetable originalTarget = _controller.GetCurrentTarget;
            SetTarget(skills[index]);

            _effects[skills[index].SKILL_ABT_01].ApplyEffect(_controller, _controller.GetCurrentTarget, skills[index]);

            _controller.SetCurrentTarget(originalTarget);
            if (index == 0 && skills[index].SKILL_AT == ETargetType.ENEMY)
            {
                _playerRelics?.TryMagicBow(originalTarget, skills[index]);
            }

        }
        else if (skills[index].SKILL_TYPE == ESkillType.ATTACK_OF_SCOPE || skills[index].SKILL_TYPE == ESkillType.ALL_TARGET)
        {
            Vector2 center = _controller.GetCurrentTarget.GetTargetObject.transform.position;
            RangeDetect(skills[index], _controller.GetCurrentTarget, center);
            
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
        switch (type)
        {
            case ESkillAbilityType.ATTACK_BASE:
                _effects.Add(type, _attackBase);
                break;
            case ESkillAbilityType.HP:
                _effects.Add(type, _hp);
                break;
            case ESkillAbilityType.BASE_SKILL_WARIOR: // 워리어 액티브 스킬추가
                _effects.Add(type, _baseSkillWarrior);
                break;
            case ESkillAbilityType.ATK_SPEED_P:
                _effects.Add(type, _atkSpeedP);
                break;
            case ESkillAbilityType.MAX_HP_P:
                _effects.Add(type, _maxHpP);
                break;
            case ESkillAbilityType.BASE_SKILL_WIZARD:
                _effects.Add(type, _baseSkillWizard);
                break;
            case ESkillAbilityType.ATK:
                _effects.Add(type, _atk);
                break;
            case ESkillAbilityType.CC:
                _effects.Add(type, _cc);
                break;
            case ESkillAbilityType.DAMAGE_TARGET_MAX_HP_P:
                _effects.Add(type, _damageTargetMaxHpP);
                break;
            case ESkillAbilityType.INVISIBILITY:
                _effects.Add(type, _invisibility);
                break;
            case ESkillAbilityType.ATK_MULT:
                _effects.Add(type, _atkMult);
                break;
            case ESkillAbilityType.NORMAL_ATK_IMMUNITY:
                _effects.Add(type, _normalAtkImmunity);
                break;
            case ESkillAbilityType.IGNORE_DEF:
                _effects.Add(type, _ignoreDef);
                break;
            case ESkillAbilityType.DAMAGE_REDUCTION_P:
                _effects.Add(type, _damageReductionP);
                break;
            case ESkillAbilityType.SKILL_CD:
                _effects.Add(type, _skillCd);
                break;
        }
    }
    
    public void RangeDetect(Skill skill, ITargetable target, Vector2 center)
    {
        // 1. 필터 설정
        ContactFilter2D filter = new ContactFilter2D();
        
        if (skill.SKILL_AT == ETargetType.ENEMY)
            filter = _controller.EnemyFilter;
        else if (skill.SKILL_AT == ETargetType.ALLY)
            filter = _controller.AllyFilter;

        if (skill.SKILL_ABT_01 == ESkillAbilityType.ATK_MULT)
        {
            Vector2 point = new Vector2(transform.position.x, transform.position.y - skill.SKILL_RANGE_Y);

            count = Physics2D.OverlapBox(point, new Vector2(skill.SKILL_RANGE_X, skill.SKILL_RANGE_Y), 0f, filter, Colliders);

            return;
        }

        if (skill.SKILL_RANGE_TYPE == ERangeType.CIRCLE)
        {
            // 마법사 장판, 궁수 화살비: center 위치 기준 원형 탐지
            count = Physics2D.OverlapCircle(center, skill.SKILL_RANGE_X, filter, Colliders);
        }
        else if (skill.SKILL_RANGE_TYPE == ERangeType.BOX)
        {

            if (target == null) return;
            Vector2 dir = (target.GetTargetObject.transform.position - transform.position).normalized;
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
            Vector2 node = (Vector2)transform.position + bestDir * (skill.SKILL_RANGE_X * 0.5f);
            count = Physics2D.OverlapBox(node,
                new Vector2(skill.SKILL_RANGE_X, skill.SKILL_RANGE_Y), 0f, filter, Colliders);
        }
        else // NONE: 시전자 위치 기준 (몬스터 ALL_TARGET 등)
        {
            count = Physics2D.OverlapCircle(transform.position, skill.SKILL_IS, filter, Colliders);
        }
    }
    
    private void SetTarget(Skill skill)
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
            RangeDetect(skill, null, center);

            for (int i = 0; i < count; i++)
            {
                if (Colliders[i].TryGetComponent(out ITargetable target))
                {
                    target.SetDamage(damage, skill);
                }
            }

            Debug.Log($"[마법사 장판] {elapsed}초 틱 → {count}명 적중");
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }
    }

    public void StartDuration(ITargetable target, Skill skill, float value)
    {
        switch (skill.SKILL_ID)
        {
            case "6049":
                StartCoroutine(DotMaxHpCor(target, skill, value));
                break;
            case "6053":
                StartCoroutine(SpeedDebuffCor(target, skill, value));
                break;
            case "6054":
                StartCoroutine(SetCcCor(target, skill));
                break;
            case "6057":
                StartCoroutine(InvisibleCor(target, skill));
                break;
            case "6503":
                StartCoroutine(ArcherActiveCor(target, skill, value));
                break;
        }
    }

    #region CoroutineSkill

    private IEnumerator DotMaxHpCor(ITargetable target, Skill skill, float value)
    {
        isDurationActive = true;
        
        target.GetTargetObject.TryGetComponent(out BaseController targetObject);
        if (targetObject == null) yield break;

        var stat = targetObject.CurrentStats;
        
        while (_durateTimer <= skill.SKILL_DURATION)
        {
            target.SetDamage((int)value + stat._defense, skill);
            
            yield return new WaitForSeconds(1f);
        }

        isDurationActive = false;
        _durateTimer = 0f;
    }
    private IEnumerator SpeedDebuffCor(ITargetable target, Skill skill, float value)
    {
        Debug.Log("이동속도 공격속도 디버프 스킬 발동");
        target.GetTargetObject.TryGetComponent(out BaseController targetObject);
        if (targetObject == null) yield break;
        
        var stat = targetObject.CurrentStats;
        
        stat._attackSpeed *= (1f - value / 100f);
        stat._moveSpeed *= (1f - value / 100f);

        targetObject.CurrentStats = stat;
        targetObject.Movement.Agent.speed = stat._moveSpeed;
        
        yield return new WaitForSeconds(skill.SKILL_DURATION);
        
        targetObject.CurrentStats = targetObject.BaseStats;
        targetObject.Movement.Agent.speed = targetObject.BaseStats._moveSpeed;
    }
    private IEnumerator SetCcCor(ITargetable target, Skill skill)
    {
        target.GetTargetObject.TryGetComponent(out BaseController targetObject);
        if (targetObject == null) yield break;
        
        targetObject.TryGetComponent(out BaseCharacter player);
        
        player.isCC = true;
        isDurationActive = true;
        targetObject.Movement.Agent.speed = 0f;
        
        yield return new WaitForSeconds(skill.SKILL_DURATION);
        
        player.isCC = false;
        isDurationActive = false;
        targetObject.Movement.Agent.speed = targetObject.BaseStats._moveSpeed;
    }
    private IEnumerator InvisibleCor(ITargetable target, Skill skill)
    {
        target.GetTargetObject.TryGetComponent(out BaseController targetObject);

        SpriteRenderer targetSprite = target.GetTargetObject.GetComponentInChildren<SpriteRenderer>();
        
        var color = targetSprite.color;

        color.a = 0.5f;
        
        targetSprite.color = color;
        targetObject.isInvincible = true;
        
        yield return new WaitForSeconds(skill.SKILL_DURATION);

        color.a = 1f;
        
        targetSprite.color = color;
        targetObject.isInvincible = false;
    }
    private IEnumerator ArcherActiveCor(ITargetable target, Skill skill, float value)
    {
        target.GetTargetObject.TryGetComponent(out BaseController targetObject);
        if (targetObject == null) yield break;
        
        var stat = targetObject.CurrentStats;
        
        stat._attackSpeed *= (1f - value / 100f);
        attackHitCount = 3;

        targetObject.CurrentStats = stat;
        
        yield return new WaitForSeconds(skill.SKILL_DURATION);

        targetObject.CurrentStats = targetObject.BaseStats;
        attackHitCount = 1;
    }
    
    #endregion
    
    public void OnCC(bool value)
    {
        if (value)
        {
            // 플레이어 액티브 스킬 UI 버튼 비활성화
            
        }
    }
}
