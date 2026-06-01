using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerManager : BaseManager<PlayerManager>
{
    [SerializeField] string _characterAddress; // 프리팹 주소

    private GameObject _loadedPrefab;
    
    [SerializeField] CharacterBaseData[] _characterDatas;

    [SerializeField] Transform[] _spawnPoints; // 스폰 및 부활

    [SerializeField] Transform[] _homePoints; // 전투 배치위치

    BaseCharacter[] _characters;

    public BaseCharacter[] Characters => _characters;

    protected override void Awake()
    {
        base.Awake();
        LoadCharcterPrefab();
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
        _characters = new BaseCharacter[_characterDatas.Length];

        for (int i = 0; i < _characterDatas.Length; i++)
        {
            GameObject obj = Instantiate(_loadedPrefab, _spawnPoints[i].position, Quaternion.identity);

            BaseCharacter chr = obj.GetComponent<BaseCharacter>();

            chr.homePosition = _homePoints[i].position;
            chr.spawnPosition = _spawnPoints[i].position;
            chr.Init(_characterDatas[i]);
            _characters[i] = chr;
            Debug.Log($"{i}번 플레이어 생성 완료");
        }
    }

    public void StartRevive(BaseCharacter character)
    {
        StartCoroutine(ReviveCoroutine(character));
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
