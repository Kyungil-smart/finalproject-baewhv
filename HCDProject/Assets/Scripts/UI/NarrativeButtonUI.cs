using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NarrativeButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;

    public void SetButton(string inText, UnityAction action)
    {
        text.text = inText;
        button.onClick.AddListener(action);
    }
}
