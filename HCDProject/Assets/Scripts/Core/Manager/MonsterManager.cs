using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MonsterManager : BaseManager<MonsterManager>
{
    private Dictionary<string, GameObject> _monsterPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, AsyncOperationHandle<GameObject>> _monsterHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();

    public void StageMonster(List<string> currentStageMonsterIds, Action OnComplete)
    {
        List<string> release = new List<string>();
        
        foreach (var id in _monsterPrefabs.Keys)
        {
            if (!currentStageMonsterIds.Contains(id)) release.Add(id);
        }

        foreach (var id in release)
        {
            if (_monsterHandles.TryGetValue(id, out var handle))
            {
                Addressables.Release(handle);
                _monsterHandles.Remove(id);
                _monsterPrefabs.Remove(id);
                Debug.Log($"release 성공 : {id}");
            }
        }

        List<string> loadTarget = new List<string>();
        
        foreach (var id in currentStageMonsterIds)
        {
            if (!_monsterPrefabs.ContainsKey(id)) loadTarget.Add(id);
        }

        if (loadTarget.Count == 0)
        {
            OnComplete?.Invoke();
            return;
        }
        
        int maxLoadCount = loadTarget.Count;
        int currentLoadCount = 0;
        
        foreach (var loadId in loadTarget)
        {
            Addressables.LoadAssetAsync<GameObject>(loadId).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _monsterPrefabs[loadId] = handle.Result;
                    _monsterHandles[loadId] = handle;
                    Debug.Log($"load 성공 : {loadId}");
                }
                else Debug.Log($"load 실패 :  {loadId}");

                currentLoadCount++;
                if (currentLoadCount >= maxLoadCount) OnComplete?.Invoke();
            };
        }
    }

    public GameObject GetMonsterPrefab(string monsterAddress)
    {
        if (_monsterPrefabs.TryGetValue(monsterAddress, out GameObject prefab)) return prefab;
        
        Debug.Log($"GetMonsterPrefab 실패 -> {monsterAddress} 가 없어요 ㅠ");
        return null;
    }
}
