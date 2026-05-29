using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnManager : BaseManager<MonsterSpawnManager>
{
    [SerializeField] private float _spawnOffsetY;
    [SerializeField] private float _spawnDelay;
    [SerializeField] private List<GameObject> _monsterPrefabs = new List<GameObject>();

    public int MonsterID { get; set; }
    [field:SerializeField] public int SpawnCount { get; set; }
    
    private bool _isWaving = false;
    public ObserveValue<int> monsterCount = new ObserveValue<int>();
    public ObserveValue<int> currentWave = new ObserveValue<int>();
    public ObserveValue<bool> stageClear = new ObserveValue<bool>();

    protected override void Awake()
    {
        base.Awake();
        
        currentWave.Value = 0;

        // MonsterID, MonsterCount는 후에 데이터 테이블로 받을 예정
        // 현재는 코드및 직렬화로 입력
        MonsterID = 1;
    }

    private void OnEnable()
    {
        monsterCount.AddListener(WaveEnd);
    }

    private void OnDisable()
    {
        monsterCount.RemoveListener(WaveEnd);
    }

    private void Start()
    {
        WaveStart();
    }

    public void WaveStart()
    {
        if (currentWave.Value >= 3) return;
        
        currentWave.Value++;
        
        _isWaving = true;
        
        SpawnMonster(MonsterID, SpawnCount);
    }

    private void WaveEnd(int value)
    {
        if (value > 0) return;
        
        _isWaving = false;

        if (currentWave.Value >= 3)
        {
            stageClear.Value = true;
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
            
            monsterCount.Value++;
            
            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    public void DespawnMonster(int monsterID, GameObject monster)
    {
        Service.Get<PoolManager>().ReturnPool(_monsterPrefabs[monsterID - 1], monster);
        monsterCount.Value--;
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
