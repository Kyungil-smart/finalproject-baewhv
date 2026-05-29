using UnityEngine;
using UnityEngine.EventSystems;

public class SlotBase : MonoBehaviour, IDropHandler
{
    public Transform[] SubSlots { get; set; } = new Transform[3];

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