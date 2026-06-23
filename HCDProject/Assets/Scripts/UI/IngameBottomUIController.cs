using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    public GameObject GetComboView => ComboView.gameObject;
    [SerializeField] private StartBattleButtonUI StartBattleButton;
    public GameObject GetStartButton => StartBattleButton.gameObject;


    [SerializeField] private GameObject characterSlotPrefab;
    [SerializeField] private RectTransform characterSlot;
    private List<CharacterSlotUI> characterSlots = new();

    private readonly Vector2 battlePhaseSlotRect = new Vector2(245, 361);
    private readonly Vector2 sortPhaseSlotRect = new Vector2(245, 990);
    private readonly Vector2 battlePhasePortraitRect = new Vector2(-12, -56.0f);
    private readonly Vector2 sortPhasePortraitRect = new Vector2(-12, -166.4f);
    public CharacterSlotUI[] GetSlots => characterSlots.ToArray();

    [SerializeField] private StoneRail upperRail;
    public StoneRail GetUpperRail => upperRail;
    [SerializeField] private StoneRail lowerRail;
    public StoneRail GetLowerRail => lowerRail;

    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI leftSortCountText;

    [SerializeField] private TextMeshProUGUI gameSpeedText;

    public ObserveValue<bool> isSortMode = new();

    private void Start()
    {
        comboText.gameObject.SetActive(false);
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
            slot.ChangeMode(false);
        }

        isSortMode.Value = false;
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
            slot.ChangeMode(true);
        }

        isSortMode.Value = true;
        StartBattleButton.SetSortStart();
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

            comboText.gameObject.SetActive(true);
        }
    }

    public void SetLeftSortCountText(float value)
    {
        if (value < 0)
        {
            leftSortCountText.text = "남은 시간 : 0 s";
            StartBattleButton.SetSortDone();
        }
        else
        {
            leftSortCountText.text = $"남은 시간 : {value:F1} s";
        }
    }

    public void SetExp(int curr, int max)
    {
        expText.text = $"{(curr > max ? max : curr)} / {max}";
        expGauge.value = Mathf.Clamp01((float)curr / max);
    }

    public void SetLevelText(int value)
    {
        levelText.text = $"Lv : {value}";
    }

    public void OnSpeedButtonClick()
    {
        gameSpeedText.text = $"X{Service.Get<GameManager>()?.ChangeSpeed()}";
    }

    public CharacterSlotUI AddCharacter(CharacterRawData data, BaseCharacter character, int order = -1)
    {
        var slot = Instantiate(characterSlotPrefab, characterSlot).GetComponent<CharacterSlotUI>();
        if (order != -1)
            slot.transform.SetSiblingIndex(order);
        string address;
        switch (data.CHARACTER_ID) //TODO : HardCoding
        {
            case "3000":
                address = "Player/Serah[Serah_BattlePortrait]";
                break;
            case "3001":
                address = "Player/Noah[Noah_BattlePortrait]";
                break;
            case "3002":
                address = "Player/Alice[Alice_BattlePortrait]";
                break;
            default:
                address = "Player/Spayin[Spayin_BattlePortrait]";
                break;
        }

        Addressables.LoadAssetAsync<Sprite>(address).Completed += handle =>
        {
            if (!slot) return;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                slot.InitPortrait(handle.Result);
            }
        };
        slot.InitSlot(character);
        characterSlots.Add(slot);
        return slot;
    }
}