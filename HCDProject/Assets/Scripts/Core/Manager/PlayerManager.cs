using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TextCore.Text;

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

    Coroutine[] _arrowCoroutines; // 화살비 코루틴

    private AsyncOperationHandle<GameObject> _prefabHandle;

    CharacterSlotUI[] _slot;

    public ObserveValue<bool> isAllSpawn = new();

    public bool IsPrefabLoaded => _loadedPrefab != null;

    public BaseCharacter[] Characters => _characters;

    protected override void Awake()
    {
        base.Awake();
        isAllSpawn.Value = false;
        LoadCharcterPrefab();
    }

    private void LoadCharcterPrefab()
    {
        _prefabHandle = Addressables.LoadAssetAsync<GameObject>(_characterAddress);

        _prefabHandle.Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedPrefab = handle.Result;
                Debug.Log("플레이어 프리팹 로드 성공");

                SpawnAllCharacters();
                /*if (!Service.Get<TutorialManager>())
                {
                    
                }*/
            }

            else
            {
                Debug.LogError($"로드 실패 : {_characterAddress}");
            }
        };
    }

    private void OnDestroy()
    {
        if (_prefabHandle.IsValid()) // 유효할 때만
            Addressables.Release(_prefabHandle);
        base.OnDestroy();
    }

    public void SpawnSingleCharacter(int index)
    {
        var data = Service.Get<DataManager>().CharacterTable.data;
        if (_loadedPrefab == null)
        {
            Debug.LogWarning("프리팹 아직 로드 안됨");
            return;
        }

        if (_characters == null)
        {
            _characters = new BaseCharacter[data.Count];
            _coroutines = new Coroutine[data.Count];
            _healCoroutines = new Coroutine[data.Count];
            _arrowCoroutines = new Coroutine[data.Count];
            _slot = new CharacterSlotUI[data.Count];
        }
        GameObject obj = Instantiate(_loadedPrefab, _spawnPoints[index].position, Quaternion.identity);
        BaseCharacter chr = obj.GetComponent<BaseCharacter>();
        chr.homePosition = _homePoints[index].position;
        chr.spawnPosition = _spawnPoints[index].position;
        chr.Init(data[index], _characterDatas[index]);
        _characters[index] = chr;
        _slot[index] = Service.Get<UIManager>()
            .GetUI<IngameBottomUIController>().AddCharacter(data[index], chr, index);

        _characters[index].BindHpUI(_slot[index].SetHPBar);
        _characters[index].BindSkillUI(_slot[index].SetSkillBar);
        _characters[index].BindDeathUI(_slot[index].SetAlive);
        if (index == data.Count - 1)
        {
            ApplyRelicStats();
            Service.Get<SortManager>()?.AutoSetupUISlots();
        }
    }

    private void SpawnAllCharacters()
    {
        /*isAllSpawn.Value = false;*/
        var data = Service.Get<DataManager>().CharacterTable.data;
        _characters = new BaseCharacter[data.Count];
        _coroutines = new Coroutine[data.Count];
        _healCoroutines = new Coroutine[data.Count];
        _arrowCoroutines = new Coroutine[data.Count];
        _slot = new CharacterSlotUI[data.Count];
        for (int i = 0; i < data.Count; i++)
        {
            GameObject obj = Instantiate(_loadedPrefab, _spawnPoints[i].position, Quaternion.identity);

            BaseCharacter chr = obj.GetComponent<BaseCharacter>();

            chr.homePosition = _homePoints[i].position;
            chr.spawnPosition = _spawnPoints[i].position;
            chr.Init(data[i], _characterDatas[i]);

            _characters[i] = chr;
            _slot[i] = Service.Get<UIManager>() // 생성 시 캐릭터의 UI 슬롯을 요청함
            .GetUI<IngameBottomUIController>().AddCharacter(data[i], chr, i);
        }
        ApplyRelicStats();

        for (int i = 0; i < _characters.Length; i++)
        {
            _characters[i].BindHpUI(_slot[i].SetHPBar);
            _characters[i].BindSkillUI(_slot[i].SetSkillBar);
            _characters[i].BindDeathUI(_slot[i].SetAlive);
        }
        Service.Get<SortManager>()?.AutoSetupUISlots();
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

    public void ApplyRelicStats() // 유물적용(SCR_001,002,003,004,005,006,007,009,010,012,015)
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
            foreach (string effectType in effectTypes)
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

            float arrowBonus = Service.Get<RelicManager>()?
                .GetTotalRelicBonus(jobTypes[i], "SKILL_UPGRADE_DAMAGE_P") ?? 0F;
            if (arrowBonus > 0) // 화살비 적용
            {
                var skillTable = Service.Get<DataManager>().PlayerSkillTable.data;
                var skillData = skillTable.Find(s => s.SKILL_ID == "6508");
                if (skillData != null)
                {
                    if (_characters[i].skills.Find(s => s.SKILL_ID == "6508") == null)
                   _characters[i].skills.Add(new Skill(skillData));
                } 
                _arrowCoroutines[i] = StartCoroutine(RainOfArrows(_characters[i], jobTypes[i]));
            }

            float earthquakeBonus = Service.Get<RelicManager>()?
                .GetTotalRelicBonus(jobTypes[i], "SKILL_UPGRADE_DAMAGE_P") ?? 0f;
            if (earthquakeBonus > 0) // 지진마법 적용
            {
                var skillTable = Service.Get<DataManager>().PlayerSkillTable.data;
                var skillData = skillTable.Find(s => s.SKILL_ID == "6509");
                if (skillData != null)
                {
                    if (_characters[i].skills.Find(s => s.SKILL_ID == "6509") == null)
                        _characters[i].skills.Add(new Skill(skillData));
                }
            }
        }
    }

    public IEnumerator RainOfArrows(BaseCharacter character, string jobtype) // 화살비 유물(궁수)
    {
        while (true)
        {
            float jobBonus = Service.Get<RelicManager>()?.GetTotalRelicBonus(jobtype, "SKILL_UPGRADE_DAMAGE_P") ?? 0f;
            float skillCdValue = Service.Get<RelicManager>()?.GetTotalRelicBonus(jobtype, "SKILL_CD") ?? 0F;
            yield return YieldContainer.WaitForSeconds(skillCdValue);
            if (character._isDead || character.GetCurrentTarget == null) continue;
            Debug.Log($"[화살비] {character.gameObject.name} 발동! / " +
          $"타겟: {character.GetCurrentTarget.GetTargetObject.name} / " +
          $"DAMAGE_P: {jobBonus} / SKILL_CD: {skillCdValue}");
            character.FireRainOfArrows(character.GetCurrentTarget.GetTargetObject.transform.position,
                Mathf.CeilToInt(character.Stats._attackPower * character.skills[2].SKILL_ABILLITY 
                * (jobBonus / 100f)));
        }
    }

    public IEnumerator HealingFactor(BaseCharacter character, string jobType) // 힐링팩터 유물(워리어)
    {
        while (true)
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
        _coroutines[index] = StartCoroutine(ReviveCoroutine(character, index));
    }

    public void ImmediateRevive(BaseCharacter character) // 플레이어 부활실행
    {
        int index = Array.IndexOf(_characters, character);
        if (_coroutines[index] != null)
            StopCoroutine(_coroutines[index]);
        character.gameObject.SetActive(true);
        character.state.ChangeState(character.spawn);
    }

    private IEnumerator ReviveCoroutine(BaseCharacter character, int index) // 플레이어 부활호출
    {
        float elapsed = character.ReviveTime;
        while (elapsed >= 0)
        {
            elapsed -= Time.deltaTime;
            _slot[index].SetDeathCount(elapsed, character.ReviveTime); // 부활 쿨타임 UI 연동
            yield return null;
        }

        character.gameObject.SetActive(true);

        // SpawnState로 전환 (Exit()에서 Revive() 호출됨)
        character.state.ChangeState(character.spawn);
        Debug.Log($"{character.gameObject.name} 부활 완료");
    }
}
