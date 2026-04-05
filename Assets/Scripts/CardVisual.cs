﻿using UnityEngine;

public class CardVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private RectTransform tiltRoot;
    [SerializeField] private RectTransform shadow;
    [SerializeField] private Canvas visualCanvas;

    [Header("Follow Lag")]
    [SerializeField] private float followSpeed = 18f;
    [SerializeField] private float rotationFollowSpeed = 18f;

    [Header("Hover Scale")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.12f;
    [SerializeField] private float dragScale = 1.08f;
    [SerializeField] private float scaleSpeed = 14f;

    [Header("Tilt")]
    [SerializeField] private float tiltAmountX = 10f;
    [SerializeField] private float tiltAmountY = 10f;
    [SerializeField] private float zTiltFromMove = 12f;
    [SerializeField] private float tiltSmooth = 12f;
    [SerializeField] private float maxMouseDistance = 120f;

    [Header("Shadow")]
    [SerializeField] private float shadowFollowSpeed = 14f;
    [SerializeField] private float shadowOffsetAmount = 12f;
    [SerializeField] private float shadowDragExtraOffset = 10f;
    [SerializeField] private float shadowScaleMultiplier = 0.95f;

    [Header("Swap Punch")]
    [SerializeField] private float punchAngle = 12f;
    [SerializeField] private float punchReturnSpeed = 12f;
    [SerializeField] private float punchDamping = 10f;

    private CardDrag card;
    private RectTransform cardRect;

    private Vector3 lastFramePosition;
    private float moveBasedZ;
    private float swapPunchZ;
    private float swapPunchVelocity;

    private bool initialized;

    public void Initialize(CardDrag targetCard)
    {
        card = targetCard;
        cardRect = targetCard != null ? targetCard.GetComponent<RectTransform>() : null;

        if (visualRoot == null)
            visualRoot = transform as RectTransform;

        if (tiltRoot == null)
            tiltRoot = transform as RectTransform;

        if (visualCanvas == null)
            visualCanvas = GetComponentInParent<Canvas>();

        initialized = card != null && cardRect != null;

        if (initialized)
        {
            lastFramePosition = SafeVector3(cardRect.position);
        }
        else
        {
            lastFramePosition = Vector3.zero;
        }

        moveBasedZ = 0f;
        swapPunchZ = 0f;
        swapPunchVelocity = 0f;
    }

    private void LateUpdate()
    {
        if (!initialized || card == null || cardRect == null)
            return;

        if (!IsFinite(cardRect.position))
            return;

        FollowCard();
        UpdateScale();
        UpdateTilt();
        UpdateShadow();
        UpdateCanvasSorting();
    }

    private void FollowCard()
    {
        if (cardRect == null)
            return;

        Vector3 targetPosition = SafeVector3(cardRect.position);
        Quaternion targetRotation = SafeQuaternion(cardRect.rotation);

        transform.position = Vector3.Lerp(
            SafeVector3(transform.position),
            targetPosition,
            Time.deltaTime * Mathf.Max(0f, followSpeed)
        );

        transform.rotation = Quaternion.Slerp(
            SafeQuaternion(transform.rotation),
            targetRotation,
            Time.deltaTime * Mathf.Max(0f, rotationFollowSpeed)
        );
    }

    private void UpdateScale()
    {
        if (visualRoot == null)
            return;

        float targetScale = normalScale;

        if (card.IsDragging)
            targetScale = dragScale;
        else if (card.IsHovering)
            targetScale = hoverScale;

        if (!IsFinite(targetScale))
            targetScale = 1f;

        Vector3 currentScale = SafeVector3(visualRoot.localScale, Vector3.one);
        Vector3 desiredScale = Vector3.one * targetScale;

        visualRoot.localScale = Vector3.Lerp(
            currentScale,
            desiredScale,
            Time.deltaTime * Mathf.Max(0f, scaleSpeed)
        );
    }

    private void UpdateTilt()
    {
        if (tiltRoot == null || visualRoot == null)
            return;

        if (maxMouseDistance <= 0.001f)
            return;

        Vector3 currentCardPosition = SafeVector3(cardRect.position, lastFramePosition);
        Vector3 delta = (currentCardPosition - lastFramePosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        delta = SafeVector3(delta, Vector3.zero);
        lastFramePosition = currentCardPosition;

        float normalizedMoveX = Mathf.Clamp(delta.x / 800f, -1f, 1f);
        if (!IsFinite(normalizedMoveX))
            normalizedMoveX = 0f;

        moveBasedZ = Mathf.Lerp(
            SafeFloat(moveBasedZ, 0f),
            -normalizedMoveX * zTiltFromMove,
            Time.deltaTime * Mathf.Max(0f, tiltSmooth)
        );

        Vector2 mouseScreen = Input.mousePosition;
        if (!IsFinite(mouseScreen))
            mouseScreen = Vector2.zero;

        Camera cam = null;
        if (visualCanvas != null && visualCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = visualCanvas.worldCamera;

        Vector2 visualScreen = RectTransformUtility.WorldToScreenPoint(cam, visualRoot.position);
        if (!IsFinite(visualScreen))
            visualScreen = Vector2.zero;

        Vector2 mouseOffset = mouseScreen - visualScreen;
        if (!IsFinite(mouseOffset))
            mouseOffset = Vector2.zero;

        mouseOffset = Vector2.ClampMagnitude(mouseOffset, maxMouseDistance);

        float normalizedX = mouseOffset.x / maxMouseDistance;
        float normalizedY = mouseOffset.y / maxMouseDistance;

        if (!IsFinite(normalizedX))
            normalizedX = 0f;

        if (!IsFinite(normalizedY))
            normalizedY = 0f;

        float targetTiltX = -normalizedY * tiltAmountX;
        float targetTiltY = normalizedX * tiltAmountY;

        if (!IsFinite(targetTiltX))
            targetTiltX = 0f;

        if (!IsFinite(targetTiltY))
            targetTiltY = 0f;

        swapPunchZ = Mathf.SmoothDamp(
            SafeFloat(swapPunchZ, 0f),
            0f,
            ref swapPunchVelocity,
            1f / Mathf.Max(punchDamping, 0.0001f)
        );

        if (!IsFinite(swapPunchZ))
            swapPunchZ = 0f;

        Quaternion targetRotation = Quaternion.Euler(
            targetTiltX,
            targetTiltY,
            SafeFloat(moveBasedZ, 0f) + SafeFloat(swapPunchZ, 0f)
        );

        tiltRoot.localRotation = Quaternion.Slerp(
            SafeQuaternion(tiltRoot.localRotation),
            SafeQuaternion(targetRotation),
            Time.deltaTime * Mathf.Max(0f, tiltSmooth)
        );
    }

    private void UpdateShadow()
    {
        if (shadow == null || visualRoot == null)
            return;

        if (maxMouseDistance <= 0.001f)
            return;

        Vector2 mouseScreen = Input.mousePosition;
        if (!IsFinite(mouseScreen))
            mouseScreen = Vector2.zero;

        Camera cam = null;
        if (visualCanvas != null && visualCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = visualCanvas.worldCamera;

        Vector2 visualScreen = RectTransformUtility.WorldToScreenPoint(cam, visualRoot.position);
        if (!IsFinite(visualScreen))
            visualScreen = Vector2.zero;

        Vector2 mouseOffset = mouseScreen - visualScreen;
        if (!IsFinite(mouseOffset))
            mouseOffset = Vector2.zero;

        mouseOffset = Vector2.ClampMagnitude(mouseOffset, maxMouseDistance);

        Vector2 normalized = mouseOffset / maxMouseDistance;
        if (!IsFinite(normalized))
            normalized = Vector2.zero;

        Vector3 targetShadowPos = new Vector3(
            normalized.x * shadowOffsetAmount,
            normalized.y * shadowOffsetAmount,
            0f
        );

        if (card.IsDragging)
            targetShadowPos += new Vector3(0f, -shadowDragExtraOffset, 0f);

        targetShadowPos = SafeVector3(targetShadowPos, Vector3.zero);

        shadow.localPosition = Vector3.Lerp(
            SafeVector3(shadow.localPosition, Vector3.zero),
            targetShadowPos,
            Time.deltaTime * Mathf.Max(0f, shadowFollowSpeed)
        );

        Vector3 desiredShadowScale = SafeVector3(visualRoot.localScale, Vector3.one) * shadowScaleMultiplier;
        shadow.localScale = Vector3.Lerp(
            SafeVector3(shadow.localScale, Vector3.one),
            desiredShadowScale,
            Time.deltaTime * Mathf.Max(0f, shadowFollowSpeed)
        );
    }

    private void UpdateCanvasSorting()
    {
        if (visualCanvas == null)
            return;

        visualCanvas.overrideSorting = card.IsDragging || card.IsHovering;
        visualCanvas.sortingOrder = card.IsDragging ? 100 : (card.IsHovering ? 50 : 0);
    }

    public void PlaySwapPunch(float direction)
    {
        if (!IsFinite(direction))
            direction = 0f;

        swapPunchZ += punchAngle * direction;

        if (!IsFinite(swapPunchZ))
            swapPunchZ = 0f;
    }

    private bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private bool IsFinite(Vector2 value)
    {
        return IsFinite(value.x) && IsFinite(value.y);
    }

    private bool IsFinite(Vector3 value)
    {
        return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
    }

    private bool IsFinite(Quaternion value)
    {
        return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z) && IsFinite(value.w);
    }

    private float SafeFloat(float value, float fallback = 0f)
    {
        return IsFinite(value) ? value : fallback;
    }

    private Vector3 SafeVector3(Vector3 value, Vector3? fallback = null)
    {
        return IsFinite(value) ? value : (fallback ?? Vector3.zero);
    }

    private Quaternion SafeQuaternion(Quaternion value)
    {
        return IsFinite(value) ? value : Quaternion.identity;
    }
}