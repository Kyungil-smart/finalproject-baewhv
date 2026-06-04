using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class CharacterRawData
{
    public string CHARACTER_ID;
    public string CHARACTER_NAME;
    public int HP;
    public int ATK;
    public int DEF;
    public float ATK_SPEED;
    public float MOVE_SPEED;
    public float CRI_RATE;
    public float CRI_DMAGE;
    public float DOUBLE_ATK_RATE;
    public string ATK_ID;
    public string SKILL_ID;
    public string CHARACTER_HIT_SFX;
    public string CHARACTER_HIT_FX;
    public string ATK_FX;
    public string CHARACTER_IMG;
}

[Serializable]
public class CharacterTable
{
    public List<CharacterRawData> data;
}
