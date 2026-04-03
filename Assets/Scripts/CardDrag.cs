using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private Camera uiCamera;

    private Vector2 dragOffset;

    private HandController handController;

    public bool IsDragging { get; private set; }

    public Vector3 TargetLocalPosition { get; private set; }
    public Quaternion TargetLocalRotation { get; private set; }

    private int logicalIndex;
    private int totalCards;

    public void Initialize(HandController controller)
    {
        handController = controller;
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = rootCanvas.worldCamera;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = rootCanvas.worldCamera;
    }

    public void SetLogicalIndex(int index, int count)
    {
        logicalIndex = index;
        totalCards = count;
    }

    public void SetTarget(Vector3 localPos, Quaternion localRot)
    {
        TargetLocalPosition = localPos;
        TargetLocalRotation = localRot;
    }

    public void AnimateToTarget(float layoutLerpSpeed, float returnSpeed, float dragScale)
    {
        if (IsDragging)
        {
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                Vector3.one * dragScale,
                Time.deltaTime * 14f
            );
            return;
        }

        rectTransform.localPosition = Vector3.Lerp(
            rectTransform.localPosition,
            TargetLocalPosition,
            Time.deltaTime * returnSpeed
        );

        rectTransform.localRotation = Quaternion.Slerp(
            rectTransform.localRotation,
            TargetLocalRotation,
            Time.deltaTime * layoutLerpSpeed
        );

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            Vector3.one,
            Time.deltaTime * 14f
        );
    }

    public void SnapInstant()
    {
        rectTransform.localPosition = TargetLocalPosition;
        rectTransform.localRotation = TargetLocalRotation;
        rectTransform.localScale = Vector3.one;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDragging = true;
        handController.BringToFront(this);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            uiCamera,
            out Vector2 localMousePos
        );

        dragOffset = (Vector2)rectTransform.localPosition - localMousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            uiCamera,
            out Vector2 localMousePos
        );

        rectTransform.localPosition = localMousePos + dragOffset;
        rectTransform.localRotation = Quaternion.identity;

        handController.ReorderCardByPosition(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
    }
}