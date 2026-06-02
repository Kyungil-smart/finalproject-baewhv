using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class Stage_Clear_RewardRawData
{
    public string CLEAR_REWARD_ID;
    public string CLEAR_REWARD_NAME;
    public int MAX_CLEAR_REWARD_COUNT;
    public string CLEAR_REWARD_ICON;
    public string CLEAR_REWARD_TYPE_01;
    public float CLEAR_REWARD_F_01;
    public float CLEAR_REWARD_S_01;
    public string CLEAR_REWARD_TYPE_02;
    public float CLEAR_REWARD_F_02;
    public float CLEAR_REWARD_S_02;
    public string CLEAR_REWARD_TYPE_03;
    public float CLEAR_REWARD_F_03;
    public float CLEAR_REWARD_S_03;
}

[Serializable]
public class Stage_Clear_RewardTable
{
    public List<Stage_Clear_RewardRawData> data;
}
