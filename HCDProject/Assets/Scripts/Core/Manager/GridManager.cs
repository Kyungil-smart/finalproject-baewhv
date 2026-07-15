using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GridManager : BaseManager<GridManager>
{
    [SerializeField] private GameObject tempMap; 
    private void Start()
    {
        var gm = Service.Get<GameManager>();
        int stage = gm.CurrentStage;
        int chapter = gm.CurrentChapter;
        string address = Service.Get<DataManager>().MapTable.data.Find(x=> x.STAGE == stage && x.CHAPTER == chapter).BG_IMG;
        
        LoadChapterDesign(address);
    }
    
    private void LoadChapterDesign(string key)
    {
        Addressables.LoadAssetAsync<GameObject>(key).Completed += asset =>
        {
            if (asset.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject Map = Instantiate(asset.Result, transform);
                Map.transform.SetSiblingIndex(0);
            }
            else if (asset.Status == AsyncOperationStatus.Succeeded)
            {
                Instantiate(tempMap, transform);
            }
        };
    }
}
