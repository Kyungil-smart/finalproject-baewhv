using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IngameBottomUIController : BaseUIController<IngameBottomUIController>
{
    [SerializeField] private GameObject sortTopObject;
    [SerializeField] private GameObject sortBottomObject;

    [SerializeField] private Slider wallHp;
    [SerializeField] private TextMeshProUGUI wallHpText;
    [SerializeField] private Slider expGauge;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI levelText;

    //DoTweenControlled
    [SerializeField] private RectTransform charactersSlotUI;
    private readonly Vector2 battleModeCharactersSlot = new Vector2(1057, 405);
    private readonly Vector2 sortModeCharactersSlot = new Vector2(1057, 1034);
    [SerializeField] private RectTransform ComboView;


    [SerializeField] private CharacterSlotUI[] characterSlots;

    private readonly Vector2 battlePhaseSlotRect = new Vector2(245, 361);
    private readonly Vector2 sortPhaseSlotRect = new Vector2(245, 990);
    private readonly Vector2 battlePhasePortraitRect = new Vector2(-12, -56.0f);
    private readonly Vector2 sortPhasePortraitRect = new Vector2(-12, -166.4f);
    public CharacterSlotUI[] GetSlots => characterSlots;

    [SerializeField] private StoneRail upperRail;
    public StoneRail GetUpperRail => upperRail;
    [SerializeField] private StoneRail lowerRail;
    public StoneRail GetLowerRail => lowerRail;

    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI leftSortCountText;

    private void Start()
    {
        comboText.gameObject.SetActive(false);
        characterSlots = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.GetSlots;
    }

    public void OnEndSort()
    {
        Service.Get<SortManager>()?.CheckSortEnd();
    }


    /// <summary>
    /// 0~1값 소수만 사용합니다.
    /// </summary>
    /// <param name="value"></param>
    public void SetWallHP(float value)
    {
        wallHp.value = value;
        wallHpText.text = $"{value:p0}";
    }

    public void SetBattlePhase()
    {
        sortTopObject.SetActive(false);
        sortBottomObject.SetActive(false);
        ComboView.DOAnchorPosY(206.0f, 0);
        charactersSlotUI.DOAnchorPosY(-692.5f, 0);
        charactersSlotUI.DOSizeDelta(battleModeCharactersSlot, 0);
        foreach (CharacterSlotUI slot in characterSlots)
        {
            ((RectTransform)slot.transform).DOSizeDelta(battlePhaseSlotRect, 0);
            slot.GetBorderRect.DOSizeDelta(battlePhasePortraitRect, 0);
        }
    }

    public void SetSortPhase()
    {
        sortTopObject.SetActive(true);
        sortBottomObject.SetActive(true);
        ComboView.DOAnchorPosY(419.0f, 0);
        charactersSlotUI.DOAnchorPosY(205.0f, 0);
        charactersSlotUI.DOSizeDelta(sortModeCharactersSlot, 0);
        foreach (CharacterSlotUI slot in characterSlots)
        {
            ((RectTransform)slot.transform).DOSizeDelta(sortPhaseSlotRect, 0);
            slot.GetBorderRect.DOSizeDelta(sortPhasePortraitRect, 0);
        }
    }

    public void SetComboText(int value)
    {
        if (value == 0) comboText.gameObject.SetActive(false);
        else
        {
            switch (value % 5)
            {
                case 1:
                    comboText.text = $"<color=black>{value}</color> Combo!";
                    break;
                case 2:
                    comboText.text = $"<color=yellow>{value}</color> Combo!";
                    break;
                case 3:
                    comboText.text = $"<color=lightblue>{value}</color> Combo!";
                    break;
                case 4:
                    comboText.text = $"<color=purple>{value}</color> Combo!";
                    break;
                default:
                    comboText.text = $"<color=red>{value}</color> Combo!";
                    break;
            }
        }

        comboText.gameObject.SetActive(false);
    }

    public void SetLeftSortCountText(int value)
    {
        comboText.text = $"남은 소트 횟수 : {value}";
    }

    public void SetExp(int curr, int max)
    {
        expText.text = $"{(curr > max ? max : curr)} / {max}";
        expGauge.value = Mathf.Clamp01((float)curr/max);    
    }

    public void SetLevelText(int value)
    {
        levelText.text = $"Lv : {value}";
    }
}