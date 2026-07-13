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

    private Transform canvasTransform;
    public bool IsGrab { get; private set; } = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas != null) canvasTransform = canvas.transform;

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
        Service.Get<SoundManager>()?.PlaySfxSound("ObjectSelect");

        Service.Get<SortManager>()?.StartTimer();
        IsGrab = true;

        Service.Get<SortManager>()?.PlayerInputLock(true);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        if (canvasTransform != null)
        {
            transform.SetParent(canvasTransform);
        }
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 pointerScreenPos = eventData.position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            pointerScreenPos,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        rectTransform.anchoredPosition = localPoint;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, originalLocalZ);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsGrab = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        var sortManager = Service.Get<SortManager>();
        if (sortManager != null && sortManager.RemainingSorts.Value > 0)
        {
            sortManager.PlayerInputLock(false);
        }

        if (transform.parent == canvasTransform || transform.parent == originalParent)
        {
            ReturnToRail();
        }
    }

    public void ReturnToRail()
    {
        IsGrab = false;

        if (originalParent != null)
        {
            transform.SetParent(originalParent);
        }

        rectTransform.anchoredPosition = originalAnchoredPosition;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, originalLocalZ);
    }
}