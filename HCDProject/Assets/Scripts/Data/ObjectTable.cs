using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class ObjectRawData
{
    public string OBJECT_ID;
    public string OBJ_NAME;
    public string OBJ_TYPE;
    public float OBJ_ABILITY;
    public float OBJ_WEIGHT;
    public string OBJ_ICON;
}

[Serializable]
public class ObjectTable
{
    public List<ObjectRawData> data;
}
