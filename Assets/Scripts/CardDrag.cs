using UnityEngine;
using UnityEngine.EventSystems;

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
    private CardGenerator[] generators;

    public bool IsDragging { get; private set; }
    public bool IsHovering { get; private set; }

    public Vector3 TargetLocalPosition { get; private set; }
    public Quaternion TargetLocalRotation { get; private set; }

    public Vector3 GeneratorTargetLocalPosition { get; private set; }
    public Quaternion GeneratorTargetLocalRotation { get; private set; }

    public CardVisual Visual { get; private set; }

    public CardZone CurrentZone { get; private set; } = CardZone.Hand;
    public CardGenerator CurrentGenerator { get; private set; }

    public void Initialize(HandController controller)
    {
        handController = controller;
        CacheReferences();
    }

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = rootCanvas.worldCamera;

        Visual = GetComponentInChildren<CardVisual>();
        if (Visual != null)
            Visual.Initialize(this);

        generators = FindObjectsOfType<CardGenerator>();
    }

    public void RefreshGenerators()
    {
        generators = FindObjectsOfType<CardGenerator>();
    }

    public void SetLogicalIndex(int index, int count)
    {
    }

    public void SetGeneratorIndex(int index)
    {
    }

    public void SetCurrentZone(CardZone zone)
    {
        CurrentZone = zone;
    }

    public void SetCurrentGenerator(CardGenerator generator)
    {
        CurrentGenerator = generator;
    }

    public void SetTarget(Vector3 localPos, Quaternion localRot)
    {
        TargetLocalPosition = localPos;
        TargetLocalRotation = localRot;
    }

    public void SetGeneratorTarget(Vector3 localPos, Quaternion localRot)
    {
        GeneratorTargetLocalPosition = localPos;
        GeneratorTargetLocalRotation = localRot;
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

    public void AnimateGeneratorZone(float speed)
    {
        if (IsDragging || CurrentZone != CardZone.Generator)
            return;

        rectTransform.localPosition = Vector3.Lerp(
            rectTransform.localPosition,
            GeneratorTargetLocalPosition,
            Time.deltaTime * speed
        );

        rectTransform.localRotation = Quaternion.Slerp(
            rectTransform.localRotation,
            GeneratorTargetLocalRotation,
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
            rectTransform.localPosition = GeneratorTargetLocalPosition;
            rectTransform.localRotation = GeneratorTargetLocalRotation;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GetComponent<FlyingCardToGenerator>() != null)
            return;

        IsDragging = true;

        if (CurrentZone == CardZone.Hand && handController != null)
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
        else if (CurrentZone == CardZone.Generator && CurrentGenerator != null)
        {
            CurrentGenerator.ReorderCardByPosition(this);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;

        CardGenerator targetGenerator = FindTargetGenerator(eventData.position);

        if (targetGenerator != null)
        {
            MoveToGenerator(targetGenerator);
            return;
        }

        MoveToHand();
    }

    private CardGenerator FindTargetGenerator(Vector2 screenPosition)
    {
        if (generators == null || generators.Length == 0)
            return null;

        for (int i = 0; i < generators.Length; i++)
        {
            CardGenerator generator = generators[i];

            if (generator == null)
                continue;

            if (!generator.IsInsideZone(screenPosition, uiCamera))
                continue;

            if (!generator.CanAcceptCard(this))
                continue;

            return generator;
        }

        return null;
    }

    private void MoveToHand()
    {
        RemoveFromCurrentZone();
        transform.SetParent(handController.hand);
        handController.AddExistingCardToHand(this);
        CurrentGenerator = null;
    }

    private void MoveToGenerator(CardGenerator generator)
    {
        if (generator == null)
            return;

        if (CurrentZone == CardZone.Generator && CurrentGenerator == generator)
        {
            generator.ReorderCardByPosition(this);
            return;
        }

        RemoveFromCurrentZone();

        transform.SetParent(generator.transform);
        generator.AddCard(this);

        CurrentGenerator = generator;
        SetCurrentZone(CardZone.Generator);
    }

    private void RemoveFromCurrentZone()
    {
        switch (CurrentZone)
        {
            case CardZone.Hand:
                handController.RemoveCardFromHand(this);
                break;

            case CardZone.Generator:
                if (CurrentGenerator != null)
                    CurrentGenerator.RemoveCard(this);
                break;
        }
    }

    public CardData GetCardData()
    {
        return GetComponent<CardData>();
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