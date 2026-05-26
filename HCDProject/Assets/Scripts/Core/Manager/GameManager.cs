using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] public MonsterManager monsterManager;
    
    private int _wallHealth;
    private bool isLoading = false;

    public int WallHealth
    {
        get => _wallHealth;
        private set
        {
            _wallHealth = value;

            if (_wallHealth <= 0)
            {
                _wallHealth = 0;
                EndStage();
            }
        }
    }

    private void Awake()
    {
        Service.Register<GameManager>(this);
        Init();
    }

    private void Start()
    {
        Service.Get<DataManager>()?.InitData(()=>{Debug.Log("초기 데이터 받기 성공");});
        
        Service.Get<GameManager>()?.Spawn(1,1,1);
    }
    
    private void OnDestroy() => Service.UnRegister<GameManager>();

    private void Update()
    {
        if (Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            EnterStage(1, 1);
        }

        if (Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            EnterStage(1, 2);
        }

        if (Keyboard.current.numpad4Key.wasPressedThisFrame)
        {
            Spawn(1, 2, 1);
        }
    }

    private void Init()
    {
        WallHealth = 100; // 추후 json으로 데이터 연결 필요 
    }

    private void EnterStage(int chapter, int stage)
    {
        isLoading = true;
        
        List<MapRawData> currentStage = Service.Get<DataManager>()?.MapTable.data.FindAll(x => x.CHAPTER == chapter && x.STAGE == stage);
        
        HashSet<string> ids = new HashSet<string>();

        foreach (var data in currentStage)
        {
            if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_01)) ids.Add(data.SPAWN_MONSTER_ID_01.Trim());
            if (!string.IsNullOrEmpty(data.SPAWN_MONSTER_ID_02)) ids.Add(data.SPAWN_MONSTER_ID_02.Trim());
        }
        
        monsterManager.StageMonster(new List<string>(ids), () =>
        {
            isLoading = false;

            foreach (var id in ids)
            {
                Debug.Log($"로딩 성공 : {id}");
            }
        });
    }

    public void Spawn(int chapter, int stage, int wave)
    {
        if (isLoading) return;
        
        MapRawData waveData = Service.Get<DataManager>()?.MapTable.data.Find(x => x.CHAPTER == chapter  && x.STAGE == stage && x.WAVE == wave);
        if (waveData == null) return;

        if (!string.IsNullOrEmpty(waveData.SPAWN_MONSTER_ID_01))
        {
            string address = waveData.SPAWN_MONSTER_ID_01.Trim();
            GameObject prefab = Service.Get<MonsterManager>().GetMonsterPrefab(address);

            for (int i = 0; i < waveData.SPAWN_MONSTER_COUNT_01; i++)
            {
                GameObject obj = Instantiate(prefab, UnityEngine.Random.insideUnitSphere * 3f, Quaternion.identity);
                
                MonsterRawData stat = Service.Get<DataManager>()?.MonsterTable.data.Find(x => x.MONSTER_ID == waveData.SPAWN_MONSTER_ID_01.Trim());
                obj.AddComponent<NormalMonster>().InitStatus(stat);
            }
        }

        if (!string.IsNullOrEmpty(waveData.SPAWN_MONSTER_ID_02))
        {
            string address = waveData.SPAWN_MONSTER_ID_02.Trim();
            GameObject prefab = Service.Get<MonsterManager>().GetMonsterPrefab(address);

            for (int i = 0; i < waveData.SPAWN_MONSTER_COUNT; i++)
            {
                GameObject obj = Instantiate(prefab, UnityEngine.Random.insideUnitSphere * 3f, Quaternion.identity);
                
                MonsterRawData stat = Service.Get<DataManager>()?.MonsterTable.data.Find(x => x.MONSTER_ID == waveData.SPAWN_MONSTER_ID_02.Trim());
                obj.AddComponent<NormalMonster>().InitStatus(stat);
            }
        }
    }

    public void EndStage()
    {
        if (_wallHealth <= 0)
        { 
            Debug.Log("성벽 삭제");
            // 스테이지 종료(실패) ui 출력 요청
            return;
        }
        
        Debug.Log("스테이지 클리어");
        // 스테이지 종료(성공) ui 출력 요청
    }

    // 외부에서 데미지 넣을 때 호출 할 메서드
    public void TakeDamage(int damage)
    {
        WallHealth -= damage;
    }
    
    // 외부에서 몹이 모두 죽으면 호출 할 메서드
    public void StageClear()
    { 
        // if (MonsterManager.Instance.남은 몬스터 수 == 0)
        {
            EndStage();
        }
    }
}
