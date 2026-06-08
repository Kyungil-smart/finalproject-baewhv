using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class MonsterSkillRawData
{
    public string SKILL_ID;
    public string SKILL_NAME;
    public string ATK_TYPE;
    public string SKILL_TYPE;
    public string SKILL_AT;
    public float SKILL_IS;
    public float SKILL_RANGE_X;
    public float SKILL_RANGE_Y;
    public float SKILL_TIME;
    public float SKILL_DURATION;
    public string SKILL_ABT_01;
    public float SKILL_AB_01;
    public string SKILL_ABT_02;
    public float SKILL_AB_02;
    public string SKILL_SFX;
    public string SKILL_FX;
    public string SKILL_HIT_SFX;
    public string SKILL_HIT_FX;
    public string SKILL_ICON;
    public string PROJECTILE_ID;
}

[Serializable]
public class MonsterSkillTable
{
    public List<MonsterSkillRawData> data;
}
