using System;
using DG.Tweening;
using UnityEngine;

public class UISimpeTween : MonoBehaviour
{
    private RectTransform rt;

    private void Awake()
    {
        rt = (RectTransform)transform;
    }

    private void OnEnable()
    {
        rt.anchoredPosition = Vector2.zero;
        rt.DOAnchorPosY(10.0f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDisable()
    {
        rt.DOKill();
    }
}
