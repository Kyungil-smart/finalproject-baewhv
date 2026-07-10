using System;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.Serialization;

public class ArchiveUIController : BaseUIController<ArchiveUIController>
{
    [SerializeField] private GameObject lobbyGroup;
    
    [SerializeField] private GameObject chapterGroup;
    [SerializeField] private Transform chapterContents;
    
    [SerializeField] private GameObject narrativeGroup;
    [SerializeField] private Transform narrativeContents;
    
    [SerializeField] private GameObject characterGroup;

    [SerializeField] private GameObject chapterButtonObject;
    [SerializeField] private GameObject narrativeButtonObject;

    private List<StoryButtonUI> narrativeButtons = new List<StoryButtonUI>();

    private List<StoryStageRawData> data;
    private EArchiveUIType type;

    private void Start()
    {
        SwitchUI(EArchiveUIType.Lobby);
        data = Service.Get<DataManager>().StoryStageTable.data;
        int maxChapter = data[data.Count - 1].CHAPTER;
        Debug.Log(maxChapter);
        for (int i = 1; i <= maxChapter; i++)
        {
            StoryButtonUI obj = Instantiate(chapterButtonObject, chapterContents).GetComponent<StoryButtonUI>();
            int index = i;
            obj.SetButton($"Chapter {i}", () => { OnOpenChapterUI(index); });
        }
    }


    public void OnBackButton()
    {
        switch (type)
        {
            case EArchiveUIType.Lobby:
                Service.Get<SceneController>().ChangeScene(SceneType.ModeSelect);
                break;
            case EArchiveUIType.Story:
                SwitchUI(EArchiveUIType.Lobby);
                break;
            case EArchiveUIType.Chapter:
                SwitchUI(EArchiveUIType.Story);
                HideChapterButton();
                break;
            case EArchiveUIType.Character:
                SwitchUI(EArchiveUIType.Lobby);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SwitchUI(EArchiveUIType inType)
    {
        type = inType;
        lobbyGroup.SetActive(type == EArchiveUIType.Lobby);
        chapterGroup.SetActive(type == EArchiveUIType.Story);
        narrativeGroup.SetActive(type == EArchiveUIType.Chapter);
        characterGroup.SetActive(type == EArchiveUIType.Character);
    }

    public void OnChangeUI(int inType)
    {
        SwitchUI((EArchiveUIType)inType);
    }

    private void OnOpenChapterUI(int index)
    {
        SwitchUI(EArchiveUIType.Chapter);
        narrIndex = 0;


        HashSet<(int, int, bool)> hashes = new();
        foreach (var d in Service.Get<DataManager>().StoryLocalizingTable.data.FindAll(x=> x.CHAPTER == index))
        {
            bool isBefore = d.STAGE_DIALOGUE_EVENT_TYPE == "BEFORE_STAGE";
            if (!hashes.Contains((d.CHAPTER, d.STAGE, isBefore)))
            {
                hashes.Add((d.CHAPTER, d.STAGE, isBefore));
                AddChapterUI(d.CHAPTER, d.STAGE, isBefore);
            }
        }
    }

    private int narrIndex = 0;

    private void AddChapterUI(int chapter, int stage, bool isBefore)
    {
        if (narrativeButtons.Count <= narrIndex)
        {
            StoryButtonUI go = Instantiate(narrativeButtonObject, narrativeContents).GetComponent<StoryButtonUI>();
            narrativeButtons.Add(go);
        }
        var chapterData = data.Find(x => x.CHAPTER == chapter && x.STAGE == stage);
        narrativeButtons[narrIndex].gameObject.SetActive(true);
        narrativeButtons[narrIndex].SetButton($"{chapter} - {stage} {(isBefore ? "Before" : "After")}",
            () => { Service.Get<NarrativeManager>().StartNarrative(chapterData, isBefore); }
        );
        narrIndex++;
    }

    private void HideChapterButton()
    {
        foreach (var button in narrativeButtons)
        {
            button.gameObject.SetActive(false);
        }
    }


    public void OnOpenSettingUI()
    {
        Service.Get<UIManager>()?.OpenOption();
    }
}

public enum EArchiveUIType
{
    Lobby,
    Story,
    Chapter,
    Character
}