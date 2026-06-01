using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;

    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    private float originalLocalZ;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalLocalZ = transform.localPosition.z;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 mouseScreenPos = Vector2.zero;
        if (Mouse.current != null)
        {
            mouseScreenPos = Mouse.current.position.ReadValue();
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            mouseScreenPos, 
            canvas.worldCamera,
            out Vector2 localPoint
        );

        rectTransform.anchoredPosition = localPoint;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, originalLocalZ);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (transform.parent == originalParent)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, originalLocalZ);
        }
    }
}