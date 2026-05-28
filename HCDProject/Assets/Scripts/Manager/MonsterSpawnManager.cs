using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnManager : BaseManager<MonsterSpawnManager>
{
    [SerializeField] private float _spawnOffsetY;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private List<GameObject> _monsterPrefabs = new List<GameObject>();

    public int MonsterID { get; set; }
    public int SpawnCount { get; set; }
    
    private int _currentWave;
    private bool _isWaving = false;
    private bool _stageClear = false;
    
    public int MonsterCount { get; set; }

    private void OnEnable()
    {
        _currentWave = 0;
    }

    public void WaveStart()
    {
        _currentWave++;
        
        _isWaving = true;
        
        SpawnMonster(MonsterID, SpawnCount);
    }

    public void WaveEnd()
    {
        _isWaving = false;

        if (_currentWave >= 3)
        {
            _stageClear = true;
        }
    }
    
    public void SpawnMonster(int monsterID, int count)
    {
        StartCoroutine(SpawnMonsterCoroutine(monsterID, count));
    }

    private IEnumerator SpawnMonsterCoroutine(int monsterID, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject monster = Service.Get<PoolManager>().GetPool(_monsterPrefabs[monsterID - 1]
                , RandomPosition()
                , Quaternion.identity);
            
            MonsterCount++;
            
            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    public void DespawnMonster(int monsterID, GameObject monster)
    {
        Service.Get<PoolManager>().ReturnPool(_monsterPrefabs[monsterID], monster);
        MonsterCount--;
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
