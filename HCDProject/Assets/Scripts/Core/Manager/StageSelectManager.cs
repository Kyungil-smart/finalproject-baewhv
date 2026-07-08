using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StageSelectManager : BaseManager<StageSelectManager>
{
    private int currentChapter;
    private int currentStage;
    private List<StoryStageRawData> stageData;
    
    private void Start()
    {
        Service.Get<SceneController>().OnLoadingComplete += LoadChapterDesign;
        Service.Get<GameManager>().OnStageChange += OnChangeStage;
        currentChapter = Service.Get<GameManager>().CurrentChapter;
        stageData = Service.Get<DataManager>()?.StoryStageTable.data.FindAll(x => x.CHAPTER == currentChapter);
        LoadStage();
    }
    private void OnDisable()
    {
        if(Service.Get<SceneController>())
            Service.Get<SceneController>().OnLoadingComplete -= LoadChapterDesign;
        if(Service.Get<GameManager>())
            Service.Get<GameManager>().OnStageChange -= OnChangeStage;
    }

    public void LoadStage()
    {
        StageSelectUIController SSUI = Service.Get<UIManager>().GetUI<StageSelectUIController>();
        currentStage = Service.Get<GameManager>().CurrentStage;
        
        for (int i = 0; i < SSUI.GetNodesCount; i++)
        {
            if (i >= stageData.Count)
            {
                SSUI.SetStageNode(i, null, EStageType.NORMAL_F, EStageState.Lock);
                continue;
            }
            EStageType type = Enum.Parse<EStageType>(stageData[i].STAGE_TYPE);
            EStageState state = EStageState.Current;
            if (currentStage < stageData[i].STAGE) state = EStageState.Lock;
            else if (currentStage > stageData[i].STAGE) state = EStageState.Clear;
            SSUI.SetStageNode(i, stageData[i], type, state);
        }
    }

    private void OnChangeStage(int stage)
    {
        StageSelectUIController SSUI = Service.Get<UIManager>().GetUI<StageSelectUIController>();
        SSUI.SetClearNode(currentStage);
        EStageType type = Enum.Parse<EStageType>(stageData[stage-1].STAGE_TYPE);
        SSUI.SetOpenNode(stage, type);
        currentStage = stage;   
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
