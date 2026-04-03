using UnityEngine;

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
        cardRect = targetCard.GetComponent<RectTransform>();

        if (visualRoot == null)
            visualRoot = transform as RectTransform;

        if (tiltRoot == null)
            tiltRoot = transform as RectTransform;

        initialized = true;
        lastFramePosition = cardRect.position;
    }

    private void LateUpdate()
    {
        if (!initialized || card == null || cardRect == null)
            return;

        FollowCard();
        UpdateScale();
        UpdateTilt();
        UpdateShadow();
        UpdateCanvasSorting();
    }

    private void FollowCard()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            cardRect.position,
            Time.deltaTime * followSpeed
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            cardRect.rotation,
            Time.deltaTime * rotationFollowSpeed
        );
    }

    private void UpdateScale()
    {
        float targetScale = normalScale;

        if (card.IsDragging)
            targetScale = dragScale;
        else if (card.IsHovering)
            targetScale = hoverScale;

        visualRoot.localScale = Vector3.Lerp(
            visualRoot.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * scaleSpeed
        );
    }

    private void UpdateTilt()
    {
        Vector3 delta = (cardRect.position - lastFramePosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastFramePosition = cardRect.position;

        float normalizedMoveX = Mathf.Clamp(delta.x / 800f, -1f, 1f);
        moveBasedZ = Mathf.Lerp(moveBasedZ, -normalizedMoveX * zTiltFromMove, Time.deltaTime * tiltSmooth);

        Vector2 mouseScreen = Input.mousePosition;
        Vector2 visualScreen = RectTransformUtility.WorldToScreenPoint(null, visualRoot.position);
        Vector2 mouseOffset = mouseScreen - visualScreen;
        mouseOffset = Vector2.ClampMagnitude(mouseOffset, maxMouseDistance);

        float normalizedX = mouseOffset.x / maxMouseDistance;
        float normalizedY = mouseOffset.y / maxMouseDistance;

        float targetTiltX = -normalizedY * tiltAmountX;
        float targetTiltY = normalizedX * tiltAmountY;

        swapPunchZ = Mathf.SmoothDamp(swapPunchZ, 0f, ref swapPunchVelocity, 1f / punchDamping);

        Quaternion targetRotation = Quaternion.Euler(
            targetTiltX,
            targetTiltY,
            moveBasedZ + swapPunchZ
        );

        tiltRoot.localRotation = Quaternion.Slerp(
            tiltRoot.localRotation,
            targetRotation,
            Time.deltaTime * tiltSmooth
        );
    }

    private void UpdateShadow()
    {
        if (shadow == null)
            return;

        Vector2 mouseScreen = Input.mousePosition;
        Vector2 visualScreen = RectTransformUtility.WorldToScreenPoint(null, visualRoot.position);
        Vector2 mouseOffset = mouseScreen - visualScreen;
        mouseOffset = Vector2.ClampMagnitude(mouseOffset, maxMouseDistance);

        Vector2 normalized = mouseOffset / maxMouseDistance;

        Vector3 targetShadowPos = new Vector3(
            normalized.x * shadowOffsetAmount,
            normalized.y * shadowOffsetAmount,
            0f
        );

        if (card.IsDragging)
            targetShadowPos += new Vector3(0f, -shadowDragExtraOffset, 0f);

        shadow.localPosition = Vector3.Lerp(
            shadow.localPosition,
            targetShadowPos,
            Time.deltaTime * shadowFollowSpeed
        );

        shadow.localScale = Vector3.Lerp(
            shadow.localScale,
            visualRoot.localScale * shadowScaleMultiplier,
            Time.deltaTime * shadowFollowSpeed
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
        swapPunchZ += punchAngle * direction;
    }
}