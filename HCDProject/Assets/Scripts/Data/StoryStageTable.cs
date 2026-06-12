using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class StoryStageRawData
{
    public string STAGE_ID;
    public string NEXT_STAGE_ID;
    public int CHAPTER;
    public int STAGE;
    public string STAGE_TYPE;
    public string STAGE_NAME_ID;
    public string TYPE_OF_ID;
    public string STORY_ID;
}

[Serializable]
public class StoryStageTable
{
    public List<StoryStageRawData> data;
}
