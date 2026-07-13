using System;
using System.Collections.Generic;
using System.IO;
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
    public SkillTable SkillTable {get; private set;}
    public StoryLocalizingTable StoryLocalizingTable {get; private set;}
    public StaticValueTable StaticValueTable {get; private set;}
    public StoryExpTable StoryExpTable {get; private set;}
    public StoryStageTable StoryStageTable {get; private set;}
    public TutorialTable TutorialTable {get; private set;}

    public RatioIntValue dataValue;
    
    private Dictionary<string, int> _stageRewardCounts = new Dictionary<string, int>();
    private Dictionary<string, int> _levelRewardCounts = new Dictionary<string, int>();
    
    private const string BestChapter = "BestChapter";
    private const string BestStage = "BestStage";
    
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "saveData.json");
    public SaveData LoadSaveData { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        if (IsManagerDestroy) return;
        
        dataValue = new RatioIntValue(12, 0);
        
        InitData(() =>
        {
            Debug.Log("초기 데이터 받기 성공");

            LoadGameData();
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
            ("STORY_LOCALIZING_TABLE", json => StoryLocalizingTable = JsonUtility.FromJson<StoryLocalizingTable>(json)),
            ("STATIC_VALUE_TABLE", json => StaticValueTable = JsonUtility.FromJson<StaticValueTable>(json)),
            ("STORY_EXP_TABLE", json => StoryExpTable = JsonUtility.FromJson<StoryExpTable>(json)),
            ("STORY_STAGE_TABLE", json => StoryStageTable = JsonUtility.FromJson<StoryStageTable>(json)),
            ("TUTORIAL_TABLE", json => TutorialTable = JsonUtility.FromJson<TutorialTable>(json)),
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

    public void LoadGameData()
    {
        if (!File.Exists(SaveFilePath))
        {
            LoadSaveData = null;
            return;
        }
        
        string json = File.ReadAllText(SaveFilePath);
        LoadSaveData = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"게임 진행상황 로딩 완료");
    }

    public void SaveGameData(int chapter, int stage, int wallHp, int wallMaxHp, Dictionary<string, int> rewards)
    {
        Dictionary<string,int> mergeRewards = new Dictionary<string, int>();

        if (LoadSaveData != null && LoadSaveData.saveRewardDatas != null)
        {
            foreach (var saveRewardData in LoadSaveData.saveRewardDatas)
            {
                mergeRewards[saveRewardData.rewardName] = saveRewardData.count;
            }
        }

        foreach (var reward in rewards)
        {
            if (mergeRewards.ContainsKey(reward.Key)) mergeRewards[reward.Key] = reward.Value;
            else  mergeRewards.Add(reward.Key, reward.Value);
        }
        
        SaveData saveData = new SaveData()
        {
            chapter = chapter,
            stage = stage,
            wallHp = wallHp,
            wallMaxHp = wallMaxHp,
            saveRewardDatas = new List<SaveRewardData>()
        };

        foreach (var reward in mergeRewards)
        {
            saveData.saveRewardDatas.Add(new SaveRewardData()
            {
                rewardName = reward.Key,
                count = reward.Value,
            });
        }
        
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SaveFilePath, json);

        LoadSaveData = saveData;
        
        Debug.Log($"{chapter}, {stage}, {rewards.Count} 저장 완료");
    }

    public void DeleteSaveData()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
        }
        
        LoadSaveData = null;
        ResetStageRewardData();
    }
    
    public void StageSelectWithLoadGame()
    {
        LoadGameData();

        if (LoadSaveData == null)
        {
            ResetStageRewardData();
            return;
        }
        
        SetSaveRewardData(LoadSaveData.saveRewardDatas);
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

    public void SetSaveRewardData(List<SaveRewardData> loadRewards)
    {
        ResetStageRewardData();

        if (loadRewards == null) return;
        
        Dictionary<string, int> rewardDict = new Dictionary<string, int>();

        foreach (var rewardData in loadRewards)
        {
            if (_stageRewardCounts.ContainsKey(rewardData.rewardName))
            {
                _stageRewardCounts[rewardData.rewardName] = rewardData.count;
                rewardDict.Add(rewardData.rewardName, rewardData.count);
                
                var reward = StageClearRewardTable.data.Find(x => x.CLEAR_REWARD_ID == rewardData.rewardName);
                if (reward != null && reward.MAX_CLEAR_REWARD_COUNT <= rewardData.count)
                {
                    _stageRewardCounts[rewardData.rewardName] = reward.MAX_CLEAR_REWARD_COUNT;
                    rewardDict[rewardData.rewardName] = reward.MAX_CLEAR_REWARD_COUNT;
                    Debug.Log($"이제 {rewardData.rewardName} 는 등장 안할거야!");
                }
            }
        }

        if (Service.Get<RelicManager>() != null) Service.Get<RelicManager>().SetRelic(rewardDict);
    }

    public Dictionary<string, int> GetSaveRewardData()
    {
        return _stageRewardCounts;
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
    
    public void CheckAndSaveBestStage(int chapter, int stage)
    {
        int saveBestChapter = PlayerPrefs.GetInt(BestChapter, 0);
        int saveBestStage = PlayerPrefs.GetInt(BestStage, 0);

        bool isNewBest = false;

        if (chapter > saveBestChapter) isNewBest = true;
        else if (chapter == saveBestChapter && stage > saveBestStage) isNewBest = true;

        if (isNewBest)
        {
            PlayerPrefs.SetInt(BestChapter, chapter);
            PlayerPrefs.SetInt(BestStage, stage);
            PlayerPrefs.Save();
        }
    }
    
    public (int chapter, int stage) LoadBestStage()
    {
        int bestChapter = PlayerPrefs.GetInt(BestChapter, 1);
        int bestStage = PlayerPrefs.GetInt(BestStage, 1);
        Debug.Log($"{bestChapter} - {bestStage} 가 최고점이야");
        
        return  (bestChapter, bestStage);
    }
}
