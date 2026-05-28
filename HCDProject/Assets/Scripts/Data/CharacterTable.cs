using System;
using System.Collections.Generic;

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
    public string ATK_TYPE;
    public float ATK_IS;
    public float CRI_RATE;
    public float CRI_DAMAGE;
    public string ATK_SFX;
    public string HIT_SFX;
    public string ATK_FX;
    public string HIT_FX;
    public string ATK_MOTION;
    public string DEAD_MOTION;
    public string MOVE_MOTION;
    public string HIT_MOTION;
    public string CHARACTER_IMG;
    public string SKILL;
}

[Serializable]
public class CharacterTable
{
    public List<CharacterRawData> data;
}
