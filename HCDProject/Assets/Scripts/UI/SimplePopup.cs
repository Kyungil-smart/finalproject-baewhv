using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimplePopup : MonoBehaviour
{
    [SerializeField] private Button positiveButton;
    [SerializeField] private Button negativeButton;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI desc;

    public void SetOneButtonPopup(string _title, string _desc, UnityAction positive)
    {
        gameObject.SetActive(true);
        negativeButton.gameObject.SetActive(false);
        title.text = _title;
        desc.text = _desc;
        positiveButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            positive?.Invoke();
        });
    }
    public void SetTwoButtonPopup(string _title, string _desc, UnityAction positive, UnityAction negative)
    {
        gameObject.SetActive(true);
        negativeButton.gameObject.SetActive(true);
        title.text = _title;
        desc.text = _desc;
        positiveButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            positive?.Invoke();
        });
        negativeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            negative.Invoke();
        });
    }
}