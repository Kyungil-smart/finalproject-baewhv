using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int chapter;
    public int stage;
    
    public List<SaveRewardData> saveRewardDatas = new List<SaveRewardData>();
}

[System.Serializable]
public class SaveRewardData
{
    public string rewardName;
    public int count;
}
