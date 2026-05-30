using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Transform[] _subslots; 
    public Transform[] SubSlots => _subslots;

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragAndDrop draggedObject = eventData.pointerDrag.GetComponent<DragAndDrop>();

            if (draggedObject != null)
            {
                if (SortManager.Instance != null)
                {
                    SortManager.Instance.ObjectDrop(this, draggedObject);
                }
            }
        }
    }
}