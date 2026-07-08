using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class IngamePopupController : BaseUIController<IngamePopupController>
{
    [SerializeField] private GameObject DangerBorder;
    [SerializeField] private GameObject ClearLogo;
    [SerializeField] private GameObject FailLogo;
    [SerializeField] private GameObject WaveClearLogo;
    
    [SerializeField] private RewardUIController RewardPopup;
    public RewardUIController GetRewardPopup => RewardPopup;
    [SerializeField] private ClearPopupUI ClearPopup;
    public ClearPopupUI GetClearPopup => ClearPopup;
    [SerializeField] private GameObject SortWarningPopup;

    public void OnSetDangerBorder(float ratio)
    {
        if (ratio < 0.3f)
        {
            Service.Get<VibrationManager>()?.TriggerVibration();
            DangerBorder.SetActive(true);
        }
        else
            DangerBorder.SetActive(false);
    }

    public void OnGameClear()
    {
        StartCoroutine(ShowLogo(ClearLogo, () =>
        {
            var currentStageData = Service.Get<GameManager>().beforeStageData;
            Service.Get<GameManager>().CheckAndStartNarrative(currentStageData, false, ()=>OnRewardPopup(OnClearPopup));
        }));
    }
    public void OnGameDefeat()
    {
        StartCoroutine(ShowLogo(FailLogo, ClearPopup.SetDefeatPopup));
    }

    public void OnWaveClear(UnityAction action)
    {
        StartCoroutine(ShowLogo(WaveClearLogo, action));
    }

    public IEnumerator ShowLogo(GameObject logo, UnityAction afterAction)
    {
        logo.SetActive(true);
        yield return new WaitForSecondsRealtime(2.0f);
        logo.SetActive(false);
        
        afterAction?.Invoke();
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
        ClearPopup.SetClearPopup(showNextButton);
    }

    public void OnReturnToStageSelect()
    {
        Service.Get<SceneController>()?.ChangeScene(SceneType.StageSelect);
    }
    public void OnNextBattle()
    {
        Service.Get<GameManager>()?.NextBattle();
    }

    private bool showNextButton; 
    public void OnNextButton(bool value)
    {
        showNextButton = value;
    }

    public void OnEndSort()
    {
        SortWarningPopup.SetActive(false);
        Service.Get<SortManager>()?.FinishSortPhase();
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