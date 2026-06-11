using UnityEngine;
using UnityEngine.UI;

public class ClearPopupUI : MonoBehaviour
{
    [SerializeField] private Button positiveButton;

    public void SetNextButton(bool isShow)
    {
        positiveButton.gameObject.SetActive(isShow);
    }
}