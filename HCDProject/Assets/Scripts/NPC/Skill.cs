using UnityEngine;

[System.Serializable]
public class Skill
{
    public int skillDamage;
    
    public int skillRange;

    public int coolTime;
}

public enum ESkillType
{
    Normal = 0,
    Skill = 1,
    Skill2 = 2,
    Skill3 = 3
}