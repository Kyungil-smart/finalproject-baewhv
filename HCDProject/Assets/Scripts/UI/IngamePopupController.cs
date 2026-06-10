using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class IngamePopupController : BaseUIController<IngamePopupController>
{
    [SerializeField] private GameObject DangerBorder;
    [SerializeField] private GameObject ClearLogo;
    [SerializeField] private GameObject FailLogo;
    
    [SerializeField] private RewardUIController RewardPopup;
    [SerializeField] private GameObject ClearPopup;
    [SerializeField] private GameObject SortWarningPopup;
     

    public void OnSetDangerBorder(float ratio)
    {
        if (ratio < 0.2f)
            DangerBorder.SetActive(true);
        else
            DangerBorder.SetActive(false);
    }

    public void OnGameClear()
    {
        StartCoroutine(ShowEndLogo(true));
    }
    public void OnGameDefeat()
    {
        StartCoroutine(ShowEndLogo(false));
    }

    public IEnumerator ShowEndLogo(bool isClear)
    {
        if (isClear) ClearLogo.SetActive(true);
        else FailLogo.SetActive(true);
        yield return YieldContainer.WaitForSeconds(2.0f);
        ClearLogo.SetActive(false);
        FailLogo.SetActive(false);

        if (isClear) OnRewardPopup();
        else OnFailPopup();
    }

    public void OnRewardPopup(UnityAction action = null)
    {
        RewardPopup.SetRelicReward(action);
    }
    public void OnLevelUpPopup(UnityAction action = null)
    {
        RewardPopup.SetLevelUpReward(action);
    }

    public void OnClearPopup()
    {
        ClearPopup.SetActive(true);
        //TODO : reward Popup은 보상과 연결할 것.
    }
    public void OnFailPopup()
    {
        ClearPopup.SetActive(true);
    }

    public void OnReturnToStageSelect()
    {
        Service.Get<SceneController>()?.ChangeScene(SceneType.StageSelect);
    }
    public void OnNextBattle()
    {
        Service.Get<SceneController>()?.ChangeScene(SceneType.InGame);
    }

    public void OnEndSort()
    {
        SortWarningPopup.SetActive(false);
        Service.Get<SortManager>()?.FinishSortPhase();
        //Service.Get<SceneController>()?
    }
    public void OnShowSortWarningPopup()
    {
        SortWarningPopup.SetActive(true);
    }
    public void OnClose()
    {
        SortWarningPopup.SetActive(false);
    }
}
