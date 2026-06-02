using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class ProjectileRawData
{
    public string PROJECTILE_ID;
    public string SKIL_ID;
    public string PREFABS_NAME;
    public string IMG;
    public float SKILL_MOVESPEED;
}

[Serializable]
public class ProjectileTable
{
    public List<ProjectileRawData> data;
}
