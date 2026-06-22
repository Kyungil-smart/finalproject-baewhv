using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : BaseManager<NarrativeManager>
{
    private NarrativeUIController ui;
    private List<StoryLocalizingRawData> data;
    private int currentChapter = -1;
    private int currentStage = -1;

    private int currentIndex = 0;

    private void Start()
    {
        //StartCoroutine(TempSkip());
        if (Service.Get<GameManager>() is GameManager gm)
        {
            currentStage = gm.CurrentStage;
            currentChapter = gm.CurrentChapter;
        }

        data = Service.Get<DataManager>().StoryLocalizingTable.data
            .FindAll(x => x.STAGE == currentStage && x.CHAPTER == currentChapter);
        var sstData = Service.Get<DataManager>().StoryStageTable.data
            .Find(x => x.STAGE == currentStage && x.CHAPTER == currentChapter);
        ui = Service.Get<UIManager>().GetUI<NarrativeUIController>();

    
        Debug.Log($"{data.Count}");
        ui.SetRegion(currentChapter, currentStage, sstData.STAGE_NAME_ID);
        ui.SetNarrative(data[currentIndex]);
    }

    public void EndNarrative()
    {
        Service.Get<GameManager>()?.NarrativeEnd();
    }

    public StoryLocalizingRawData GetNextNarrative()
    {
        if (currentIndex >= data.Count) return null;
        return data[++currentIndex];
    }
}