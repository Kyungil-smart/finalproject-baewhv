using UnityEngine;
using UnityEngine.EventSystems;

public class SlotBase : MonoBehaviour, IDropHandler
{
    public Transform[] subSlots = new Transform[3];

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragAndDropItem draggedObject = eventData.pointerDrag.GetComponent<DragAndDropItem>();

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