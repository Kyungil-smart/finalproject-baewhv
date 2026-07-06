using System;
using System.Collections;
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
    private GameState currentState;

    private IEnumerator Start()
    {
        while (Service.Get<UIManager>() == null || Service.Get<GameManager>() == null ||
               Service.Get<MonsterSpawnManager>() == null) yield return null;

        IngamePopupController ingamePopupController = null;
        while (ingamePopupController == null)
        {
            ingamePopupController = Service.Get<UIManager>()?.GetUI<IngamePopupController>();
            yield return null;
        }
        
        Service.Get<GameManager>()?.CurrentState.AddListener(OnChangeGameStateType);
        Service.Get<MonsterSpawnManager>()?.currentWave.AddListener(OnChangeWave);
        //Service.Get<SortManager>()?.RemainingSorts.AddListener(OnRemainingSort);
        Service.Get<UIManager>()?.GetUI<IngamePopupController>().GetRewardPopup.AddListener(OpenLevelUpPopup);
        Service.Get<UIManager>()?.GetUI<IngamePopupController>().GetClearPopup.AddListener(OpenResultPopup);
        
        background = touchShield.GetComponent<Image>();
    }

    private void OnChangeGameStateType(GameState state)
    {
        currentState = state; 
        if (state == GameState.Sort)
        {
            if (currentWave == 0)
            {
                Service.Get<SortManager>().TutorialBlocks(new List<int> { 0, 1, 2, 3, 0, 1, 2, 3,0,1,2,3 });
                Tutorial0();
                //sortRemain = Service.Get<SortManager>().RemainingSorts.Value;
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
            if (currentWave == 3)
            {
                Tutorial12();
                Debug.Log($"2bjm {state} / {currentWave}");
            }
        }
        else if (state == GameState.Clear)
        {
            Tutorial17();
        }
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

    private void CopyNGlow(GameObject raw)
    {
        GameObject clone = Instantiate(raw, uiStorage, false);
        RectTransform rawRt = (RectTransform)raw.transform;
        RectTransform rt = (RectTransform)clone.transform;
        rt.position = rawRt.position;
        rt.sizeDelta = rawRt.sizeDelta;
        rt.localScale = rawRt.localScale;
        clone.GetComponent<UIGlow>()?.StartGlow();
        GlowUIList.Add(clone);
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
        //CopyNGlow(Service.Get<RailManager>().GetRailA[1].gameObject);
//        ShowArrow(GlowUIList[1].transform.position, GlowUIList[0].transform.position);
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
        //CopyNGlow(Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetComboView);
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
    
    //wave3
    private void Tutorial12()
    {
        //5 대사 출력 연속
        TouchActions = null;
        touchShield.SetActive(true);
        touchField.SetActive(true);
        ShowMessage(scripts.GetData[7].title, scripts.GetData[7].desc, false);
        TouchActions = Tutorial13;
    }
    private void Tutorial13()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[8].title, scripts.GetData[8].desc, false);
        TouchActions = Tutorial14;
    }
    private void Tutorial14()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[9].title, scripts.GetData[9].desc, false);
        TouchActions = Tutorial15;
    }
    private void Tutorial15()
    {
        //대사 종료
        TouchActions = null;
        touchField.SetActive(true);
        background.DOFade(0.75f, 1);
        Service.Get<TimeManager>().SaveTimeScale();
        ShowMessage(scripts.GetData[10].title, scripts.GetData[10].desc);
        CopyNGlow(Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetSlots[0].gameObject);
        TouchActions = Tutorial16;
        HideMessage();
    }
    private void Tutorial16()
    {
        //대사 종료
        TouchActions = null;
        Service.Get<TimeManager>().LoadTimeScale();
        touchShield.SetActive(false);
        touchField.SetActive(false);
        IsSkillTutorial = true;
        //playerUseSkill
        background.DOFade(0, 1);
        DeleteAllGlow();
        HideMessage();
    }
    
    //stageclear
    private void Tutorial17()
    {
        //5 대사 출력 연속
        TouchActions = null;
        touchField.SetActive(true);
        Service.Get<TimeManager>().SaveTimeScale();
        ShowMessage(scripts.GetData[11].title, scripts.GetData[11].desc, false);
        TouchActions = Tutorial18;
    }
    private void Tutorial18()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[12].title, scripts.GetData[12].desc, false);
        TouchActions = Tutorial19;
    }
    private void Tutorial19()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[13].title, scripts.GetData[13].desc, false);
        TouchActions = Tutorial20;
    }
    private void Tutorial20()
    {
        //대사 종료
        TouchActions = null;
        touchField.SetActive(false);
        Service.Get<TimeManager>().LoadTimeScale();
        HideMessage();
    }
    
    //reward
    private void Tutorial21()
    {
        //5 대사 출력 연속
        TouchActions = null;
        touchField.SetActive(true);
        Service.Get<TimeManager>().SaveTimeScale();
        ShowMessage(scripts.GetData[14].title, scripts.GetData[14].desc, false);
        TouchActions = Tutorial22;
    }
    private void Tutorial22()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[15].title, scripts.GetData[15].desc, false);
        TouchActions = Tutorial23;
    }
    private void Tutorial23()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[16].title, scripts.GetData[16].desc, false);
        TouchActions = Tutorial24;
    }
    private void Tutorial24()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[17].title, scripts.GetData[17].desc, false);
        TouchActions = Tutorial25;
    }
    private void Tutorial25()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[18].title, scripts.GetData[18].desc, false);
        TouchActions = Tutorial26;
    }
    private void Tutorial26()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[19].title, scripts.GetData[19].desc, false);
        TouchActions = Tutorial27;
    }

    private void Tutorial27()
    {
        //대사 종료
        TouchActions = null;
        touchField.SetActive(false);
        Service.Get<TimeManager>().LoadTimeScale();
        HideMessage();
    }
    
    //stageclear
    private void Tutorial28()
    {
        //5 대사 출력 연속
        TouchActions = null;
        touchField.SetActive(true);
        ShowMessage(scripts.GetData[20].title, scripts.GetData[20].desc, false);
        TouchActions = Tutorial29;
    }
    private void Tutorial29()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[21].title, scripts.GetData[21].desc, false);
        TouchActions = Tutorial30;
    }

    private void Tutorial30()
    {
        TouchActions = null;
        ShowMessage(scripts.GetData[22].title, scripts.GetData[22].desc, false);
        TouchActions = Tutorial31;
    }
    private void Tutorial31()
    {
        //대사 종료
        TouchActions = null;
        touchField.SetActive(false);
        HideMessage();
    }

    private void OpenLevelUpPopup(bool isActive)
    {
        if (currentState == GameState.Clear)
        {
            Tutorial21();
        }
        if (IsSkillTutorial)
        {
            touchShield.SetActive(false);
            //Service.Get<UIManager>().GetUI<IngamePopupController>().GetRewardPopup.RemoveListener(OpenLevelUpPopup);
            return;
        }
        touchShield.SetActive(!isActive);
    }

    private void OpenResultPopup(bool isActive)
    {
        if (isActive)
            Tutorial28();
    }
}