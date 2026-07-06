using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class PlayerManager : BaseManager<PlayerManager>
{
    [SerializeField] string _characterAddress; // 프리팹 주소

    private GameObject _loadedPrefab;

    [SerializeField] PlayerStats[] _characterDatas;

    [SerializeField] Transform[] _spawnPoints; // 스폰 및 부활

    [SerializeField] Transform[] _homePoints; // 전투 배치위치

    BaseCharacter[] _characters;

    CharacterStats[] _sortBuffStats; // 소트 버프 누적량 변수

    Coroutine[] _coroutines;

    Coroutine[] _healCoroutines; // 힐링팩터 코루틴

    Coroutine[] _arrowCoroutines; // 화살비 코루틴

    private AsyncOperationHandle<GameObject> _prefabHandle;

    private int _revaivalTime;

    CharacterSlotUI[] _slot;

    public ObserveValue<bool> isAllSpawn = new();

    public bool IsPrefabLoaded => _loadedPrefab != null;

    public BaseCharacter[] Characters => _characters;

    protected override void Awake()
    {
        base.Awake();
        isAllSpawn.Value = false;
    }

    public IEnumerator LoadCharcterPrefabRoutine()
    {
        if (_loadedPrefab != null) yield break;
        
        _prefabHandle = Addressables.LoadAssetAsync<GameObject>(_characterAddress);

        while (!_prefabHandle.IsDone) yield return null;

        _prefabHandle.Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedPrefab = handle.Result;
                Debug.Log("플레이어 프리팹 로드 성공");
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
        Service.Get<GameManager>()?.CurrentState.RemoveListener(OnGameStateChanged);
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
            _sortBuffStats = new CharacterStats[data.Count];
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
            Service.Get<GameManager>().CurrentState.AddListener(OnGameStateChanged);
        }
    }

    public void SpawnAllCharacters()
    {
        GameObject spawnPoints = GameObject.Find("SpawnPoints");
        if (spawnPoints != null) _spawnPoints = spawnPoints.GetComponentsInChildren<Transform>().Where(x => x != spawnPoints.transform).ToArray();
        
        GameObject homePoints = GameObject.Find("HomePoints");
        if (homePoints != null) _homePoints = homePoints.GetComponentsInChildren<Transform>().Where(x => x != homePoints.transform).ToArray();
        
        /*isAllSpawn.Value = false;*/
        var data = Service.Get<DataManager>().CharacterTable.data;
        _characters = new BaseCharacter[data.Count];
        _coroutines = new Coroutine[data.Count];
        _healCoroutines = new Coroutine[data.Count];
        _arrowCoroutines = new Coroutine[data.Count];
        _sortBuffStats = new CharacterStats[data.Count];
        _slot = new CharacterSlotUI[data.Count];
        LoadStaticValues();
        for (int i = 0; i < data.Count; i++)
        {
            GameObject obj = Instantiate(_loadedPrefab, _spawnPoints[i].position, Quaternion.identity);

            BaseCharacter chr = obj.GetComponent<BaseCharacter>();

            chr.homePosition = _homePoints[i].position;
            chr.spawnPosition = _spawnPoints[i].position;
            chr.Init(data[i], _characterDatas[i]);

            _characters[i] = chr;

            var ingameUi = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();

            if (ingameUi != null) _slot[i] = ingameUi.AddCharacter(data[i], chr, i);
        }
        ApplyRelicStats();

        for (int i = 0; i < _characters.Length; i++)
        {
            _characters[i].BindHpUI(_slot[i].SetHPBar);
            _characters[i].BindSkillUI(_slot[i].SetSkillBar);
            _characters[i].BindDeathUI(_slot[i].SetAlive);
        }
        Service.Get<SortManager>()?.AutoSetupUISlots();
        Service.Get<GameManager>().CurrentState.AddListener(OnGameStateChanged);
    }

    public void LoadStaticValues() // 부활시간 테이블 값 불러옴
    {
        var reviveData = Service.Get<DataManager>()?.StaticValueTable.data.Find(x => x.VARIABLE_NAME == "REVAIVAL_TIME");
        if (reviveData != null)
        {
            if (int.TryParse(reviveData.VARIABLE_VALUE, out int value))
            {
                _revaivalTime = value;
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
        var sortBuff = _sortBuffStats[index];
        switch (objType)
        {
            case "OBJ_ATK":
                stats._attackPower += (int)bonus;
                sortBuff._attackPower += (int)bonus;
                break;
            case "OBJ_DEF":
                stats._defense += (int)bonus;
                sortBuff._defense += (int)bonus;
                break;
            case "OBJ_AS":
                stats._attackSpeed += bonus;
                sortBuff._attackSpeed += bonus;
                break;
            case "OBJ_HP":
                stats._maxHp += (int)bonus;
                sortBuff._maxHp += (int)bonus;
                break;
        }
        _characters[index].UpdateStats(stats);
        _sortBuffStats[index] = sortBuff;
    }

    public void ResetSortBuffs() // 웨이브 끝나고 소트 버프 리셋
    {
        for (int i = 0; i < _characters.Length; i++)
        {
            var stats = _characters[i].Stats;
            stats._attackPower -= _sortBuffStats[i]._attackPower;
            stats._defense -= _sortBuffStats[i]._defense;
            stats._attackSpeed -= _sortBuffStats[i]._attackSpeed;
            stats._maxHp -= _sortBuffStats[i]._maxHp;
            _characters[i].UpdateStats(stats);
            _characters[i].SetHeal(0);
            _sortBuffStats[i] = new CharacterStats();
        }
    }

    public void SetCharacterBattleState(bool battle) // 전투상태 감지
    {
        for (int i = 0; i < _characters.Length; i++)
        {
            _characters[i].isBattle = battle;
        }
    }

    private void OnGameStateChanged(GameState state) // 게임 상태 변화 감지 → 상태별 처리
    {
        if (state == GameState.Sort) ResetSortBuffs();

        SetCharacterBattleState(state != GameState.Sort);
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
            chr.UpdateStats(stats);
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
            float hpPercent = 0f;
            float atkPercent = 0f;
            float defPercent = 0f;
            float atkSpeedPercent = 0f;
            float moveSpeedPercent = 0f;
            float critRatePercent = 0f;
            float critDamagePercent = 0f;
            _characters[i].GetComponent<PlayerRelics>()?.Init(jobTypes[i]);
            var stats = _characters[i].Stats;
            foreach (string effectType in effectTypes)
            {
                float jobBonus = Service.Get<RelicManager>()?.GetTotalRelicBonus(jobTypes[i], effectType) ?? 0f;
                float allyBonus = Service.Get<RelicManager>()?.GetTotalRelicBonus("ALLY", effectType) ?? 0f;
                float totalBonus = jobBonus + allyBonus;
                if (totalBonus == 0) continue;

                switch (effectType)
                {
                    case "MAX_HP_P":
                        hpPercent += totalBonus;
                        break;
                    case "ATK_P":
                        atkPercent += totalBonus;
                        break;
                    case "DEF_P":
                        defPercent += totalBonus;
                        break;
                    case "ATK_SPEED_P":
                        atkSpeedPercent += totalBonus;
                        break;
                    case "MOVE_SPEED":
                        stats._moveSpeed += totalBonus;
                        break;
                    case "MOVE_SPEED_P":
                        moveSpeedPercent += totalBonus;
                        break;
                    case "DOUBLE_ATK_RATE_P": // 궁수 연속공격 확률
                        var playerStat = _characters[i].PlayerStat;
                        playerStat._doubleAtkRate += totalBonus / 100f;
                        _characters[i].PlayerStat = playerStat;
                        break;
                    case "SKILL_UPGRADE_RANGE_P": // 마법사 스킬범위 확대
                        _characters[i].BaseSkill.skills[1].SKILL_RANGE_X +=
                            _characters[i].BaseSkill.skills[1].SKILL_RANGE_X * totalBonus / 100f;
                        break;
                    case "SKILL_UPGRADE": // 힐러 치유 증폭기
                        _characters[i].BaseSkill.skills[1].SKILL_ABILLITY +=
                            _characters[i].BaseSkill.skills[1].SKILL_ABILLITY * totalBonus / 100f;
                        break;
                    case "ALLY_UPGRADE_P": // 모든스탯 증가
                        hpPercent += totalBonus;
                        atkPercent += totalBonus;
                        defPercent += totalBonus;
                        atkSpeedPercent += totalBonus;
                        moveSpeedPercent += totalBonus;
                        critRatePercent += totalBonus;
                        critDamagePercent += totalBonus;
                        break;
                }
            }
            stats._maxHp += Mathf.CeilToInt(stats._maxHp * hpPercent / 100f);
            stats._attackPower += Mathf.CeilToInt(stats._attackPower * atkPercent / 100f);
            stats._defense += Mathf.CeilToInt(stats._defense * defPercent / 100f);
            stats._attackSpeed += stats._attackSpeed * atkSpeedPercent / 100f;
            stats._moveSpeed += stats._moveSpeed * moveSpeedPercent / 100f;
            stats._critRate += stats._critRate * critRatePercent / 100f;
            stats._critDamage += stats._critDamage * critDamagePercent / 100f;
            _characters[i].UpdateStats(stats);
            _characters[i].Movement.Agent.speed = _characters[i].Stats._moveSpeed;

            float healBonus = Service.Get<RelicManager>()?
                .GetTotalRelicBonus(jobTypes[i], "MAX_HP_RECOVERY_P") ?? 0f;
            if (healBonus > 0) // 힐링팩터 적용
            {
                _healCoroutines[i] = StartCoroutine(HealingFactor(_characters[i], jobTypes[i]));
            }

            float arrowBonus = Service.Get<RelicManager>()?
                .GetTotalRelicBonus(jobTypes[i], "SKILL_UPGRADE_DAMAGE_P") ?? 0F;
            if (arrowBonus > 0 && jobTypes[i] == "ARCHER") // 화살비 적용
            {
                var skillTable = Service.Get<DataManager>().SkillTable.data;
                var skillData = skillTable.Find(s => s.SKILL_ID == "6508");
                if (skillData != null)
                {
                    if (_characters[i].BaseSkill.skills.Find(s => s.SKILL_ID == "6508") == null)
                        _characters[i].BaseSkill.skills.Add(new Skill(skillData));
                } 
                _arrowCoroutines[i] = StartCoroutine(RainOfArrows(_characters[i], jobTypes[i]));
            }

            float earthquakeBonus = Service.Get<RelicManager>()?
                .GetTotalRelicBonus(jobTypes[i], "EARTHQUAKE_DAMAGE_P") ?? 0f;
            Debug.Log($"[지진 체크] i={i} / job={jobTypes[i]} / earthquakeBonus={earthquakeBonus}");
            if (earthquakeBonus > 0 && jobTypes[i] == "WIZARD") // 지진마법 적용
            {
                var skillTable = Service.Get<DataManager>().SkillTable.data;
                var skillData = skillTable.Find(s => s.SKILL_ID == "6509");
                Debug.Log($"[지진 추적1] skillData null? {skillData == null}");
                if (skillData != null)
                {
                    if (_characters[i].BaseSkill.skills.Find(s => s.SKILL_ID == "6509") == null)
                        _characters[i].BaseSkill.skills.Add(new Skill(skillData));
                    Debug.Log($"[지진 추적2] skills.Add 완료! Count={_characters[i].BaseSkill.skills.Count}");
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
            /*Debug.Log($"[화살비] {character.gameObject.name} 발동! / " +
          $"타겟: {character.GetCurrentTarget.GetTargetObject.name} / " +
          $"DAMAGE_P: {jobBonus} / SKILL_CD: {skillCdValue}");*/
            character.FireRainOfArrows(character.GetCurrentTarget.GetTargetObject.transform.position,
                Mathf.CeilToInt(character.Stats._attackPower * character.BaseSkill.skills[2].SKILL_AB_01
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
        float elapsed = _revaivalTime;
        while (elapsed >= 0)
        {
            elapsed -= Time.deltaTime;
            _slot[index].SetDeathCount(elapsed, _revaivalTime); // 부활 쿨타임 UI 연동
            yield return null;
        }

        character.gameObject.SetActive(true);

        // SpawnState로 전환 (Exit()에서 Revive() 호출됨)
        character.state.ChangeState(character.spawn);
        Debug.Log($"{character.gameObject.name} 부활 완료");
    }
}
