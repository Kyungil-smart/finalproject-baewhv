using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : BaseUIController<TitleUIController>
{
    [SerializeField] private Slider LoadingGauge;
    [SerializeField] private TextMeshProUGUI LoadingText;
    [SerializeField] private Button NextButton;
    [SerializeField] private RectTransform NextText;
    [SerializeField] private TextMeshProUGUI VersionText;

    private ObserveValue<bool> _isLoaded = new ();
    private ObserveValue<float> _loadingValue = new ();
    private float _loadingTime = 2.0f;
    private bool isStartLoading;

    private Sequence NextSceneTweener;

    private void Awake()
    {
        _loadingValue.Value = 0.0f;
        _isLoaded.Value = false;
        _loadingValue.AddListener(SetGauge);
        _isLoaded.AddListener(SetNextButton);
        if(NextButton.gameObject.activeSelf)
            NextButton.gameObject.SetActive(false);
        SetVersionText();
    }

    private void Update()
    {
        //if (!isStartLoading) return;
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
        if (NextSceneTweener != null && NextSceneTweener.active) return;
        NextText.gameObject.SetActive(true);
        NextSceneTweener = DOTween.Sequence();
        NextSceneTweener.Join(NextText.GetComponent<TextMeshProUGUI>().DOFade(1.0f,1.0f));
        NextSceneTweener.Join(NextText.DOAnchorPosX(743.0f, 1.0f).From());
        if (Service.Get<UIManager>())
        {
            Image fader = Service.Get<UIManager>().GetFader;
            if (fader)
            {
                fader.color = new Color(1, 1, 1, 0);
                NextSceneTweener.Append(Service.Get<UIManager>().GetFader.DOFade(1.0f, 1.0f));
            }
        }

        NextSceneTweener.OnComplete(() =>
        {
            Service.Get<SceneController>()?.ChangeScene(SceneType.ModeSelect);
            Service.Get<UIManager>().RemoveFader();
        });
        
    }

    public void OnOpenSettingUI()
    {
        Service.Get<UIManager>()?.OpenOption(ESettingPopupType.OnlyLanguage);
        
    }

    private void SetVersionText()
    {
        VersionText.text = $"Version : {Application.version}";
    }
}
