using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = System.Random;

public class DataManager : BaseManager<DataManager>
{
    // 추후 추가될 데이터 파일 ++
    public MapTable MapTable  {get; private set;}
    public MonsterTable MonsterTable {get; private set;}
    public LevelRewardTable LevelRewardTable {get; private set;}
    public CharacterTable CharacterTable {get; private set;}
    public StageClearRewardTable StageClearRewardTable {get; private set;}
    public ObjectTable ObjectTable {get; private set;}
    public PlayerSkillTable PlayerSkillTable {get; private set;}
    public MonsterSkillTable MonsterSkillTable {get; private set;}
    public SkillTable SkillTable {get; private set;}
    public ProjectileTable ProjectileTable {get; private set;}
    public LocalizingTable LocalizingTable {get; private set;}
    public StoryLocalizingTable StoryLocalizingTable {get; private set;}
    public StaticValueTable StaticValueTable {get; private set;}
    public StoryExpTable StoryExpTable {get; private set;}
    public StoryStageTable StoryStageTable {get; private set;}

    public RatioIntValue dataValue;
    
    private Dictionary<string, int> _stageRewardCounts = new Dictionary<string, int>();
    private Dictionary<string, int> _levelRewardCounts = new Dictionary<string, int>();

    private void Awake()
    {
        base.Awake();
        
        dataValue = new RatioIntValue(14, 0);
        
        InitData(() =>
        {
            Debug.Log("초기 데이터 받기 성공");
            ResetStageRewardData();
        });
    }
    
    public void InitData(Action OnDataLoaded)
    {
        (string key, Action<string> assignAction)[] loadList = new (string key, Action<string> assignAction)[]
        {
            ("MAP_TABLE", json => MapTable = JsonUtility.FromJson<MapTable>(json)),
            ("MONSTER_TABLE", json => MonsterTable = JsonUtility.FromJson<MonsterTable>(json)),
            ("LEVEL_REWARD_TABLE", json => LevelRewardTable = JsonUtility.FromJson<LevelRewardTable>(json)),
            ("CHARACTER_TABLE", json => CharacterTable = JsonUtility.FromJson<CharacterTable>(json)),
            ("STAGE_CLEAR_REWARD_TABLE", json => StageClearRewardTable = JsonUtility.FromJson<StageClearRewardTable>(json)),
            ("OBJECT_TABLE", json => ObjectTable = JsonUtility.FromJson<ObjectTable>(json)),
            ("SKILL_TABLE", json => SkillTable = JsonUtility.FromJson<SkillTable>(json)),
            ("PLAYER_SKILL_TABLE", json => PlayerSkillTable = JsonUtility.FromJson<PlayerSkillTable>(json)),
            ("MONSTER_SKILL_TABLE", json => MonsterSkillTable = JsonUtility.FromJson<MonsterSkillTable>(json)),
            ("PROJECTILE_TABLE", json => ProjectileTable = JsonUtility.FromJson<ProjectileTable>(json)),
            ("LOCALIZING_TABLE", json => LocalizingTable = JsonUtility.FromJson<LocalizingTable>(json)),
            ("STORY_LOCALIZING_TABLE", json => StoryLocalizingTable = JsonUtility.FromJson<StoryLocalizingTable>(json)),
            ("STATIC_VALUE_TABLE", json => StaticValueTable = JsonUtility.FromJson<StaticValueTable>(json)),
            ("STORY_EXP_TABLE", json => StoryExpTable = JsonUtility.FromJson<StoryExpTable>(json)),
            ("STORY_STAGE_TABLE", json => StoryStageTable = JsonUtility.FromJson<StoryStageTable>(json))
        };
        
        // 총 데이터 파일의 개수 
        int maxLoadCount = loadList.Length;
        // 로드 완료된 데이터 파일의 개수
        int currentLoadCount = 0;
        
        dataValue.MaxValue = maxLoadCount;

        foreach (var load in loadList)
        {
            string key = load.key;
            Action<string> action = load.assignAction;

            Addressables.LoadAssetAsync<TextAsset>(key).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded) action?.Invoke(handle.Result.text);

                Addressables.Release(handle);
                currentLoadCount++;

                dataValue.Value = currentLoadCount;
                
                if (currentLoadCount >= maxLoadCount) OnDataLoaded?.Invoke();
            };
        }
    }

    #region 스테이지 랜덤 리워드
    
    public void ResetStageRewardData()
    {
        _stageRewardCounts.Clear();
        
        foreach (var reward in StageClearRewardTable.data)
        {
            _stageRewardCounts[reward.CLEAR_REWARD_ID] = 0;
        }
    }

    public List<StageClearRewardRawData> GetStageRandomRewards()
    {
        if (StageClearRewardTable == null || StageClearRewardTable.data == null) return new List<StageClearRewardRawData>();
        
        List<StageClearRewardRawData> rewardPool = StageClearRewardTable.data.Where(reward =>{
            int currentCount = _stageRewardCounts.ContainsKey(reward.CLEAR_REWARD_ID) ? _stageRewardCounts[reward.CLEAR_REWARD_ID] : 0;
            
            return reward.MAX_CLEAR_REWARD_COUNT == 0 || currentCount < reward.MAX_CLEAR_REWARD_COUNT;
        }).ToList();

        
        for (int i = rewardPool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = rewardPool[i];
            rewardPool[i] = rewardPool[j];
            rewardPool[j] = temp;
        }
        
        return rewardPool.GetRange(0, 3);
    }

    public void SelectStageReward(string rewardId)
    {
        if (_stageRewardCounts.ContainsKey(rewardId))
        {
            _stageRewardCounts[rewardId]++;

            var reward = StageClearRewardTable.data.Find(x => x.CLEAR_REWARD_ID == rewardId);
            if (reward != null && reward.MAX_CLEAR_REWARD_COUNT <= _stageRewardCounts[rewardId])
            {
                Debug.Log($"이제 {reward.CLEAR_REWARD_ID} 는 등장 안할거야!");
            }
        }
    }
    
    public int CurrentStageRewardCount(string rewardId)
    {
        if (_stageRewardCounts.ContainsKey(rewardId))
        {
            return _stageRewardCounts[rewardId];
        }
        return 0;
    }
    
    #endregion

    #region 레벨 랜덤 리워드

    public void ResetLevelRewardData()
    {
        _levelRewardCounts.Clear();

        foreach (var reward in LevelRewardTable.data)
        {
            _levelRewardCounts[reward.LEVEL_ID] = 0;
        }
    }

    public List<LevelRewardRawData> GetRandomLevelRewards()
    {
        if (LevelRewardTable == null || LevelRewardTable.data == null) return new List<LevelRewardRawData>();

        List<LevelRewardRawData> rewardPool = new(LevelRewardTable.data);

        for (int i = rewardPool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = rewardPool[i];
            rewardPool[i] = rewardPool[j];
            rewardPool[j] = temp;
        }
        
        return rewardPool.GetRange(0, 3);
    }

    public void SelectLevelReward(string rewardId)
    {
        if (_levelRewardCounts.ContainsKey(rewardId))
        {
            _levelRewardCounts[rewardId]++;
            
            var reward = LevelRewardTable.data.Find(x => x.LEVEL_ID == rewardId);
            if (reward != null)
            {
                // 선택된 id 기준 능력치 적용
            }
        }
    }
    

    #endregion
}
