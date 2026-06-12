using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TutorialManager : BaseManager<TutorialManager>
{
    [SerializeField] private GameObject touchShield;
    private Image background;
    [SerializeField] private GameObject touchField;
    [SerializeField] private TutorialScriptsSO scripts;
    [SerializeField] private RectTransform arrowUI;
    [SerializeField] private Transform uiStorage;
    [SerializeField] private TutorialMessageBox message;

    private int currentWave;

    private List<GameObject> GlowUIList = new();

    private UnityAction TouchActions;

    private List<StoryLocalizingRawData> data;

    private int sortRemain = -1;

    private bool IsSkillTutorial;

    private void Start()
    {
        Service.Get<GameManager>()?.CurrentState.AddListener(OnChangeGameStateType);
        Service.Get<MonsterSpawnManager>()?.currentWave.AddListener(OnChangeWave);
        Service.Get<SortManager>()?.RemainingSorts.AddListener(OnRemainingSort);
        Service.Get<UIManager>().GetUI<IngamePopupController>().GetRewardPopup.AddListener(OpenLevelUpPopup);
        background = touchShield.GetComponent<Image>();
    }

    private void OnChangeGameStateType(GameState state)
    {
        if (state == GameState.Sort)
        {
            if (currentWave == 0)
            {
                Tutorial0();
                sortRemain = Service.Get<SortManager>().RemainingSorts.Value;
            }
            else
            {
                touchShield.SetActive(false);
            }
        }
        else if (state == GameState.Wave)
        {
            Debug.Log($"bjm {state} / {currentWave}");
            if (currentWave == 1)
            {
                Tutorial6();
                Debug.Log($"1bjm {state} / {currentWave}");
            }
            if (currentWave == 2)
            {
                Tutorial9();
                Debug.Log($"2bjm {state} / {currentWave}");
            }
        }
        
        
    }


    private void OnSortCount(int count)
    {
    }

    private void OnRemainingSort(int value)
    {
        if (currentWave != 0) return;
        if (Service.Get<SortManager>()?.RemainingSorts.Value == sortRemain - 1) // TODO : 0이될때? 혹은 감소할 때?
        {
            Tutorial3();
        }
    }

    private void OnChangeWave(int value)
    {
        currentWave = value;
    }

    private GameObject CopyNGlow(GameObject raw)
    {
        GameObject clone = Instantiate(raw, uiStorage, false);
        RectTransform rawRt = (RectTransform)raw.transform;
        RectTransform rt = (RectTransform)clone.transform;
        rt.position = rawRt.position;
        rt.sizeDelta = rawRt.sizeDelta;
        rt.localScale = rawRt.localScale;
        clone.GetComponent<UIGlow>()?.StartGlow();
        GlowUIList.Add(clone);
        return clone;
    }

    private void DeleteAllGlow()
    {
        foreach (var temp in GlowUIList)
        {
            temp.gameObject.SetActive(false);
            Destroy(temp);
        }
        GlowUIList.Clear();
    }

    private Tween currentTween;

    private void ShowArrow(Vector2 startPos, Vector2 endPos)
    {
        arrowUI.gameObject.SetActive(true);
        // arrowUI.anchoredPosition = endPos - startPos;
        // //arrowUI.
        // currentTween = arrowUI.GetComponent<Image>().DOFade(0, 0.5f).SetLoops(-1, LoopType.Yoyo);
        foreach (var temp in arrowUI.GetComponents<UIGlow>())
        {
            temp.StartGlow();
        }
    }

    private void HideArrow()
    {
        currentTween.Kill();
        arrowUI.gameObject.SetActive(false);
    }

    private void ShowMessage(string name, string desc, bool isTopPosition = true)
    {
        message.gameObject.SetActive(true);
        message.SetMessage(name, desc, isTopPosition);
    }

    private void HideMessage()
    {
        message.gameObject.SetActive(false);
    }

    public void OnTouchNext()
    {
        TouchActions?.Invoke();
    }

    private void Tutorial0()
    {
        //0 텍스트 메세지 출력
        TouchActions = null;
        touchField.SetActive(true);
        ShowMessage(scripts.GetData[0].title, "TST_043");
        background.DOFade(0.75f, 1);
        TouchActions = Tutorial1;
    }

    private void Tutorial1()
    {
        //1 텍스트 메세지 + 글로우 2개 출력
        TouchActions = null;
        touchField.SetActive(true);
        ShowMessage(scripts.GetData[1].title, "TST_044");
        CopyNGlow(Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetSlots[0].gameObject);
        CopyNGlow(Service.Get<RailManager>().GetRailA[1].gameObject);
        ShowArrow(GlowUIList[1].transform.position, GlowUIList[0].transform.position);
        TouchActions = Tutorial2;
    }

    private void Tutorial2()
    {
        //2 텍스트 메세지 + 글로우 유지
        TouchActions = null;
        touchField.SetActive(false);
        touchShield.SetActive(false);
        HideMessage();
        HideArrow();
        DeleteAllGlow();
        background.DOFade(0f, 1);
    }

    private void Tutorial3()
    {
        //2 텍스트 메세지 + 글로우 유지
        TouchActions = null;
        touchField.SetActive(true);
        touchShield.SetActive(true);
        CopyNGlow(Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetSlots[0].gameObject);
        CopyNGlow(Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetComboView);
        background.DOFade(0.75f, 1);
        TouchActions = Tutorial4;
    }
    
    private void Tutorial4()
    {
        //2 텍스트 메세지 + 글로우 유지
        TouchActions = null;
        HideMessage();
        DeleteAllGlow();
        ShowMessage(scripts.GetData[2].title, scripts.GetData[2].desc);
        CopyNGlow(Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetStartButton);
        TouchActions = Tutorial5;
    }
    private void Tutorial5()
    {
        //2 텍스트 메세지 + 글로우 유지
        TouchActions = null;
        touchShield.SetActive(false);
        touchField.SetActive(false);
        HideMessage();
        DeleteAllGlow();
        background.DOFade(0f, 1);
    }
    
    //wave1
    private void Tutorial6()
    {
        //5 대사 출력 연속
        TouchActions = null;
        touchShield.SetActive(true);
        touchField.SetActive(true);
        ShowMessage(scripts.GetData[3].title, scripts.GetData[3].desc, false);
        TouchActions = Tutorial7;
    }
    private void Tutorial7()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[4].title, scripts.GetData[4].desc, false);
        TouchActions = Tutorial8;
    }
    private void Tutorial8()
    {
        //대사 종료
        TouchActions = null;
        touchField.SetActive(false);
        HideMessage();
    }
    
    
    //wave2
    private void Tutorial9()
    {
        //5 대사 출력 연속
        TouchActions = null;
        touchShield.SetActive(true);
        touchField.SetActive(true);
        ShowMessage(scripts.GetData[5].title, scripts.GetData[5].desc, false);
        TouchActions = Tutorial10;
    }
    private void Tutorial10()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[6].title, scripts.GetData[6].desc, false);
        TouchActions = Tutorial11;
    }
    private void Tutorial11()
    {
        //대사 종료
        TouchActions = null;
        touchField.SetActive(false);
        HideMessage();
    }

    private void OpenLevelUpPopup(bool isActive)
    {
        if (IsSkillTutorial)
        {
            touchShield.SetActive(false);
            Service.Get<UIManager>().GetUI<IngamePopupController>().GetRewardPopup.RemoveListener(OpenLevelUpPopup);
            return;
        }
        touchShield.SetActive(!isActive);
    }
}