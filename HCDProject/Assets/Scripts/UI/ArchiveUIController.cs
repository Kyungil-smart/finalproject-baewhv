using System;
using System.Collections.Generic;
using UnityEngine;

public class ArchiveUIController : BaseUIController<ArchiveUIController>
{
    [SerializeField] private GameObject lobbyGroup;
    [SerializeField] private GameObject narrativeGroup;
    [SerializeField] private Transform narrativeContents;
    [SerializeField] private GameObject chapterGroup;
    [SerializeField] private GameObject characterGroup;

    [SerializeField] private GameObject chapterButtonObject;

    private List<StoryStageRawData> data;
    private EArchiveUIType type;

    private void Start()
    {
        SwitchUI(EArchiveUIType.Lobby);
        data = Service.Get<DataManager>().StoryStageTable.data;
        int maxChapter = data[data.Count-1].CHAPTER;
        for (int i = 1; i <= maxChapter; i++)
        {
            StoryButtonUI obj = Instantiate(chapterButtonObject, narrativeContents).GetComponent<StoryButtonUI>();
            int index = i;
            obj.SetButton($"Chapter {i}",()=>
            {
                OnOpenChapterUI(index);
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
        narrativeGroup.SetActive(type == EArchiveUIType.Story);
        chapterGroup.SetActive(type == EArchiveUIType.Chapter);
        characterGroup.SetActive(type == EArchiveUIType.Character);
    }

    public void OnChangeUI(int inType)
    {
        SwitchUI((EArchiveUIType)inType); 
    }

    private void OnOpenChapterUI(int index)
    {
        SwitchUI(EArchiveUIType.Chapter);
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