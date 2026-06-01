using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Transform[] _subslots; 
    public Transform[] SubSlots => _subslots;

    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider skillBar;
    
    
    

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
        hpBar.value = value;
    }    
    /// <summary>
    /// 스킬 구독
    /// </summary>
    /// <param name="value">0~1값만 들어와야 합니다. <br />(쿨타임 / 남은 시간)을 float형으로 계산하여 입력해주시기 바랍니다. </param>
    public void SetSkillBar(float value)
    {
        hpBar.value = value;
    }
}