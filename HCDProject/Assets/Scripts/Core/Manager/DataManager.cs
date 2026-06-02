using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataManager : BaseManager<DataManager>
{
    // 추후 추가될 데이터 파일 ++
    public MapTable MapTable  {get; private set;}
    public MonsterTable MonsterTable {get; private set;}
    public Level_RewardTable Level_RewardTable {get; private set;}
    public CharacterTable CharacterTable {get; private set;}
    public Stage_Clear_RewardTable Stage_Clear_RewardTable {get; private set;}
    public ObjectTable ObjectTable {get; private set;}
    public Player_Active_SkillTable Player_Active_SkillTable {get; private set;}
    public Monster_SkillTable Monster_SkillTable {get; private set;}
    public Monster_Skill_Effect_GroupTable Monster_Skill_Effect_GroupTable {get; private set;}
    public ProjectileTable ProjectileTable {get; private set;}
    public LocalizingTable LocalizingTable {get; private set;}
    public Story_LocalizingTable Story_LocalizingTable {get; private set;}
    public Static_ValueTable Static_ValueTable {get; private set;}
    public Story_ExpTable Story_ExpTable {get; private set;}

    public void InitData(Action OnDataLoaded)
    {
        (string key, Action<string> assignAction)[] loadList = new (string key, Action<string> assignAction)[]
        {
            ("MAP_TABLE", json => MapTable = JsonUtility.FromJson<MapTable>(json)),
            ("MONSTER_TABLE", json => MonsterTable = JsonUtility.FromJson<MonsterTable>(json)),
            ("LEVEL_REWARD", json => Level_RewardTable = JsonUtility.FromJson<Level_RewardTable>(json)),
            ("CHARACTER_TABLE", json => CharacterTable = JsonUtility.FromJson<CharacterTable>(json)),
            ("STAGE_CLEAR_REWARD_TABLE", json => Stage_Clear_RewardTable = JsonUtility.FromJson<Stage_Clear_RewardTable>(json)),
            ("OBJECT_TABLE", json => ObjectTable = JsonUtility.FromJson<ObjectTable>(json)),
            ("PLAYER_ACTIVE_SKILL_TABLE", json => Player_Active_SkillTable = JsonUtility.FromJson<Player_Active_SkillTable>(json)),
            ("MONSTER_SKILL_TABLE", json => Monster_SkillTable = JsonUtility.FromJson<Monster_SkillTable>(json)),
            ("MONSTER_SKILL_EFFECT_GROUP_TABLE", json => Monster_Skill_Effect_GroupTable = JsonUtility.FromJson<Monster_Skill_Effect_GroupTable>(json)),
            ("PROJECTILE_TABLE", json => ProjectileTable = JsonUtility.FromJson<ProjectileTable>(json)),
            ("LOCALIZING_TABLE", json => LocalizingTable = JsonUtility.FromJson<LocalizingTable>(json)),
            ("STORY_LOCALIZING_TABLE", json => Story_LocalizingTable = JsonUtility.FromJson<Story_LocalizingTable>(json)),
            ("STATIC_VALUE_TABLE", json => Static_ValueTable = JsonUtility.FromJson<Static_ValueTable>(json)),
            ("STORY_EXP_TABLE", json => Story_ExpTable = JsonUtility.FromJson<Story_ExpTable>(json)),
        };
        
        // 총 데이터 파일의 개수 
        int maxLoadCount = 14;
        // 로드 완료된 데이터 파일의 개수
        int currentLoadCount = 0;

        foreach (var load in loadList)
        {
            string key = load.key;
            Action<string> action = load.assignAction;

            Addressables.LoadAssetAsync<TextAsset>(key).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded) action?.Invoke(handle.Result.text);

                Addressables.Release(handle);
                currentLoadCount++;

                if (currentLoadCount >= maxLoadCount) OnDataLoaded?.Invoke();
            };
        }
    }
}
