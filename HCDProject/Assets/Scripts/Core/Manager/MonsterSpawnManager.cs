using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MonsterSpawnManager : BaseManager<MonsterSpawnManager>
{
    private Dictionary<string, GameObject> _stageMonsterPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, AsyncOperationHandle<GameObject>> _monsterHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    
    [SerializeField] private float _spawnOffsetY;
    private float _spawnDelay;
    private int _totalSpawnCycle;
    [SerializeField] private List<GameObject> prefabs = new List<GameObject>();
    
    // 생성 되어야할 총 몬스터 수
    public int SpawnCount { get; set; }
    
    // 남은 몬스터 수
    public ObserveValue<int> monsterCount = new ObserveValue<int>();
    public ObserveValue<int> currentWave = new ObserveValue<int>();
    
    [SerializeField] private List<string> _waveMonsterList = new List<string>();
    [SerializeField] private List<int> _waveSpawnCountList = new List<int>();

    protected override void Awake()
    {
        base.Awake();
        
        currentWave.Value = 0;
    }

    private void Start()
    {
        StageMonster(new List<string>(Service.Get<GameManager>().ids), () =>
        {
            Service.Get<GameManager>().isLoading = false;
        });
    }

    public void WaveStart()
    {
        if (currentWave.Value >= 3) return;
        
        currentWave.Value++;
        
        AddMonsterData(Service.Get<GameManager>().CurrentChapter, Service.Get<GameManager>().CurrentStage, currentWave.Value);
        
        if (_waveMonsterList.Count > 0 && _waveSpawnCountList.Count > 0)
        {
            SpawnCountCheck();
            StartCoroutine(SpawnMonster(_waveMonsterList, _waveSpawnCountList));   
        }
    }

    public void DespawnMonster(int prefabIndex, GameObject monster)
    {
        Service.Get<PoolManager>().ReturnPool(prefabs[prefabIndex], monster);
        monsterCount.Value--;
    }

    private IEnumerator SpawnMonster(List<string> monsterList, List<int> spawnCountList)
    {
        prefabs.Clear();

        for (int k = 0; k < _totalSpawnCycle; k++)
        {
            for (int i = 0; i < monsterList.Count; i++)
            {
                if (!string.IsNullOrEmpty(monsterList[i]))
                {
                    string address = monsterList[i].Trim();
                    prefabs.Add(GetMonsterPrefab(address));
                    MonsterRawData stat = Service.Get<DataManager>()?.MonsterTable.data.Find(x => x.MONSTER_ID == monsterList[i].Trim());

                    for (int j = 0; j < spawnCountList[i]; j++)
                    {
                        if (int.Parse(address) < 1035)
                        {
                            GameObject obj = Service.Get<PoolManager>().GetPool(prefabs[i], RandomPosition(), Quaternion.identity);
                            if (obj.TryGetComponent(out BaseMonster monster))
                            {
                                monster.PrefabIndex = i;
                                monster.InitStatus(stat);
                            }
                        }
                        else
                        {
                            if (k == _totalSpawnCycle - 1)
                            {
                                GameObject obj = Service.Get<PoolManager>().GetPool(prefabs[i], RandomPosition(), Quaternion.identity);
                                if (obj.TryGetComponent(out BaseMonster monster))
                                {
                                    monster.PrefabIndex = i;
                                    monster.InitStatus(stat);
                                }
                            }
                        }

                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        
            yield return new WaitForSeconds(_spawnDelay);
        }
        
        
    }
    
    private void AddMonsterData(int chapter, int stage, int wave)
    {
        if (Service.Get<GameManager>().isLoading) return;
        
        _waveMonsterList.Clear();
        _waveSpawnCountList.Clear();
        
        MapRawData waveData = Service.Get<DataManager>()?.MapTable.data.Find(x => x.CHAPTER == chapter  && x.STAGE == stage && x.WAVE == wave);
        
        if (waveData == null)
        {
            GetDummyMonster();
            return;
        }

        _waveMonsterList.Add(waveData.SPAWN_MONSTER_ID_01);
        _waveMonsterList.Add(waveData.SPAWN_MONSTER_ID_02);
        _waveMonsterList.Add(waveData.SPAWN_MONSTER_ID_03);
        _waveMonsterList.Add(waveData.SPAWN_MONSTER_ID_04);
        _waveMonsterList.Add(waveData.SPAWN_MONSTER_ID_05);
        _waveMonsterList.Add(waveData.SPAWN_MONSTER_ID_06);
        _waveMonsterList.Add(waveData.SPAWN_MONSTER_ID_07);
        _waveSpawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_01);
        _waveSpawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_02);
        _waveSpawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_03);
        _waveSpawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_04);
        _waveSpawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_05);
        _waveSpawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_06);
        _waveSpawnCountList.Add(waveData.SPAWN_MONSTER_COUNT_07);
        
        _spawnDelay = waveData.WAVE_RESPAWN_TIME;
        _totalSpawnCycle = waveData.TOTAL_WAVE_MONSTER;
    }
    
    public void StageMonster(List<string> currentStageMonsterIds, Action onComplete)
    {
        List<string> release = new List<string>();
        
        foreach (var id in _stageMonsterPrefabs.Keys)
        {
            if (!currentStageMonsterIds.Contains(id)) release.Add(id);
        }

        foreach (var id in release)
        {
            if (_monsterHandles.TryGetValue(id, out var handle))
            {
                Addressables.Release(handle);
                _monsterHandles.Remove(id);
                _stageMonsterPrefabs.Remove(id);
                Debug.Log($"release 성공 : {id}");
            }
        }

        List<string> loadTarget = new List<string>();
        
        foreach (var id in currentStageMonsterIds)
        {
            if (!_stageMonsterPrefabs.ContainsKey(id)) loadTarget.Add(id);
        }
        
        Debug.Log(loadTarget.Count);

        if (loadTarget.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }
        
        int maxLoadCount = loadTarget.Count;
        int currentLoadCount = 0;
        
        foreach (var loadId in loadTarget)
        {
            Addressables.LoadAssetAsync<GameObject>(loadId).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _stageMonsterPrefabs[loadId] = handle.Result;
                    _monsterHandles[loadId] = handle;
                    Debug.Log($"load 성공 : {loadId}");
                }
                else Debug.Log($"load 실패 :  {loadId}");

                currentLoadCount++;
                if (currentLoadCount >= maxLoadCount) onComplete?.Invoke();
            };
        }
    }

    public GameObject GetMonsterPrefab(string monsterAddress)
    {
        if (_stageMonsterPrefabs.TryGetValue(monsterAddress, out GameObject prefab)) return prefab;
        
        return null;
    }

    private void SpawnCountCheck()
    {
        SpawnCount = 0;

        int boss = 0;

        for (int i = 0; i < _waveSpawnCountList.Count; i++)
        {
            if (int.TryParse(_waveMonsterList[i].Trim(), out int num))
            {
                if (num < 1035)
                {
                    SpawnCount += _waveSpawnCountList[i]; 
                }
                else
                {
                    boss += _waveSpawnCountList[i];
                }
            }
        }

        SpawnCount = (SpawnCount * _totalSpawnCycle) + boss;

        monsterCount.Value = SpawnCount;
    }

    private Vector3 RandomPosition()
    {
        Vector3 pos = Vector3.zero;

        float randomX = UnityEngine.Random.Range(-2f, 2f);
        float randomY = UnityEngine.Random.Range(_spawnOffsetY - 2f, _spawnOffsetY + 2f);

        pos.y = randomY;
        pos.x = randomX;
        
        return pos;
    }
    
    public void DebugWaveStart(int chapter, int stage, int wave)
    {
        if (wave >= 3) return;
        currentWave.Value = wave;
        
        AddMonsterData(chapter, stage, wave);
        
        List<string> dummyList = new List<string>();
        foreach (var id in _waveMonsterList)
        {
            if (!string.IsNullOrEmpty(id))
            {
                dummyList.Add(id);
            }
        }
        
        if (dummyList.Count == 0) return;
        
        StageMonster(dummyList, () =>
        {
            SpawnCountCheck();
            StartCoroutine(SpawnMonster(_waveMonsterList, _waveSpawnCountList));
        });
    }
    
    private void GetDummyMonster()
    {
        _waveMonsterList.Add("1000");
        _waveMonsterList.Add("1001");
        _waveMonsterList.Add("1002");
        _waveMonsterList.Add("1003");
        _waveMonsterList.Add("1004");
        _waveMonsterList.Add("1035");
        
        _waveSpawnCountList.Add(13);
        _waveSpawnCountList.Add(2);
        _waveSpawnCountList.Add(5);
        _waveSpawnCountList.Add(2);
        _waveSpawnCountList.Add(2);
        _waveSpawnCountList.Add(1);

        _spawnDelay = 10f;
        _totalSpawnCycle = 3;
    }
}
