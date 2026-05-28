using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    [SerializeField] private Slider LoadingGauge;
    [SerializeField] private TextMeshProUGUI LoadingText;
    [SerializeField] private Button NextButton;

    private ObserveValue<bool> _isLoaded = new ();
    private ObserveValue<float> _loadingValue = new ();
    private float _loadingTime = 2.0f;

    private void Awake()
    {
        _loadingValue.Value = 0.0f;
        _isLoaded.Value = false;
        _loadingValue.AddListener(SetGauge);
        _isLoaded.AddListener(SetNextButton);
        if(NextButton.gameObject.activeSelf)
            NextButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(_loadingValue.Value + Time.deltaTime< _loadingTime)
            _loadingValue.Value += Time.deltaTime;
        else
        {
            _loadingValue.Value = _loadingTime;
            _isLoaded.Value = true;
        }
    }

    private void SetGauge(float value)
    {
        if (LoadingGauge)
        {
            LoadingGauge.value = value / _loadingTime;
        }

        if (LoadingText)
        {
            LoadingText.text = $"{value/_loadingTime:P0}";
        }
    }

    private void SetNextButton(bool value)
    {
        if (value)
        {
            NextButton.gameObject.SetActive(true);
            LoadingGauge.gameObject.SetActive(false);
        }
    }

    public void OnNextScene()
    {
        Service.Get<SceneController>()?.ChangeModeScene();
    }
}
