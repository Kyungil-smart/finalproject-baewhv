using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

public class NarrativeUIController : BaseUIController<NarrativeUIController>
{
    //Desc
    [SerializeField] private LocalizeStringEvent nameText;
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private LocalizeStringEvent descText;
    [SerializeField] private TextMeshProUGUI descTMP;
    [SerializeField] private GameObject descEndIcon;
    private Tweener decsTweener;

    //Character
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private Image ColorLine;

    //Auto
    [SerializeField] private GameObject autoStatusText;
    [SerializeField] private TextMeshProUGUI autoText;
    [SerializeField] private Image autoLogo;
    private bool isAuto;
    [SerializeField] private Color autoColor;
    Color defaultColor = Color.white;
    private Coroutine autoRoutine;

    //Queue
    [SerializeField] private NarrativeUIQueue queue;

    //Region
    [SerializeField] private TextMeshProUGUI StageNumber;
    [SerializeField] private LocalizeStringEvent StageText;

    [SerializeField] private Image Background;
    [SerializeField] private Image Certain;

    private StoryLocalizingRawData currentdata;
    private bool isEnd;

    public void SetRegion(StoryStageRawData data)
    {
        StageNumber.text = $"stage {data.CHAPTER}-{data.STAGE}";
        StageText.SetEntry(data.STAGE_NAME_ID);
    }


    private void OnEnable()
    {
        descText.OnUpdateString.AddListener(SetText);
        Certain.color = Color.black;
    }

    private void OnDisable()
    {
        descText.OnUpdateString.RemoveListener(SetText);
    }

    public void SetNarrative(StoryLocalizingRawData data)
    {
        if (data == null)
        {
            Service.Get<NarrativeManager>().EndNarrative();
            isEnd = true;
            return;
        }

        currentdata = data;
        if (string.IsNullOrEmpty(data.NAME))
            nameTMP.text = "";
        else
            nameText.SetEntry(data.NAME);
        if (string.IsNullOrEmpty(data.NAME))
            descTMP.text = "";
        else
            descText.SetEntry(data.TEXT_ID);

        Color temp;
        switch (data.NAME)
        {
            case "C_001":
                ColorUtility.TryParseHtmlString("#BC3F3F", out temp);
                break;
            case "C_002":
                ColorUtility.TryParseHtmlString("#A25FA6", out temp);
                break;
            case "C_003":
                ColorUtility.TryParseHtmlString("#EEE83B", out temp);
                break;
            case "C_004":
                ColorUtility.TryParseHtmlString("#59C8FF", out temp);
                break;
            default:
                temp = Color.white;
                break;
        }
        ColorLine.color = temp;

        
        
        switch (data.CATEGORY)
        {
            case "FADEIN":
                decsTweener = Certain.DOFade(0.0f, 1.0f).SetUpdate(true);
                decsTweener.onComplete += SetEndText;
                break;
            case "FADEOUT":
                decsTweener = Certain.DOFade(1.0f, 1.0f).SetUpdate(true);
                decsTweener.onComplete += SetEndText;
                break;
        }
    }

    private void SetText(string text)
    {
        descTMP.maxVisibleCharacters = 0;
        decsTweener = DOTween.To(x => descTMP.maxVisibleCharacters = (int)x, 0f, descTMP.text.Length, 1f).SetUpdate(true);
        if (isAuto)
            decsTweener.onComplete += SetAuto;
        decsTweener.onComplete += SetEndText;
    }

    public void OnNextButton()
    {
        if (isEnd) return;
        //텍스트 연출 중이라면 스킵
        if (decsTweener != null && decsTweener.active)
        {
            decsTweener.Complete();
            return;
        }


        //연출이 끝났으면 다음 텍스트 출력.
        if (string.IsNullOrEmpty(currentdata.NEXT_ID))
        {
            Service.Get<NarrativeManager>().EndNarrative();
            isEnd = true;
        }
        else
            SetNarrative(Service.Get<NarrativeManager>().GetNextNarrative());

        descEndIcon.SetActive(false);
    }

    public void OnSkipButton()
    {
        if (isEnd) return;
        Service.Get<NarrativeManager>().EndNarrative();
        isEnd = true;
    }

    public void OnToggleAuto()
    {
        isAuto = !isAuto;
        autoStatusText.SetActive(isAuto);
        autoText.color = isAuto ? autoColor : defaultColor;
        autoLogo.color = isAuto ? autoColor : defaultColor;


        if (isAuto)
        {
            if (decsTweener == null || !decsTweener.active)
                autoRoutine = StartCoroutine(OnAuto());
            else
                decsTweener.OnComplete(SetAuto);
        }
        else
        {
            if (decsTweener != null)
                decsTweener.onComplete -= SetAuto;
        }
    }

    private void SetAuto()
    {
        StartCoroutine(OnAuto());
    }

    private void SetEndText()
    {
        descEndIcon.SetActive(true);
    }


    private IEnumerator OnAuto()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        OnNextButton();
    }
}