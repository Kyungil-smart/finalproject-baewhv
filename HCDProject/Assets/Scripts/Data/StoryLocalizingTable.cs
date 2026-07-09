using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class StoryLocalizingRawData
{
    public string STORY_ID;
    public string NEXT_ID;
    public int CHAPTER;
    public int STAGE;
    public string STAGE_DIALOGUE_EVENT_TYPE;
    public string CATEGORY;
    public string BACKGROUND;
    public string BGM;
    public string SFX;
    public string PORTRAIT_H;
    public string PORTRAIT_L;
    public string PORTRAIT_R;
    public string NAME;
    public string TEXT_ID;
}

[Serializable]
public class StoryLocalizingTable
{
    public List<StoryLocalizingRawData> data;
}
