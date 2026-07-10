using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StoryButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;

    public void SetButton(string inText, UnityAction action)
    {
        text.text = inText;
        button.onClick.AddListener(action);
    }

}
