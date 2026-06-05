using System;
using UnityEngine;

[System.Serializable]
public class Skill
{
    public float SKILL_ABILLITY;

    public string SKILL_ID;
    public string SKILL_NAME;
    public float SKILL_IS;
    public float SKILL_RANGE_X;
    public float SKILL_RANGE_Y;
    public float SKILL_TIME;

    public int targetCount;

    public ETargetType SKILL_AT;

    public EAtkType ATK_TYPE;
    public ESkillType SKILL_TYPE;
    public string SKILL_DT; // 스킬 데미지 타입
    public string SKILL_SFX;
    public string SKILL_FX;
    public string SKILL_HIT_SFX;
    public string SKILL_HIT_FX;
    public string SKILL_ICON;
    
    // Monster Skill Data
    public ESkillAbilityType SKILL_ABT_01;
    public ESkillAbilityType SKILL_ABT_02;
    public float SKILL_AB_01;
    public float SKILL_AB_02;
    public float SKILL_DURATION;

    public Skill(PlayerActiveSkillRawData data)
    {
        SKILL_ABILLITY = data.SKILL_ABILLITY;
        SKILL_IS = data.SKILL_IS;
        SKILL_RANGE_X = data.SKILL_RANGE_X;
        SKILL_RANGE_Y = data.SKILL_RANGE_Y;
        SKILL_TIME = data.SKILL_TIME;
        SKILL_AT = Enum.Parse<ETargetType>(data.SKILL_AT);
        ATK_TYPE = Enum.Parse<EAtkType>(data.ATK_TYPE);
        SKILL_TYPE = Enum.Parse<ESkillType>(data.SKILL_TYPE);
        targetCount = 1;
    }

    public Skill(MonsterSkillRawData data)
    {
        SKILL_ID = data.SKILL_ID;
        SKILL_NAME = data.SKILL_NAME;
        SKILL_IS = data.SKILL_IS;
        SKILL_RANGE_X = data.SKILL_RANGE_X;
        SKILL_RANGE_Y = data.SKILL_RANGE_Y;
        SKILL_TIME = data.SKILL_TIME;
        SKILL_AT = Enum.Parse<ETargetType>(data.SKILL_AT);
        ATK_TYPE = Enum.Parse<EAtkType>(data.ATK_TYPE);
        SKILL_TYPE = Enum.Parse<ESkillType>(data.SKILL_TYPE);
        SKILL_DURATION = data.SKILL_DURATION;
        SKILL_ABT_01 = Enum.Parse<ESkillAbilityType>(data.SKLL_ABT_01);
        SKILL_AB_01 = data.SKILL_AB_01;
        SKILL_ABT_02 = Enum.Parse<ESkillAbilityType>(data.SKLL_ABT_02);
        SKILL_AB_02 = data.SKILL_AB_02;
    }
}

public enum EAtkType
{
    NORMAL = 0,
    BUFF = 1,
    HEAL = 2,
    SKILL = 3 // 이건 확인해야함
}

public enum ESkillType
{
    SINGLE_TARGET = 0,
    ALL_TARGET = 1,
    ATTACK_OF_SCOPE = 2
}

public enum ESkillSlot
{
    Normal = 0,
    Skill = 1,
    Skill2 = 2,
    Skill3 = 3
}

public enum ETargetType
{
    ENEMY = 0,
    ALLY = 1,
    SELF = 2
}
public enum EFindType
{
    Nearest = 0,  // 가장 가까운 대상
    Farthest = 1, // 가장 먼 대상
    LowestHp = 2  // 체력이 가장 낮은 대상 (힐러용)
}

public enum ESkillAbilityType
{
    NONE = 0,
    HP = 1,
    ATK = 2
}