using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class Player_Active_SkillRawData
{
    public string SKILL_ID;
    public string SKILL_NAME;
    public string ATK_TYPE;
    public string SKILL_TYPE;
    public float SKILL_IS;
    public float SKILL_RANGE;
    public float SKILL_TIME;
    public string SKILL_AT;
    public string SKILL_DT;
    public float SKILL_ABILLITY;
    public string SKILL_SFX;
    public string SKILL_FX;
    public string SKILL_HIT_SFX;
    public string SKILL_HIT_FX;
    public string SKILL_ICON;
}

[Serializable]
public class Player_Active_SkillTable
{
    public List<Player_Active_SkillRawData> data;
}
