using System;
using System.Collections.Generic;

[Serializable]
public class Set_EnumRawData
{
    public string ATK_TYPE;
    public string LEVEL_REWARD_TYPE;
    public string CLEAR_REWARD_TYPE;
    public string SKILL_TYPE;
    public string LANGUAGE_TYPE;
    public string OBJ_TYPE;
}

[Serializable]
public class Set_EnumTable
{
    public List<Set_EnumRawData> data;
}
