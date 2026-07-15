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
    private readonly Vector2 battleModeCharactersSlot = new Vector2(1057, 575);
    private readonly Vector2 sortModeCharactersSlot = new Vector2(1057, 1095);

    [SerializeField] private StartBattleButtonUI StartBattleButton;
    public GameObject GetStartButton => StartBattleButton.gameObject;


    [SerializeField] private GameObject characterSlotPrefab;
    [SerializeField] private RectTransform characterSlot;
    private List<CharacterSlotUI> characterSlots = new();


    //687
    public CharacterSlotUI[] GetSlots => characterSlots.ToArray();

    [SerializeField] private StoneRail upperRail;
    public StoneRail GetUpperRail => upperRail;
    [SerializeField] private StoneRail lowerRail;
    public StoneRail GetLowerRail => lowerRail;

    [SerializeField] private TextMeshProUGUI comboText;
    public GameObject GetComboText => comboText.gameObject;
    [SerializeField] private TextMeshProUGUI leftTimeText;
    [SerializeField] private TextMeshProUGUI addTimeText;
    [SerializeField] private Slider LeftTimeGauge;

    [SerializeField] private TextMeshProUGUI gameSpeedText;

    public ObserveValue<bool> isSortMode = new();

    [SerializeField] private GameObject[] SkillEffect;

    private void Start()
    {
        comboText.gameObject.SetActive(false);
    }

    public void OnEndSort()
    {
        if (Service.Get<TutorialManager>() && Service.Get<TutorialManager>().pauseWave)
        {
            SetBattlePhase();
        }
        else
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

    public void SetWallHP(int current, int max)
    {
        wallHp.value = Mathf.Clamp01((float)current / max);
        if (current < 0) current = 0;
        wallHpText.text = $"{current} / {max}";
    }

    public void HideSkill()
    {
        foreach (var slot in characterSlots)
        {
            slot.HideSkill();
        }
    }

    public void ShowSkill()
    {
        foreach (var slot in characterSlots)
        {
            slot.ShowSkill();
        }
    }

    public void SetBattlePhase()
    {
        sortTopObject.SetActive(false);
        sortBottomObject.SetActive(false);
        charactersSlotUI.anchoredPosition = new Vector2(charactersSlotUI.anchoredPosition.x, -786.5f);
        //charactersSlotUI.DOAnchorPosY(-786.5f, 0).SetUpdate(true);
        charactersSlotUI.sizeDelta = battleModeCharactersSlot;
        //charactersSlotUI.DOSizeDelta(battleModeCharactersSlot, 0).SetUpdate(true);
        foreach (CharacterSlotUI slot in characterSlots)
        {
            slot.ChangeMode(false);
        }

        if(isSortMode.Value)
            isSortMode.Value = false;
        Service.Get<TimeManager>().LoadTimeScale();
    }

    public void SetSortPhase()
    {
        sortTopObject.SetActive(true);
        sortBottomObject.SetActive(true);
        charactersSlotUI.anchoredPosition = new Vector2(charactersSlotUI.anchoredPosition.x, 262.0f);
        //charactersSlotUI.DOAnchorPosY(262.0f, 0).SetUpdate(true);
        charactersSlotUI.sizeDelta = sortModeCharactersSlot;
        //charactersSlotUI.DOSizeDelta(sortModeCharactersSlot, 0).SetUpdate(true);
        foreach (CharacterSlotUI slot in characterSlots)
        {
            slot.ChangeMode(true);
        }

        if(!isSortMode.Value)
            isSortMode.Value = true;
        StartBattleButton.SetSortStart();
        Service.Get<TimeManager>().SaveTimeScale();
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

    public void SetLeftSortCountText(float value, float max)
    {
        if (value < 0)
        {
            leftTimeText.text = "0 s";
            LeftTimeGauge.value = 0;
            StartBattleButton.SetSortDone();
        }
        else
        {
            LeftTimeGauge.value = Mathf.Clamp01(value / max);
            leftTimeText.text = $"{value:F1} s";
        }
    }

    public void SetAddTimeText(float value)
    {
        addTimeText.color = new Color(0, 1, 0, 1);
        RectTransform rt = (RectTransform)addTimeText.transform;
        addTimeText.text = $"+ {value:F1}";
        Sequence sq = DOTween.Sequence();
        sq.Join(rt.DOAnchorPosY(20.0f, 1.0f).From());
        sq.Join(addTimeText.DOFade(0.0f, 1.0f));
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
        switch (data.CHARACTER_ID) //TODO : HardCoding
        {
            case "3000":
                slot.SkillEffect = Instantiate(SkillEffect[0], slot.SkillArea.transform);
                break;
            case "3001":
                slot.SkillEffect = Instantiate(SkillEffect[1], slot.SkillArea.transform);
                break;
            case "3002":
                slot.SkillEffect = Instantiate(SkillEffect[2], slot.SkillArea.transform);
                break;
            case "3003":
                slot.SkillEffect = Instantiate(SkillEffect[3], slot.SkillArea.transform);
                break;
        }

        slot.SkillEffect?.SetActive(false);

        Sprite sp = Service.Get<ResourcesManager>().GetSprite(data.CHARACTER_IMG, handle =>
        {
            slot.InitPortrait(handle);
        });
        if(sp)
            slot.InitPortrait(sp);
        slot.InitSlot(character);
        characterSlots.Add(slot);
        return slot;
    }
    public void OnOpenRewardView()
    {
        Service.Get<UIManager>()?.RewardViewPopupUI.OpenUI(true);
    }
}