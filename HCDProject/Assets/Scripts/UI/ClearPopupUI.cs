using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ClearPopupUI : MonoBehaviour
{
    [SerializeField] private Button positiveButton;
    [FormerlySerializedAs("isOpenRewardPopup")] public ObserveValue<bool> isOpenClearPopup = new();
    public void SetNextButton(bool isShow)
    {
        positiveButton.gameObject.SetActive(isShow);
        isOpenClearPopup.Value = true;
    }

    public void AddListener(UnityAction<bool> action)
    {
        isOpenClearPopup.AddListener(action);
    }
    public void RemoveListener(UnityAction<bool> action)
    {
        isOpenClearPopup.RemoveListener(action);
    }
}