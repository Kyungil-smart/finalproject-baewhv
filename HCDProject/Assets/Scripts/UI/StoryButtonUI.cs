using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StoryButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image background;
    [SerializeField] private Image mainSprite;

    public void SetButton(string inText, UnityAction action)
    {
        text.text = inText;
        button.onClick.AddListener(action);
    }

    public void SetImage(string bg, string main)
    {
        if (background)
            background.sprite = Service.Get<ResourcesManager>().GetSprite(bg, handle =>
            {
                background.sprite = handle;
            });
        if (mainSprite)
            mainSprite.sprite = Service.Get<ResourcesManager>().GetSprite(main, handle =>
            {
                mainSprite.sprite = handle;
            });
    }

}
