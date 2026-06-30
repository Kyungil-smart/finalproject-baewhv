using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private Transform[] _subslots;
    public Transform[] SubSlots => _subslots;

    [SerializeField] private Slider hpBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Slider skillBar;
    [SerializeField] private RectTransform borderRect;
    [SerializeField] private GameObject SkillArea;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image DeathCountImage;
    [SerializeField] private TextMeshProUGUI deathTimerText;
    public RectTransform GetBorderRect => borderRect;

    private BaseCharacter _character;


    public void InitSlot(BaseCharacter character)
    {
        //캐릭터 초상화 설정
        _character = character;
    }
    public void InitPortrait(Sprite image) =>portraitImage.sprite = image;


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

    public void ChangeMode(bool isSort)
    {
        SkillArea.SetActive(!isSort);
        foreach (var slot in _subslots)
        {
            slot.gameObject.SetActive(isSort);
        }
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
}