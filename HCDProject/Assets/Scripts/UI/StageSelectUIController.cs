using System.Collections.Generic;
using UnityEngine;

public class StageSelectUIController : BaseUIController<StageSelectUIController>
{
    [SerializeField] private List<StageNodeUI> stageNodes = new();
    public int GetNodesCount => stageNodes.Count;

    [SerializeField] private List<Sprite> stageSprites;
    [SerializeField] private Sprite lockStageSprites;
    [SerializeField] private Sprite clearStageSprites;
    [SerializeField] private StagePopUpUI stagePopUpUI;
    [SerializeField] private Color clearColor;

    private int _currentChapter;

    public void SetStageNode(int index, StoryStageRawData data, EStageType type, EStageState state)
    {
        if (data == null)
        {
            stageNodes[index].SetActive(false);
            return;
        }

        Sprite sp;
        switch (state)
        {
            case EStageState.Clear:
                sp = clearStageSprites;
                break;
            case EStageState.Lock:
                sp = lockStageSprites;
                break;
            default:
                sp = stageSprites[(int)type];
                break;
        }

        stageNodes[index].SetActive(true)
            .SetImage(sp)
            .SetColor(state == EStageState.Clear ? clearColor : Color.white)
            .SetButtonAction(state != EStageState.Clear ? () => { stagePopUpUI.OpenStagePopup(data, type); } : null)
            .SetText(data.CHAPTER, data.STAGE);
    }

    public void SetClearNode(int index)
    {
        stageNodes[index-1].SetImage(clearStageSprites).SetColor(clearColor);
    }

    public void SetOpenNode(int index, EStageType type)
    {
        stageNodes[index-1].SetImage(stageSprites[(int)type]);
    }

    public void OnOpenSettingUI()
    {
        Service.Get<UIManager>()?.OpenOption();
    }
}

public enum StageState
{
    Lock,
    Clear,
    Current,
    LockSpecial,
    OpenSpecial,
    LockBoss,
    OpenBoss
}

public enum EStageState
{
    Current,
    Lock,
    Clear
}

public enum EStageType
{
    TUTORIAL,
    NORMAL_F,
    EVENT,
    MAINTENANCE,
    BOSS_F
}