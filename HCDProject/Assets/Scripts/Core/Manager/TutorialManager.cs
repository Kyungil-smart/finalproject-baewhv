using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TutorialManager : BaseManager<TutorialManager>
{
    [SerializeField] private GameObject TouchShield;
    [SerializeField] private TutorialScriptsSO scripts;
    [SerializeField] private RectTransform ArrowUI;
    private int currentWave;

    private List<GameObject> GlowUIList = new();

    private void Start()
    {
        Service.Get<GameManager>().CurrentState.AddListener(OnChangeGameStateType);
        Service.Get<MonsterSpawnManager>().currentWave.AddListener(OnChangeWave);
        Service.Get<UIManager>().GetUI<IngameBottomUIController>().isSortMode.AddListener(OnOpenSortPhase);
    }

    private void OnChangeGameStateType(GameState state)
    {
        if (state == GameState.Sort)
        {
        }
    }

    private void OnOpenSortPhase(bool value)
    {
        if (value)
        {
            //OpenNext
            MakeGlow(Service.Get<UIManager>().GetUI<IngameBottomUIController>().GetSlots[0].gameObject);
        }
    }

    private void OnChangeWave(int value)
    {
        currentWave = value;
    }

    private GameObject MakeGlow(GameObject raw)
    {
        GameObject clone = Instantiate(raw, transform, false);
        RectTransform rawRt = (RectTransform)raw.transform;
        RectTransform rt = (RectTransform)clone.transform;
        rt.position = rawRt.position;
        rt.sizeDelta = rawRt.sizeDelta;
        rt.localScale = rawRt.localScale;
        GlowUIList.Add(clone);
        return clone;
    }

    private void DeleteAllGlow()
    {
        foreach (var temp in GlowUIList)
        {
            Destroy(temp);
        }
    }

    private Tween currentTween;

    private void ShowArrow(Vector2 startPos, Vector2 endPos)
    {
        ArrowUI.gameObject.SetActive(true);
        currentTween = ArrowUI.GetComponent<Image>().DOFade(0, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    private void HideArrow()
    {
        currentTween.Kill();
        ArrowUI.gameObject.SetActive(false);
    }
}