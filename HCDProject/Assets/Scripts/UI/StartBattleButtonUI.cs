using UnityEngine;
using UnityEngine.UI;

public class StartBattleButtonUI : MonoBehaviour
{
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color sortDoneColor;

    public void SetSortStart()
    {
        buttonImage.color = defaultColor;
    }

    public void SetSortDone()
    {
        buttonImage.color = sortDoneColor;
    }
}
