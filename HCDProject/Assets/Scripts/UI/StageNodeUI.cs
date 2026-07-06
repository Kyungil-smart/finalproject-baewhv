using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StageNodeUI : MonoBehaviour
{
    private Image nodeImage;
    private Button nodeButton;
    [SerializeField] private TextMeshProUGUI nodeText;


    private void Awake()
    {
        nodeImage = GetComponent<Image>();
        nodeButton = GetComponent<Button>();
    }

    public StageNodeUI SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
        return this;
    }

    public StageNodeUI SetColor(Color color)
    {
        nodeImage.color = color;
        return this;
    }

    public StageNodeUI SetButtonAction(UnityAction action)
    {
        nodeButton.onClick.RemoveAllListeners();
        nodeButton.onClick.AddListener(action);
        return this;
    }

    public StageNodeUI SetText(int chapter, int stage)
    {
        nodeText.text = $"stage {chapter}-{stage}";
        return this;
    }

    public StageNodeUI SetImage(Sprite img)
    {
        nodeImage.sprite = img;
        return this;
    }
}
