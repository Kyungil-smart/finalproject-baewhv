using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

public class NarrativeManager : BaseManager<NarrativeManager>
{
    [SerializeField] private GameObject uiObj;
    private NarrativeUIController ui;
    private List<StoryLocalizingRawData> storyData = new List<StoryLocalizingRawData>();
    private int currentChapter = -1;
    private int currentStage = -1;
    private int currentIndex = 0;
    public Dictionary<string, Color> ColorPicker { get; private set; } = new();

    protected override void Awake()
    {
        base.Awake();
        if (IsManagerDestroy) return;
        SetNarrativeUI();
    }

    private void Start()
    {
        foreach (var d in Service.Get<DataManager>().StaticValueTable.data)
        {
            if (d.VARIABLE_TYPE == "COLOR")
            {
                string name = "";
                switch (d.VARIABLE_NAME)
                {
                    case "COLOR_SERAH":
                        name = "C_001";
                        break;
                    case "COLOR_NOAH":
                        name = "C_002";
                        break;
                    case "COLOR_ALICE":
                        name = "C_003";
                        break;
                    case "COLOR_SPAYNE":
                        name = "C_004";
                        break;
                    case "COLOR_COMMANDER":
                        name = "C_008";
                        break;
                }

                if (string.IsNullOrEmpty(name)) continue;
                Color pickedColor = Color.white;
                ColorUtility.TryParseHtmlString(d.VARIABLE_VALUE, out pickedColor);
                ColorPicker[name] = pickedColor;
            }
        }
    }

    private void OnEnable()
    {
        Service.Get<SceneController>().OnLoadingComplete += SetNarrativeUI;
    }

    private void OnDisable()
    {
        if (Service.Get<SceneController>())
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
        currentStage = data.STAGE;
        currentChapter = data.CHAPTER;
        currentIndex = 0;
        Debug.Log($"StartNarrative{currentChapter}-{currentStage} {(isBefore? "before":"after")}");

        storyData = Service.Get<DataManager>().StoryLocalizingTable.data
            .FindAll(x =>
                {
                    bool success = x.STAGE == currentStage &&
                        x.CHAPTER == currentChapter &&
                        x.STAGE_DIALOGUE_EVENT_TYPE == (isBefore ? "BEFORE_STAGE" : "AFTER_STAGE");
                    if (success)
                    {
                        Service.Get<ResourcesManager>().GetSprite(x.PORTRAIT_L,bind=>{});
                        Service.Get<ResourcesManager>().GetSprite(x.PORTRAIT_R,bind=>{});
                        Service.Get<ResourcesManager>().GetSprite(x.BACKGROUND,bind=>{});
                    }
                    return success;
                }
            );
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