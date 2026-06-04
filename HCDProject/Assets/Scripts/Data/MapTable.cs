using System;
using System.Collections.Generic;

// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 
[Serializable]
public class MapRawData
{
    public string MAP_ID;
    public int CHAPTER;
    public int STAGE;
    public int SORT_COUNT;
    public int TOTAL_WAVE;
    public int WAVE;
    public int TOTAL_WAVE_MONSTER;
    public int WAVE_RESPAWN_TIME;
    public string BGM;
    public string BG_IMG;
    public string SPAWN_MONSTER_ID_01;
    public int SPAWN_MONSTER_COUNT_01;
    public string SPAWN_MONSTER_ID_02;
    public int SPAWN_MONSTER_COUNT_02;
    public string SPAWN_MONSTER_ID_03;
    public int SPAWN_MONSTER_COUNT_03;
    public string SPAWN_MONSTER_ID_04;
    public int SPAWN_MONSTER_COUNT_04;
    public string SPAWN_MONSTER_ID_05;
    public int SPAWN_MONSTER_COUNT_05;
    public string SPAWN_MONSTER_ID_06;
    public int SPAWN_MONSTER_COUNT_06;
    public string SPAWN_MONSTER_ID_07;
    public int SPAWN_MONSTER_COUNT_07;
    public string ENTRY;
}

[Serializable]
public class MapTable
{
    public List<MapRawData> data;
}
