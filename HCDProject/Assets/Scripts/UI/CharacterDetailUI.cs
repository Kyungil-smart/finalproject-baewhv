using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CharacterDetailUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fullScreen;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private LocalizeStringEvent nameText;
    [SerializeField] private LocalizeStringEvent descText;
    [SerializeField] private GameObject zoomButton;
    [SerializeField] private GameObject zoomOutButton;

    private RectTransform characterRT;
    private Vector2 defaultPos;
    private Sequence seq;

    private void Start()
    {
        characterRT = (RectTransform)characterPortrait.transform;
        defaultPos = characterRT.anchoredPosition;
    }

    public CharacterDetailUI OpenUI()
    {
        gameObject.SetActive(true);
        return this;
    }

    public CharacterDetailUI SetBGColor(Color color)
    {
        backgroundImage.color = color;
        return this;
    }

    public CharacterDetailUI SetPortrait(Sprite image)
    {
        characterPortrait.sprite = image;
        return this;
    }

    public CharacterDetailUI SetText(string charName, string desc)
    {
        nameText.SetEntry(charName);
        descText.SetEntry(desc);
        return this;
    }

    public void OnZoom()
    {
        fullScreen.gameObject.SetActive(true);
        zoomButton.gameObject.SetActive(false);
        nameText.gameObject.SetActive(false);
        descText.gameObject.SetActive(false);
        characterRT.DOAnchorPos(Vector2.zero, 1.0f);
        seq = DOTween.Sequence();
        seq.Join(fullScreen.DOFade(1.0f, 1.0f));
        seq.onComplete += () => { zoomOutButton.gameObject.SetActive(true); };
    }

    public void OffZoom()
    {
        characterRT.DOAnchorPos(defaultPos, 1.0f);
        Sequence seq = DOTween.Sequence();
        seq.Join(fullScreen.DOFade(0.0f, 1.0f));
        seq.onComplete += () =>
        {
            fullScreen.gameObject.SetActive(false);
            zoomOutButton.gameObject.SetActive(false);
            zoomButton.gameObject.SetActive(true);
            nameText.gameObject.SetActive(true);
            descText.gameObject.SetActive(true);
        };
    }
}