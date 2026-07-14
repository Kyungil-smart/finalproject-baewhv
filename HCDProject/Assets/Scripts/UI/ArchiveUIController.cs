using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ColorUtility = UnityEngine.ColorUtility;

public class ArchiveUIController : BaseUIController<ArchiveUIController>
{
    [SerializeField] private GameObject lobbyGroup;

    [SerializeField] private GameObject chapterGroup;
    [SerializeField] private Transform chapterContents;
    [SerializeField] private GameObject chapterWarning;

    [SerializeField] private GameObject narrativeGroup;
    [SerializeField] private Transform narrativeContents;

    [SerializeField] private GameObject characterGroup;
    [SerializeField] private Transform characterContents;
    
    [SerializeField] private GameObject detailGroup;
    private CharacterDetailUI detail;

    [SerializeField] private GameObject chapterButtonObject;
    [SerializeField] private GameObject narrativeButtonObject;
    [SerializeField] private GameObject characterCardObject;

    [FormerlySerializedAs("so")] [SerializeField] private CharacterDetailSO[] characterSO;

    private List<StoryButtonUI> narrativeButtons = new List<StoryButtonUI>();
    private int narrIndex = 0;

    private List<StoryStageRawData> data;
    private EArchiveUIType type;
    private int bestChapter;
    private int bestStage;

    private void Start()
    {
        SwitchUI(EArchiveUIType.Lobby);
        detail = detailGroup.GetComponent<CharacterDetailUI>();
        SetStory();
        SetCharacter();
    }

    public void SetStory()
    {
        data = Service.Get<DataManager>().StoryStageTable.data;
        
        (int chapter, int stage) bestStageRaw =  Service.Get<DataManager>().LoadBestStage();
        bestChapter = bestStageRaw.chapter;
        bestStage = bestStageRaw.stage;
        if (bestChapter == 0)
        {
            chapterWarning.SetActive(true);
            return;
        }
        chapterWarning.SetActive(false);

        int maxChapter = data[data.Count - 1].CHAPTER;
        for (int i = 1; i <= maxChapter; i++)
        {
            if (i == bestChapter + 1) break;
            StoryButtonUI obj = Instantiate(chapterButtonObject, chapterContents).GetComponent<StoryButtonUI>();
            int index = i;
            obj.SetButton($"Chapter {i}", () => { OnOpenChapterUI(index); });
        }
    }

    public void SetCharacter()
    {
        foreach (var so in characterSO)
        {
            CharacterCardUI go = Instantiate(characterCardObject, characterContents).GetComponent<CharacterCardUI>();
            string colorValue = Service.Get<DataManager>().StaticValueTable.data.Find(x => x.VARIABLE_NAME == so.color).VARIABLE_VALUE;
            Color pickedColor = Color.white;
            ColorUtility.TryParseHtmlString(colorValue, out pickedColor);
            Sprite portrait = Service.Get<ResourcesManager>().GetSprite(so.address , temp => { });
            go.SetCard(pickedColor, portrait, () =>
            {
                SwitchUI(EArchiveUIType.Detail);
                detail.OpenUI()
                    .SetPortrait(portrait)
                    .SetText(so.charName, so.desc)
                    .SetBGColor(pickedColor);
            });
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
            case EArchiveUIType.Detail:
                SwitchUI(EArchiveUIType.Character);
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
        detailGroup.SetActive(type == EArchiveUIType.Detail);
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
        foreach (var d in Service.Get<DataManager>().StoryLocalizingTable.data.FindAll(x => x.CHAPTER == index))
        {
            if (d.STAGE > bestStage) break;
            bool isBefore = d.STAGE_DIALOGUE_EVENT_TYPE == "BEFORE_STAGE";
            if (!hashes.Contains((d.CHAPTER, d.STAGE, isBefore)))
            {
                hashes.Add((d.CHAPTER, d.STAGE, isBefore));
                AddChapterUI(d.CHAPTER, d.STAGE, isBefore);
            }
        }
    }

    

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
        narrativeButtons[narrIndex].SetImage(chapterData.NARRATIVE_BG_THUBNAIL,
            isBefore ? chapterData.NARRATIVE_B_THUBNAIL : chapterData.NARRATIVE_A_THUBNAIL);
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
    Character,
    Detail,
}