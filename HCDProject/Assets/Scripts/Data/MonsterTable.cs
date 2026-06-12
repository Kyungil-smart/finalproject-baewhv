using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class MonsterRawData
{
    public string MONSTER_ID;
    public string MONSTER_NAME;
    public int HP;
    public int ATK;
    public int DEF;
    public float ACCESS_AREA;
    public float ATK_SPEED;
    public float MOVE_SPEED;
    public string ATK_ID;
    public string SKILL_ID;
    public string MONSTER_IMG;
    public float EXP;
    public string MONSTER_HIT_SFX;
    public string MONSTER_HIT_FX;
}

[Serializable]
public class MonsterTable
{
    public List<MonsterRawData> data;
}
