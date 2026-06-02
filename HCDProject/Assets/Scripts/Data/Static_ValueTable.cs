using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class Static_ValueRawData
{
    public string VARIABLE_NAME;
    public float VARIABLE_TYPE;
    public string VARIABLE_VALUE;
}

[Serializable]
public class Static_ValueTable
{
    public List<Static_ValueRawData> data;
}
