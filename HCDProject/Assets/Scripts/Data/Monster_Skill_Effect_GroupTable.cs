using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class Monster_Skill_Effect_GroupRawData
{
    public string MSEG_ID;
    public string SKILL_ID;
    public string SKILL_AT;
    public string SKLL_ABT_01;
    public float SKILL_AB_01;
    public string SKLL_ABT_02;
    public float SKILL_AB_02;
}

[Serializable]
public class Monster_Skill_Effect_GroupTable
{
    public List<Monster_Skill_Effect_GroupRawData> data;
}
