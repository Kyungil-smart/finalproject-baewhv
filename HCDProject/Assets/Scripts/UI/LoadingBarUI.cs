using DG.Tweening;
using UnityEngine;

public class LoadingBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform LoadingImage;

    public void Init()
    {
        Service.Get<LoadManager>().OnLoading += LoadingUi;
        Service.Get<LoadManager>().OnLoadingComplete += CloseLoadingUi;
    }

    private void OnEnable()
    {
        //LoadingImage.DORotate(Vector3.zero, 0.0f);
        LoadingImage.DORotate(new Vector3(0, 0, -360), 1.0f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

    private void OnDisable()
    {
        LoadingImage.DOKill();
    }

    private void OnDestroy()
    {
        var loadManager = Service.Get<LoadManager>();
        if (loadManager != null)
        {
            //sceneController.OnLoading -= LoadingUi;
            loadManager.OnLoadingComplete -= CloseLoadingUi;
        }
    }

    private void LoadingUi(float progress)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        
    }

    private void CloseLoadingUi()
    {
        gameObject.SetActive(false);
    }
}
