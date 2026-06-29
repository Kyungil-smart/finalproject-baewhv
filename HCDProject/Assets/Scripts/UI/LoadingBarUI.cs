using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBarUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private Slider loadingBarSlider;
    [SerializeField] private TextMeshProUGUI loadingBarText;

    public void Init()
    {
        Service.Get<SceneController>().OnLoading += LoadingUi;
        Service.Get<SceneController>().OnLoadingComplete += CloseLoadingUi;
    }

    private void OnDestroy()
    {
        var sceneController = Service.Get<SceneController>();
        if (sceneController != null)
        {
            sceneController.OnLoading -= LoadingUi;
            sceneController.OnLoadingComplete -= CloseLoadingUi;
        }
    }

    private void LoadingUi(float progress)
    {
        if (!loadingBar.activeSelf) loadingBar.SetActive(true);
        
        loadingBarSlider.value = progress;
        
        float percent = progress * 100f;
        
        if (percent >= 99.9f) loadingBarText.text = "100%";
        else                  loadingBarText.text = $"{(percent):F2}%";
    }

    private void CloseLoadingUi()
    {
        loadingBar.SetActive(false);
    }
}
