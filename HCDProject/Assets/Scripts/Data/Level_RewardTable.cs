using System;
using System.Collections.Generic;

[Serializable]
public class Level_RewardRawData
{
    public string LEVEL_ID;
    public string LEVEL_REWARD_NAME;
    public float LEVEL_REWARD;
    public string LEVEL_REWARD_ICON;
    public string LEVEL_REWARD_TYPE;
}

[Serializable]
public class Level_RewardTable
{
    public List<Level_RewardRawData> data;
}
