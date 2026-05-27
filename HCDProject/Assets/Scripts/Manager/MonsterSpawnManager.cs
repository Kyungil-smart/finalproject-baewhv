using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnManager : SingletonMonoBehaviour<MonsterSpawnManager>
{
    [SerializeField] private float _spawnOffsetY;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private List<GameObject> _monsterPrefabs = new List<GameObject>();
    
    public int MonsterCount { get; set; }
    
    private void Start()
    {
        SpawnMonster(1, 5);
    }
    
    public void SpawnMonster(int monsterID, int count)
    {
        StartCoroutine(SpawnMonsterCoroutine(monsterID, count));
    }

    private IEnumerator SpawnMonsterCoroutine(int monsterID, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject monster = PoolManager.Instance.GetPool(_monsterPrefabs[monsterID - 1]
                , RandomPosition()
                , Quaternion.identity);
            
            MonsterCount++;
            
            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    public void DespawnMonster(int monsterID, GameObject monster)
    {
        PoolManager.Instance.ReturnPool(_monsterPrefabs[monsterID], monster);
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
