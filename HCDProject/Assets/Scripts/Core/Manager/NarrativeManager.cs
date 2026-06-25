using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NarrativeManager : BaseManager<NarrativeManager>
{
    [SerializeField] private GameObject uiObj;
    private NarrativeUIController ui;
    private List<StoryLocalizingRawData> storyData = new List<StoryLocalizingRawData>();
    private int currentChapter = -1;
    private int currentStage = -1;
    private int currentIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        if (IsManagerDestroy) return;
        SetNarrativeUI();
    }

    private void OnEnable()
    {
        Service.Get<SceneController>().OnLoadingComplete += SetNarrativeUI;
    }

    private void OnDisable()
    {
        Service.Get<SceneController>().OnLoadingComplete -= SetNarrativeUI;
    }


    private void SetNarrativeUI()
    {
        if (!ui)
        {
            ui = Instantiate(uiObj).GetComponent<NarrativeUIController>();
            ui.GameObject().SetActive(false);
        }
    }

    public void StartNarrative(StoryStageRawData data, bool isBefore)
    {
        ui.GameObject().SetActive(false);
        currentStage = data.STAGE;
        currentChapter = data.CHAPTER;
        currentIndex = 0;

        storyData = Service.Get<DataManager>().StoryLocalizingTable.data
            .FindAll(x =>
                x.STAGE == currentStage &&
                x.CHAPTER == currentChapter &&
                x.STAGE_DIALOGUE_EVENT_TYPE == (isBefore ? "BEFORE_STAGE" : "AFTER_STAGE"));
        Debug.Log($"NarrativeManager: StartNarrative {storyData.Count}");
        ui = Service.Get<UIManager>().GetUI<NarrativeUIController>();

        ui.GameObject().SetActive(true);
        ui.InitData(data);
        ui.SetNarrative(storyData[currentIndex]);
    }

    public void EndNarrative()
    {
        ui.GameObject().SetActive(false);
        Service.Get<GameManager>()?.NarrativeEnd();
    }

    public StoryLocalizingRawData GetNextNarrative()
    {
        if (currentIndex + 1 >= storyData.Count) return null;
        return storyData[++currentIndex];
    }
}