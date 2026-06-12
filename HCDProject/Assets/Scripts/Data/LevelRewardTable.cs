using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class LevelRewardRawData
{
    public string LEVEL_ID;
    public string LEVEL_REWARD_NAME;
    public string LEVEL_REWARD_TEXT_ID;
    public string LEVEL_REWARD_TYPE_01;
    public float LEVEL_REWARD_01;
    public string LEVEL_REWARD_TYPE_02;
    public float LEVEL_REWARD_02;
    public string LEVEL_REWARD_ICON;
}

[Serializable]
public class LevelRewardTable
{
    public List<LevelRewardRawData> data;
}
