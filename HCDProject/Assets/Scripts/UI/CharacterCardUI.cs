using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterCardUI : MonoBehaviour
{
    [SerializeField]private Image background;
    [SerializeField]private Image portrait;
    [SerializeField] private Button button;

    public void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetCard(Color bg, Sprite portraitImage, UnityAction action)
    {
        background.color = bg;
        portrait.sprite = portraitImage;
        button.onClick.AddListener(action);
    }
}
