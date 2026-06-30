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
            positive?.Invoke();
            gameObject.SetActive(false);
        });
    }
    public void SetTwoButtonPopup(string _title, string _desc, UnityAction positive, UnityAction negative)
    {
        gameObject.SetActive(true);
        negativeButton.gameObject.SetActive(false);
        title.text = _title;
        desc.text = _desc;
        positiveButton.onClick.AddListener(() =>
        {
            positive?.Invoke();
            gameObject.SetActive(false);
        });
        negativeButton.onClick.AddListener(() =>
        {
            negative.Invoke();
            gameObject.SetActive(false);
        });
    }
}