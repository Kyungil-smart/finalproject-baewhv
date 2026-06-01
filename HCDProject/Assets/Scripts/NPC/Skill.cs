using UnityEngine;

[System.Serializable]
public class Skill
{
    public int skillDamage;
    
    public int skillRange;

    public int coolTime;

    public int targetCount;

    public ETargetType TargetType;
}

public enum ESkillType
{
    Normal = 0,
    Skill = 1,
    Skill2 = 2,
    Skill3 = 3
}

public enum ETargetType
{
    Enemy = 0,
    Friendly = 1
}
public enum EFindType
{
    Nearest = 0,  // 가장 가까운 대상
    Farthest = 1, // 가장 먼 대상
    LowestHp = 2  // 체력이 가장 낮은 대상 (힐러용)
}