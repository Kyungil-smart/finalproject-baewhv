using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private Transform[] _subslots;
    [SerializeField] private TextMeshProUGUI[] stoneText;
    public Transform[] SubSlots => _subslots;

    [SerializeField] private GameObject PortraitLayout;
    [SerializeField] private GameObject GaugeLayout;
    [SerializeField] private GameObject SocketLayout;
    [SerializeField] private GameObject StoneLayout;

    [SerializeField] private Slider hpBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI deathTimerText;
    [SerializeField] private Slider skillBar;

    [SerializeField] private RectTransform borderRect;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image DeathCountImage;
    public RectTransform GetBorderRect => borderRect;

    [SerializeField] private GameObject SkillArea;

    private BaseCharacter _character;
    
    //private readonly Vector2 battlePhaseSlotRect = new Vector2(245, 361);
    //private readonly Vector2 sortPhaseSlotRect = new Vector2(245, 1000);
    
    private readonly Vector2 battlePhasePortraitRect = new Vector2(-12, 300);
    private readonly Vector2 sortPhasePortraitRect = new Vector2(-12, 700);

    public void InitSlot(BaseCharacter character)
    {
        //캐릭터 초상화 설정
        _character = character;
    }

    public void InitPortrait(Sprite image) => portraitImage.sprite = image;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragAndDrop draggedObject = eventData.pointerDrag.GetComponent<DragAndDrop>();

            if (draggedObject != null)
            {
                Service.Get<SortManager>()?.ObjectDrop(this, draggedObject);
            }
        }
    }

    public void ChangeMode(bool isSort)
    {
        SkillArea.SetActive(!isSort);
        GaugeLayout.SetActive(!isSort);
        StoneLayout.transform.SetSiblingIndex(isSort ? 0 : 3);
        SocketLayout.SetActive(isSort);
        //borderRect.DOSizeDelta(isSort ? sortPhasePortraitRect : battlePhasePortraitRect, 0).SetUpdate(true);
        
    }


    /// <summary>
    /// 체력바 구독
    /// </summary>
    /// <param name="value">0~1값만 들어와야 합니다. <br />(최대체력 / 현재 체력)을 float형으로 계산하여 입력해주시기 바랍니다. </param>
    public void SetHPBar(float value)
    {
        hpBar.value = Mathf.Clamp01(value);
        hpText.text = $"{value:p0}";
    }

    public void SetHPBar(int min, int max)
    {
        hpBar.value = Mathf.Clamp01((float)min / max);
        hpText.text = $"{min} / {max}";
    }

    /// <summary>
    /// 스킬 구독
    /// </summary>
    /// <param name="value">0~1값만 들어와야 합니다. <br />(쿨타임 / 남은 시간)을 float형으로 계산하여 입력해주시기 바랍니다. </param>
    public void SetSkillBar(float value)
    {
        skillBar.value = value;
    }

    public void OnUseSkill()
    {
        _character?.TryUseActiveSkill();
    }


    /// <summary>
    /// 1이 사망 직후, 0이 부활 임박입니다.
    /// </summary>
    /// <param name="value"></param>
    public void SetDeathCount(float current, float max)
    {
        DeathCountImage.fillAmount = Mathf.Clamp01(current / max);
        deathTimerText.text = $"{current:F1} s";
    }

    public void SetAlive(bool isAlive)
    {
        deathTimerText.gameObject.SetActive(!isAlive);
        hpText.gameObject.SetActive(isAlive);
    }


    /// <summary>
    /// sort결과를 변경할 수 있는 함수.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void SetStoneCount(EStoneType type, int value)
    {
        stoneText[(int)type].text = $"X{value}";
    }
}

public enum EStoneType
{
    ATK = 0,
    AS = 1,
    DEF = 2,
    HP = 3
}