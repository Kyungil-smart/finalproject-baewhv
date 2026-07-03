using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StageSelectManager : BaseManager<StageSelectManager>
{
    private int currentChapter;
    private List<StoryStageRawData> stageData;
    
    private void Start()
    {
        Service.Get<SceneController>().OnLoadingComplete += LoadChapterDesign;
        currentChapter = Service.Get<GameManager>().CurrentChapter;
        LoadStage();
    }
    private void OnDisable()
    {
        if(Service.Get<SceneController>())
            Service.Get<SceneController>().OnLoadingComplete -= LoadChapterDesign;
    }

    public void LoadStage()
    {
        //stageData = Service.Get<DataManager>();
    }
    private void LoadChapterDesign()
    {
        Addressables.LoadAssetAsync<GameObject>($"Grid/Chapter{currentChapter}").Completed += asset =>
        {
            if (asset.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject Map = Instantiate(asset.Result);
                GridData gd = Map.GetComponent<GridData>();
                Camera.main.transform.position = gd.GetCameraPos;
                Camera.main.orthographicSize = gd.GetOrthographicSize;
            }
        };
    }
}
