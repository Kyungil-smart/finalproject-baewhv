using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class LocalizingRawData
{
    public string TEXT_ID;
    public string Korean;
    public string English;
    public string Thai;
    public string Vietnamese;
    public string Indonesian;
}

[Serializable]
public class LocalizingTable
{
    public List<LocalizingRawData> data;
}
