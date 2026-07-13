using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class TutorialRawData
{
    public string TUTORIAL_ID;
    public string NEXT_ID;
    public string OCCUR;
    public int OCCUR_VALUE;
    public string NEXT_TYPE;
    public string NEXT_TYPE_VALUE;
    public string CATEGORY;
    public string CATEGORY_VALUE;
    public string HIGHLIGHT;
    public string MESSAGE_TYPE;
    public string NAME_ID;
    public string TEXT_ID;
    public string SFX;
    public string BGM;
}

[Serializable]
public class TutorialTable
{
    public List<TutorialRawData> data;
}
