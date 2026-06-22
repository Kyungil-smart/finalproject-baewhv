using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NarrativeUIController : BaseUIController<NarrativeUIController>
{
    //Desc
    [SerializeField] private LocalizeStringEvent nameText;
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private LocalizeStringEvent descText;
    [SerializeField] private TextMeshProUGUI descTMP;
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


    private void Update()
    {
        Debug.Log($"{autoRoutine}");
    }

    private StoryLocalizingRawData currentdata;
    private bool isEnd;

    public void SetRegion(int chapter, int stage, string text)
    {
        StageNumber.text = $"stage {chapter}-{stage}";
        StageText.SetEntry(text);
    }


    private void OnEnable()
    {
        descText.OnUpdateString.AddListener(SetText);
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
    }

    private void SetText(string text)
    {
        descTMP.maxVisibleCharacters = 0;
        decsTweener = DOTween.To(x => descTMP.maxVisibleCharacters = (int)x, 0f, descTMP.text.Length, 1f);
        if(isAuto)
            decsTweener.OnComplete(()=>StartCoroutine(OnAuto()));
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
                decsTweener.OnComplete(() => StartCoroutine(OnAuto()));
        }
        else
        {
            decsTweener.onComplete = null;
        }
    }

    private IEnumerator OnAuto()
    {
        yield return YieldContainer.WaitForSeconds(2.0f);
        OnNextButton();
    }
}