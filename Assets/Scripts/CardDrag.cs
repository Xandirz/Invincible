using UnityEngine;
using UnityEngine.EventSystems;

public enum CardZone
{
    Hand,
    PlayOrder
}

[RequireComponent(typeof(RectTransform))]
public class CardDrag : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private Camera uiCamera;
    private Vector2 dragOffset;

    private HandController handController;
    private CardsToPlayOrder playOrderZone;

    public bool IsDragging { get; private set; }
    public bool IsHovering { get; private set; }

    public Vector3 TargetLocalPosition { get; private set; }
    public Quaternion TargetLocalRotation { get; private set; }

    public Vector3 PlayZoneTargetLocalPosition { get; private set; }
    public Quaternion PlayZoneTargetLocalRotation { get; private set; }

    public CardVisual Visual { get; private set; }

    public CardZone CurrentZone { get; private set; } = CardZone.Hand;

    public void Initialize(HandController controller)
    {
        handController = controller;
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = rootCanvas.worldCamera;

        Visual = GetComponentInChildren<CardVisual>();
        if (Visual != null)
            Visual.Initialize(this);

        playOrderZone = FindObjectOfType<CardsToPlayOrder>();
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = rootCanvas.worldCamera;

        Visual = GetComponentInChildren<CardVisual>();
        if (Visual != null)
            Visual.Initialize(this);

        playOrderZone = FindObjectOfType<CardsToPlayOrder>();
    }

    public void SetLogicalIndex(int index, int count)
    {
    }

    public void SetPlayOrderIndex(int index)
    {
    }

    public void SetCurrentZone(CardZone zone)
    {
        CurrentZone = zone;
    }

    public void SetTarget(Vector3 localPos, Quaternion localRot)
    {
        TargetLocalPosition = localPos;
        TargetLocalRotation = localRot;
    }

    public void SetPlayZoneTarget(Vector3 localPos, Quaternion localRot)
    {
        PlayZoneTargetLocalPosition = localPos;
        PlayZoneTargetLocalRotation = localRot;
    }

    public void AnimateToTarget(float layoutLerpSpeed, float returnSpeed, float dragScale)
    {
        if (IsDragging || CurrentZone != CardZone.Hand)
            return;

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
    }

    public void AnimatePlayZone(float speed)
    {
        if (IsDragging || CurrentZone != CardZone.PlayOrder)
            return;

        rectTransform.localPosition = Vector3.Lerp(
            rectTransform.localPosition,
            PlayZoneTargetLocalPosition,
            Time.deltaTime * speed
        );

        rectTransform.localRotation = Quaternion.Slerp(
            rectTransform.localRotation,
            PlayZoneTargetLocalRotation,
            Time.deltaTime * speed
        );
    }

    public void SnapInstant()
    {
        if (CurrentZone == CardZone.Hand)
        {
            rectTransform.localPosition = TargetLocalPosition;
            rectTransform.localRotation = TargetLocalRotation;
        }
        else
        {
            rectTransform.localPosition = PlayZoneTargetLocalPosition;
            rectTransform.localRotation = PlayZoneTargetLocalRotation;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDragging = true;

        if (CurrentZone == CardZone.Hand)
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

        if (CurrentZone == CardZone.Hand)
        {
            handController.ReorderCardByPosition(this);
        }
        else if (CurrentZone == CardZone.PlayOrder && playOrderZone != null)
        {
            playOrderZone.ReorderCardByPosition(this);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;

        bool droppedIntoPlayZone =
            playOrderZone != null &&
            playOrderZone.IsInsideZone(eventData.position, uiCamera);

        if (droppedIntoPlayZone)
        {
            if (CurrentZone == CardZone.Hand)
            {
                handController.RemoveCardFromHand(this);
                transform.SetParent(playOrderZone.transform);
                playOrderZone.AddCard(this);
            }
            else
            {
                playOrderZone.ReorderCardByPosition(this);
            }
        }
        else
        {
            if (CurrentZone == CardZone.PlayOrder)
            {
                playOrderZone.RemoveCard(this);
                transform.SetParent(handController.hand);
                handController.AddExistingCardToHand(this);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHovering = false;
    }
}