using System;
using System.Collections.Generic;

[Serializable]
public class MapRawData
{
    public string MAP_ID;
    public int CHAPTER;
    public int STAGE;
    public int _3_Sort_Count;
    public int TOTAL_WAVE;
    public int WAVE;
    public int NEXT_WAVE_TIME;
    public int TOTAL_WAVE_MONSTER;
    public int WAVE_RESPAWN_TIME;
    public string SPAWN_MONSTER_ID_01;
    public int SPAWN_MONSTER_COUNT_01;
    public string SPAWN_MONSTER_ID_02;
    public int SPAWN_MONSTER_COUNT;
    public string ENTRY;
}

[Serializable]
public class MapTable
{
    public List<MapRawData> data;
}
