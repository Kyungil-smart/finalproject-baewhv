using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NarrativeManager : BaseManager<NarrativeManager>
{
    [SerializeField] private GameObject uiObj;
    private NarrativeUIController ui;
    private List<StoryLocalizingRawData> data = new List<StoryLocalizingRawData>();
    private int currentChapter = -1;
    private int currentStage = -1;
    private int currentIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        if (!IsManagerDestroy && !ui)
        {
            ui = Instantiate(uiObj, transform).GetComponent<NarrativeUIController>();
            ui.GameObject().SetActive(false);
        }
    }

    public void StartNarrative(StoryStageRawData data, bool isBefore)
    {
        Debug.Log("NarrativeManager: StartNarrative");
        ui.GameObject().SetActive(false);
        currentStage = data.STAGE;
        currentChapter = data.CHAPTER;
        currentIndex = 0;

        var localData = Service.Get<DataManager>().StoryLocalizingTable.data
            .FindAll(x =>
                x.STAGE == currentStage &&
                x.CHAPTER == currentChapter &&
                x.STAGE_DIALOGUE_EVENT_TYPE == (isBefore ? "BEFORE_STAGE" : "AFTER_STAGE"));
        ui = Service.Get<UIManager>().GetUI<NarrativeUIController>();

        ui.GameObject().SetActive(true);
        ui.SetRegion(data);
        ui.SetNarrative(localData[currentIndex]);
    }

    public void EndNarrative()
    {
        ui.GameObject().SetActive(false);
        Service.Get<GameManager>()?.NarrativeEnd();
    }

    public StoryLocalizingRawData GetNextNarrative()
    {
        if (currentIndex >= data.Count) return null;
        return data[++currentIndex];
    }
}