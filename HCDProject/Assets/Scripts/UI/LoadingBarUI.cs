using DG.Tweening;
using UnityEngine;

public class LoadingBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform LoadingImage;

    public void Init()
    {
        Service.Get<SceneController>().OnLoading += LoadingUi;
        Service.Get<SceneController>().OnLoadingComplete += CloseLoadingUi;
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
        var sceneController = Service.Get<SceneController>();
        if (sceneController != null)
        {
            //sceneController.OnLoading -= LoadingUi;
            sceneController.OnLoadingComplete -= CloseLoadingUi;
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
