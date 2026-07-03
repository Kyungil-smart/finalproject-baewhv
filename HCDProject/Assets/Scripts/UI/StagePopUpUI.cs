using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StagePopUpUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private LocalizeStringEvent stageTypeText;
    [SerializeField] private GameObject imageLayout;
    [SerializeField] private Image stageImage;
    [SerializeField] private Sprite TempCharacterImage;
    [SerializeField] private Sprite TempStageImage;

    [SerializeField] private TextMeshProUGUI EventTopText;
    [SerializeField] private TextMeshProUGUI EventBottomText;
    
    [SerializeField] private GameObject MaintanenceLayout;
    [SerializeField] private GameObject RelicLayout;
    [SerializeField] private GameObject HPGaugeLayout;
        
    [SerializeField] private GameObject buttonLayout;
    [SerializeField] private Button positiveButton;
    [SerializeField] private LocalizeStringEvent positiveText;
    [SerializeField] private Button negativeButton;
    [SerializeField] private LocalizeStringEvent negativeText;


    private StoryStageRawData data;

    public void InitLayout()
    {
        imageLayout.SetActive(false);
        buttonLayout.SetActive(false);
        MaintanenceLayout.SetActive(false);
        RelicLayout.SetActive(false); 
        EventTopText.gameObject.SetActive(false);
        EventBottomText.gameObject.SetActive(false);
    }

    public void OpenStagePopup(StoryStageRawData _data)
    {
        data = _data;
        InitLayout();
        stageText.text = $"Stage {_data.CHAPTER} - {_data.STAGE}";
        stageTypeText.SetEntry($"UI_STAGE_{_data.STAGE_TYPE}");
        
        StageType type = Enum.Parse<StageType>(_data.STAGE_TYPE);
        switch (type)
        {
            case StageType.NORMAL_F:
            case StageType.TUTORIAL:
                SetNormal();
                break;
            case StageType.EVENT:
                SetEvent();
                break;
            case StageType.MAINTENANCE:
                SetMaintenance();
                break;
            case StageType.BOSS_F:
                SetBoss();
                break;
        }
    }
    

    private void SetEvent()
    {
        imageLayout.SetActive(true);
        stageImage.sprite = TempCharacterImage;
        
    }

    private void SetMaintenance()
    {
        MaintanenceLayout.SetActive(true);
        
    }

    private void SetNormal()
    {
        imageLayout.SetActive(true);
        stageImage.sprite = TempStageImage;
        
        SetBottomButton(StageType.NORMAL_F, () => { }, () => { });
    }

    private void SetBoss()
    {
        SetBottomButton(StageType.BOSS_F, () => { }, () => { });
    }

    private void SetRelic(bool isPresent)
    {
    }

    private void SetBottomButton(StageType type, UnityAction positive, UnityAction negative)
    {
        buttonLayout.SetActive(true);
        positiveButton.onClick.RemoveAllListeners();
        negativeButton.onClick.RemoveAllListeners();
        positiveButton.onClick.AddListener(positive);
        negativeButton.onClick.AddListener(negative);
        positiveText.SetEntry(type == StageType.EVENT ? "UI_POPUP_CHECK" : "UI_POPUP_START");
        positiveText.SetEntry("UI_POPUP_BACK");
    }
}

public enum StageType
{
    TUTORIAL,
    NORMAL_F,
    EVENT,
    MAINTENANCE,
    BOSS_F
}