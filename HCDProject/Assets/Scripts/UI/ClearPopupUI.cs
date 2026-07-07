using System;
using JetBrains.Annotations;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ClearPopupUI : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent popupTitle;
    [SerializeField] private LocalizeStringEvent positiveButtonText;
    [SerializeField] private LocalizeStringEvent negativeButtonText;


    [SerializeField] private TextMeshProUGUI leftTimeText;
    [SerializeField] private TextMeshProUGUI accrueSortText;
    [SerializeField] private TextMeshProUGUI maxComboText;

    [SerializeField] private GameObject selectedRelicLayer;
    [SerializeField] private RewardButtonUI selectedRelic;

    [SerializeField] private GameObject currentWaveLayer;
    [SerializeField] private TextMeshProUGUI currentStageText;
    [SerializeField] private TextMeshProUGUI currentWaveText;
    [SerializeField] private Slider currentWaveSlider;

    [SerializeField] private GameObject killCountLayer;
    [SerializeField] private TextMeshProUGUI killCountText;


    private ObserveValue<bool> isOpenClearPopup = new();
    [SerializeField] private Button positiveButton;


    public void SetClearPopup(bool hidePosButton)
    {
        gameObject.SetActive(true);
        popupTitle.SetEntry("UI_RESULT_WIN");
        positiveButtonText.SetEntry("UI_POPUP_NEXT");
        negativeButtonText.SetEntry("UI_POPUP_SS");

        selectedRelicLayer.SetActive(true);
        currentWaveLayer.SetActive(false);
        killCountLayer.SetActive(false);
        
        SetTime();
        Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.GetRewardPopup.CopyElement(selectedRelic);

        if (!hidePosButton)
        {
            positiveButton.gameObject.SetActive(true);
            positiveButton.onClick.RemoveAllListeners();
            positiveButton.onClick.AddListener(Service.Get<GameManager>().NextBattle);
        }
        else
        {
            positiveButton.gameObject.SetActive(false);
        }
    }

    public void SetDefeatPopup()
    {
        gameObject.SetActive(true);
        popupTitle.SetEntry("UI_RESULT_LOSE");
        positiveButtonText.SetEntry("UI_POPUP_RETRY");
        negativeButtonText.SetEntry("UI_POPUP_RETREAT");

        selectedRelicLayer.SetActive(false);
        currentWaveLayer.SetActive(true);
        killCountLayer.SetActive(true);

        SetTime();


        Service.Get<UIManager>()?.GetUI<InGameTopUIController>()
            ?.GetCurrentWave(currentWaveSlider, currentStageText, currentWaveText);
        Service.Get<UIManager>()?.GetUI<InGameTopUIController>()?.GetCurrentKill(killCountText);

        positiveButton.onClick.RemoveAllListeners();
        positiveButton.onClick.AddListener(Service.Get<GameManager>().RestartStage);
    }

    private void SetTime()
    {
        float currentTime = Service.Get<TimeManager>().BattleTime;
        leftTimeText.text = string.Format(leftTimeText.text, (int)(currentTime / 60), (int)(currentTime % 60));
        Debug.Log(leftTimeText.text);
    }

    public void AddListener(UnityAction<bool> action)
    {
        isOpenClearPopup.AddListener(action);
    }

    public void RemoveListener(UnityAction<bool> action)
    {
        isOpenClearPopup.RemoveListener(action);
    }
}