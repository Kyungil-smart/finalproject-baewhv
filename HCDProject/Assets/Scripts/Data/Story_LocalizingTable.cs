using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class Story_LocalizingRawData
{
    public string STORY_ID;
    public string NEXT_ID;
    public int CHAPTER;
    public int STAGE;
    public string NUMBER;
    public string CATEGORY;
    public string BACKGROUND;
    public string BGM;
    public string SFX;
    public string PORTRAIT;
    public string NAME;
    public string KOR;
    public string ENG;
    public string THA;
    public string VN;
    public string IDN;
}

[Serializable]
public class Story_LocalizingTable
{
    public List<Story_LocalizingRawData> data;
}
