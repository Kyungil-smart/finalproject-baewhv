using System.Collections.Generic;
using UnityEngine;

public class PoolManager : SingletonMonoBehaviour<PoolManager>
{
    private Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();
    
    public GameObject GetPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!_pool.ContainsKey(prefab))
        {
            _pool.Add(prefab, new Queue<GameObject>());
        }

        GameObject obj = _pool[prefab].Count > 0 ? _pool[prefab].Dequeue() : Instantiate(prefab);

        if (obj == null)
        {
            obj = Instantiate(prefab);
        }
        
        obj.transform.position = position;
        obj.transform.rotation =  rotation;
        obj.SetActive(true);

        return obj;
    }

    public void ReturnPool(GameObject prefab, GameObject obj)
    {
        if (!_pool.ContainsKey(prefab))
        {
            _pool.Add(prefab, new Queue<GameObject>());
        }
        
        obj.SetActive(false);
        _pool[prefab].Enqueue(obj);
    }
}
