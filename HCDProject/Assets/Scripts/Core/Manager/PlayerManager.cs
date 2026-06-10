using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;

public partial class PlayerManager : BaseManager<PlayerManager>
{
    [SerializeField] string _characterAddress; // 프리팹 주소

    private GameObject _loadedPrefab;

    [SerializeField] PlayerStats[] _characterDatas;

    [SerializeField] Transform[] _spawnPoints; // 스폰 및 부활

    [SerializeField] Transform[] _homePoints; // 전투 배치위치

    BaseCharacter[] _characters;

    Coroutine[] _coroutines;

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
        if(Keyboard.current.digit2Key.wasPressedThisFrame)
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
        var slots = Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetSlots;

        for (int i = 0; i < data.Count; i++)
        {
            GameObject obj = Instantiate(_loadedPrefab, _spawnPoints[i].position, Quaternion.identity);

            BaseCharacter chr = obj.GetComponent<BaseCharacter>();

            chr.homePosition = _homePoints[i].position;
            chr.spawnPosition = _spawnPoints[i].position;
            chr.Init(data[i], _characterDatas[i]);
            if (slots != null)
            {
                chr.BindHpUI(slots[i].SetHPBar);
            }
            _characters[i] = chr;
            Debug.Log($"{i}번 플레이어 생성 완료");
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

    public void ApplyBuff(int index, string objType, float bonus)
    {
        var stats = _characters[index].Stats;
        switch(objType)
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

    public void ApplyLevelReward(LevelRewardRawData reward)
    {
        ApplySingleStat(reward.LEVEL_REWARD_TYPE_01, reward.LEVEL_REWARD_01);
        ApplySingleStat(reward.LEVEL_REWARD_TYPE_02, reward.LEVEL_REWARD_02);
    }

    public void ApplySingleStat(string rewardType, float bonus)
    {
        if (rewardType == "NONE") return;
        foreach(BaseCharacter chr in _characters)
        {
            if (chr == null) continue;
            var stats = chr.Stats;
            switch(rewardType)
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
                    stats._moveSpeed += (int)bonus;
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

    public void StartRevive(BaseCharacter character)
    {
        int index = Array.IndexOf(_characters, character);
        _coroutines[index] = StartCoroutine(ReviveCoroutine(character));
    }

    public void ImmediateRevive(BaseCharacter character) // 플레이어 부활
    {
        int index = Array.IndexOf(_characters, character);
        if (_coroutines[index] != null)
        StopCoroutine(_coroutines[index]);
        character.gameObject.SetActive(true);
        character.state.ChangeState(character.spawn);
    }

    private IEnumerator ReviveCoroutine(BaseCharacter character)
    {
        yield return YieldContainer.WaitForSeconds(character.ReviveTime);

        character.gameObject.SetActive(true);

        // SpawnState로 전환 (Exit()에서 Revive() 호출됨)
        character.state.ChangeState(character.spawn);
        Debug.Log($"{character.gameObject.name} 부활 완료");
    }
}
