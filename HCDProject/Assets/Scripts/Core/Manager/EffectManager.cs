using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EffectManager : BaseManager<EffectManager>
{
    public HashSet<string> EffectIds = new HashSet<string>();
    private Dictionary<string, GameObject> _effectPrefabs = new Dictionary<string, GameObject>();
    
    public void InitEffect()
    {
        foreach (var id in EffectIds)
        {
            Addressables.LoadAssetAsync<GameObject>(id).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _effectPrefabs[id] = handle.Result;
                    Debug.Log($"Effect Load success: {id}");
                }
                else
                {
                    Debug.Log($"Effect Load failed: {id}");
                }
            };
        }
    }

    public void SpawnEffect(string id, Vector2 pos, Quaternion rot)
    {
        GameObject prefab = GetEffectPrefab(id);

        if (prefab != null)
        {
            GameObject obj = Service.Get<PoolManager>()?.GetPool(prefab, pos, rot);
            
            StartCoroutine(DespawnEffect(prefab, obj));
        }
    }

    private IEnumerator DespawnEffect(GameObject origin, GameObject obj)
    {
        yield return new WaitForSeconds(3f);
        
        Service.Get<PoolManager>()?.ReturnPool(obj, origin);
    }
    
    private GameObject GetEffectPrefab(string id)
    {
        if (_effectPrefabs.TryGetValue(id, out GameObject prefab)) return prefab;

        return null;
    }
}
