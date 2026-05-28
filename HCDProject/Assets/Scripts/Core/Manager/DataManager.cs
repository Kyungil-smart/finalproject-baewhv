using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataManager : BaseManager<DataManager>
{
    // 추후 추가될 데이터 파일 ++
    public MapTable MapTable  {get; private set;}
    public MonsterTable MonsterTable {get; private set;}

    public void InitData(Action OnDataLoaded)
    {
        // 총 데이터 파일의 개수 
        int maxLoadCount = 2;
        // 로드 완료된 데이터 파일의 개수
        int currentLoadCount = 0;

        Addressables.LoadAssetAsync<TextAsset>("MONSTER_TABLE").Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded) MonsterTable = JsonUtility.FromJson<MonsterTable>(handle.Result.text);

            Addressables.Release(handle);
            currentLoadCount++;
            if (currentLoadCount >= maxLoadCount) OnDataLoaded?.Invoke();
        };

        Addressables.LoadAssetAsync<TextAsset>("MAP_TABLE").Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded) MapTable = JsonUtility.FromJson<MapTable>(handle.Result.text);

            Addressables.Release(handle);
            currentLoadCount++;
            if (currentLoadCount >= maxLoadCount) OnDataLoaded?.Invoke();
        };
    }
}
