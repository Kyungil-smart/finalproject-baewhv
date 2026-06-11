using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class PlayerManager : BaseManager<PlayerManager>
{
    [SerializeField] string _characterAddress; // 프리팹 주소

    private GameObject _loadedPrefab;

    [SerializeField] PlayerStats[] _characterDatas;

    [SerializeField] Transform[] _spawnPoints; // 스폰 및 부활

    [SerializeField] Transform[] _homePoints; // 전투 배치위치

    BaseCharacter[] _characters;

    Coroutine[] _coroutines;

    Coroutine[] _healCoroutines; // 힐링팩터 코루틴

    public ObserveValue<bool> isAllSpawn = new();

    public BaseCharacter[] Characters => _characters;

    protected override void Awake()
    {
        base.Awake();
        isAllSpawn.Value = false;
        LoadCharcterPrefab();
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            _characters[0]?.TryUseActiveSkill();
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            _characters[1]?.TryUseActiveSkill();
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            _characters[2]?.TryDotFieldSkill();
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            _characters[3]?.TryUseActiveSkill();
        }

    }

    private void LoadCharcterPrefab()
    {
        Addressables.LoadAssetAsync<GameObject>(_characterAddress).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedPrefab = handle.Result;
                Debug.Log("플레이어 프리팹 로드 성공");
                SpawnAllCharacters();
            }

            else
            {
                Debug.LogError($"로드 실패 : {_characterAddress}");
            }
        };
    }

    private void SpawnAllCharacters()
    {
        var data = Service.Get<DataManager>().CharacterTable.data;
        _characters = new BaseCharacter[data.Count];
        _coroutines = new Coroutine[data.Count];
        _healCoroutines = new Coroutine[data.Count];
        var slots = Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetSlots;

        for (int i = 0; i < data.Count; i++)
        {
            GameObject obj = Instantiate(_loadedPrefab, _spawnPoints[i].position, Quaternion.identity);

            BaseCharacter chr = obj.GetComponent<BaseCharacter>();

            chr.homePosition = _homePoints[i].position;
            chr.spawnPosition = _spawnPoints[i].position;
            chr.Init(data[i], _characterDatas[i]);
            _characters[i] = chr;
            Debug.Log($"{i}번 플레이어 생성 완료");
        }
        
        ApplyRelicStats();

        for (int i = 0; i < data.Count; i++)
        {
            if (slots != null)
            {
                _characters[i].BindHpUI(slots[i].SetHPBar);
            }
        }
    }

    public void IsAllSpawnPlayer()
    {
        foreach (BaseCharacter chr in _characters)
        {
            if (chr.IsSpawning)
            {
                return;
            }
        }
        isAllSpawn.Value = true;
    }

    public void ApplyBuff(int index, string objType, float bonus) // Sort 적용 함수
    {
        var stats = _characters[index].Stats;
        switch (objType)
        {
            case "OBJ_ATK":
                stats._attackPower += (int)bonus;
                break;
            case "OBJ_DEF":
                stats._defense += (int)bonus;
                break;
            case "OBJ_AS":
                stats._attackSpeed += bonus;
                break;
            case "OBJ_HP":
                stats._maxHp += (int)bonus;
                break;
        }
        _characters[index].Stats = stats;
    }

    public void ApplyLevelReward(LevelRewardRawData reward) // 레벨업 보상 호출
    {
        ApplySingleStat(reward.LEVEL_REWARD_TYPE_01, reward.LEVEL_REWARD_01);
        ApplySingleStat(reward.LEVEL_REWARD_TYPE_02, reward.LEVEL_REWARD_02);
    }

    public void ApplySingleStat(string rewardType, float bonus) // 레벨업 보상 스탯
    {
        if (rewardType == "NONE") return;
        foreach (BaseCharacter chr in _characters)
        {
            if (chr == null) continue;
            var stats = chr.Stats;
            switch (rewardType)
            {
                case "ATK":
                    stats._attackPower += (int)bonus;
                    break;
                case "DEF":
                    stats._defense += (int)bonus;
                    break;
                case "ATK_SPEED":
                    stats._attackSpeed += bonus;
                    break;
                case "MAX_HP":
                    stats._maxHp += (int)bonus;
                    break;
                case "HP":
                    chr.SetHeal(Mathf.CeilToInt(bonus));
                    break;
                case "MOVE_SPEED":
                    stats._moveSpeed += bonus;
                    break;
                case "CRITICAL_RATE":
                    stats._critRate += bonus;
                    break;
                case "CRITICAL_DAMAGE":
                    stats._critDamage += bonus;
                    break;
            }
            chr.Stats = stats;
        }
    }

    public void ApplyRelicStats() // 유물적용(SCR_001,002,003,004,006,007,009,010,012,015)
    {
        string[] jobTypes = { "WARRIOR", "ARCHER", "WIZARD", "HEALER" };
        string[] effectTypes =
        {
            "MAX_HP_P",
            "ATK_P",
            "DEF_P",
            "ATK_SPEED_P",
            "MOVE_SPEED",
            "MOVE_SPEED_P",
            "DOUBLE_ATK_RATE_P",
            "SKILL_UPGRADE_RANGE_P",
            "ALLY_UPGRADE_P",
            "SKILL_UPGRADE"
        };
        for (int i = 0; i < _characters.Length; i++)
        {
            foreach(string effectType in effectTypes)
            {
                float jobBonus = Service.Get<RelicManager>()?.GetTotalRelicBonus(jobTypes[i], effectType) ?? 0f;
                float allyBonus = Service.Get<RelicManager>()?.GetTotalRelicBonus("ALLY", effectType) ?? 0f;
                float totalBonus = jobBonus + allyBonus;
                if (totalBonus == 0) continue;

                var stats = _characters[i].Stats;

                switch (effectType)
                {
                    case "MAX_HP_P":
                        stats._maxHp += Mathf.CeilToInt(stats._maxHp * totalBonus / 100f);
                        break;
                    case "ATK_P":
                        stats._attackPower += Mathf.CeilToInt(stats._attackPower * totalBonus / 100f);
                        break;
                    case "DEF_P":
                        stats._defense += Mathf.CeilToInt(stats._defense * totalBonus / 100f);
                        break;
                    case "ATK_SPEED_P":
                        stats._attackSpeed += stats._attackSpeed * totalBonus / 100f;
                        break;
                    case "MOVE_SPEED":
                        stats._moveSpeed += totalBonus;
                        break;
                    case "MOVE_SPEED_P":
                        stats._moveSpeed += stats._moveSpeed * totalBonus / 100f;
                        break;
                    case "DOUBLE_ATK_RATE_P": // 궁수 연속공격 확률
                        var playerStat = _characters[i].PlayerStat;
                        playerStat._doubleAtkRate += totalBonus / 100f;
                        _characters[i].PlayerStat = playerStat;
                        break;
                    case "SKILL_UPGRADE_RANGE_P": // 마법사 스킬범위 확대
                        _characters[i].skills[1].SKILL_RANGE_X += 
                            _characters[i].skills[1].SKILL_RANGE_X * totalBonus / 100f;
                        break;
                    case "SKILL_UPGRADE": // 힐러 치유 증폭기
                        _characters[i].skills[1].SKILL_ABILLITY +=
                            _characters[i].skills[1].SKILL_ABILLITY * totalBonus / 100f;
                        break;
                    case "ALLY_UPGRADE_P": // 모든스탯 증가
                        stats._maxHp += Mathf.CeilToInt(stats._maxHp * totalBonus / 100f);
                        stats._attackPower += Mathf.CeilToInt(stats._attackPower * totalBonus / 100f);
                        stats._defense += Mathf.CeilToInt(stats._defense * totalBonus / 100f);
                        stats._attackSpeed += stats._attackSpeed * totalBonus / 100f;
                        stats._moveSpeed += stats._moveSpeed * totalBonus / 100f;
                        stats._critRate += stats._critRate * totalBonus / 100f;
                        stats._critDamage += stats._critDamage * totalBonus / 100f;
                        break;
                }
                _characters[i].Stats = stats;
            }
            _characters[i].Movement.Agent.speed = _characters[i].Stats._moveSpeed;

            float healBonus = Service.Get<RelicManager>()?
                .GetTotalRelicBonus(jobTypes[i], "MAX_HP_RECOVERY_P") ?? 0f;
            if (healBonus > 0) // 힐링팩터 적용
            {
                _healCoroutines[i] = StartCoroutine(HealingFactor(_characters[i], jobTypes[i]));
            }
        }
    }

    public IEnumerator HealingFactor(BaseCharacter character, string jobType) // 힐링팩터 유물(워리어)
    {
        while(true)
        {
            float jobBonus = Service.Get<RelicManager>()?.GetTotalRelicBonus(jobType, "MAX_HP_RECOVERY_P") ?? 0f;
            yield return YieldContainer.WaitForSeconds(1f);
            if (character._isDead) continue;
            character.SetHeal(Mathf.CeilToInt(character.Stats._maxHp * jobBonus / 100f));
        }
    }

    public void StopAllHealCoroutines() // 스테이지 종료시 호출
    {
        for (int i = 0; i < _healCoroutines.Length; i++)
        {
            if (_healCoroutines[i] != null)
            {
                StopCoroutine(_healCoroutines[i]);
                _healCoroutines[i] = null;
            }
        }
    }

    public void StartRevive(BaseCharacter character)
    {
        int index = Array.IndexOf(_characters, character);
        _coroutines[index] = StartCoroutine(ReviveCoroutine(character));
    }

    public void ImmediateRevive(BaseCharacter character) // 플레이어 부활실행
    {
        int index = Array.IndexOf(_characters, character);
        if (_coroutines[index] != null)
            StopCoroutine(_coroutines[index]);
        character.gameObject.SetActive(true);
        character.state.ChangeState(character.spawn);
    }

    private IEnumerator ReviveCoroutine(BaseCharacter character) // 플레이어 부활호출
    {
        yield return YieldContainer.WaitForSeconds(character.ReviveTime);

        character.gameObject.SetActive(true);

        // SpawnState로 전환 (Exit()에서 Revive() 호출됨)
        character.state.ChangeState(character.spawn);
        Debug.Log($"{character.gameObject.name} 부활 완료");
    }
}
