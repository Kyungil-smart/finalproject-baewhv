using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class MonsterSpawnManager : BaseManager<MonsterSpawnManager>
{
    [SerializeField] private float _spawnOffsetY;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private List<GameObject> _monsterPrefabs = new List<GameObject>();
    
    [field:SerializeField] public int SpawnCount { get; set; }
    
    private bool _isWaving = false;
    public ObserveValue<int> monsterCount = new ObserveValue<int>();
    public ObserveValue<int> currentWave = new ObserveValue<int>();
    
    private List<string> _monsterList = new List<string>();
    private List<int> _spawnCountList = new List<int>();

    protected override void Awake()
    {
        base.Awake();
        
        currentWave.Value = 0;
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            WaveStart();    
        }
    }

    public void WaveStart()
    {
        if (currentWave.Value >= 3) return;
        
        currentWave.Value++;
        
        // 임시로 Wave가 시작될 때 데이터를 불러와 리스트에 설정하도록 작성
        // AddMonsterData(1, 1, currentWave.Value);
        
        _isWaving = true;

        _monsterList.Add("1000");
        _spawnCountList.Add(5);
        _spawnDelay = 1f;

        if (_monsterList.Count > 0 && _spawnCountList.Count > 0)
        {
            StartCoroutine(SpawnMonster(_monsterList, _spawnCountList));   
        }
    }

    public void DespawnMonster(int monsterID, GameObject monster)
    {
        Service.Get<PoolManager>().ReturnPool(_monsterPrefabs[monsterID], monster);
        monsterCount.Value--;
    }

    private IEnumerator SpawnMonster(List<string> monsterList, List<int> spawnCountList)
    {
        for (int i = 0; i < monsterList.Count; i++)
        {
            if (!string.IsNullOrEmpty(monsterList[i]))
            {
                string address = monsterList[i].Trim();
                // GameObject prefab = Service.Get<MonsterManager>().GetMonsterPrefab(address);
                MonsterRawData stat = Service.Get<DataManager>()?.MonsterTable.data.Find(x => x.MONSTER_ID == monsterList[i].Trim());

                for (int j = 0; j < spawnCountList[i]; j++)
                {
                    GameObject obj = Service.Get<PoolManager>().GetPool(_monsterPrefabs[i], RandomPosition(), Quaternion.identity);
                    obj.GetComponent<BaseMonster>().InitStatus(stat);
                    
                    monsterCount.Value++;
                    
                    yield return new WaitForSeconds(_spawnDelay);
                }
            }
        }
    }
    
    public void AddMonsterData(int chapter, int stage, int wave)
    {
        if (Service.Get<GameManager>().isLoading) return; 
        
        MapRawData waveData = Service.Get<DataManager>()?.MapTable.data.Find(x => x.CHAPTER == chapter  && x.STAGE == stage && x.WAVE == wave);
        if (waveData == null) return;

        _monsterList.Add(waveData.SPAWN_MONSTER_ID_01);
        _monsterList.Add(waveData.SPAWN_MONSTER_ID_02);
        _monsterList.Add(waveData.SPAWN_MONSTER_ID_03);
        _monsterList.Add(waveData.SPAWN_MONSTER_ID_04);
        _monsterList.Add(waveData.SPAWN_MONSTER_ID_05);
        _monsterList.Add(waveData.SPAWN_MONSTER_ID_06);
        _monsterList.Add(waveData.SPAWN_MONSTER_ID_07);
        _spawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_01);
        _spawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_02);
        _spawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_03);
        _spawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_04);
        _spawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_05);
        _spawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_06);
        _spawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_07);
        
        _spawnDelay = waveData.WAVE_RESPAWN_TIME;
    }

    private Vector3 RandomPosition()
    {
        Vector3 pos = Vector3.zero;

        float randomX = UnityEngine.Random.Range(-2f, 2f);

        pos.y = _spawnOffsetY;
        pos.x = randomX;
        
        return pos;
    }
}
