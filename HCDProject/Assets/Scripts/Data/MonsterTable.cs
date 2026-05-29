using System;
using System.Collections.Generic;

[Serializable]
public class MonsterRawData
{
    public string MONSTER_ID;
    public string MONSTER_NAME;
    public int HP;
    public int ATK;
    public int DEF;
    public float ATK_SPEED;
    public float MOVE_SPEED;
    public string ATK_TYPE;
    public float ATK_IS;
    public string ATK_SFX;
    public string HIT_SFX;
    public string ATK_FX;
    public string HIT_FX;
    public string ATTACK_MOTION;
    public string DEAD_MOTION;
    public string MOVE_MOTION;
    public string HIT_MOTION;
    public string MONSTER_IMG;
    public float EXP;
}

[Serializable]
public class MonsterTable
{
    public List<MonsterRawData> data;
}
