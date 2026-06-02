using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class Level_RewardRawData
{
    public string LEVEL_ID;
    public string LEVEL_REWARD_NAME;
    public string LEVEL_REWARD_ICON;
    public string LEVEL_REWARD_TYPE_01;
    public float LEVEL_REWARD_01;
    public string LEVEL_REWARD_TYPE_02;
    public float LEVEL_REWARD_02;
}

[Serializable]
public class Level_RewardTable
{
    public List<Level_RewardRawData> data;
}
