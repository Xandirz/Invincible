using UnityEngine;

public class FlyingCardToGenerator : MonoBehaviour
{
    public float flySpeed = 10f;

    private RectTransform rectTransform;
    private CardGenerator targetGenerator;
    private HandController handController;
    private bool initialized;

    public void Initialize(CardGenerator generator, HandController controller)
    {
        targetGenerator = generator;
        handController = controller;
        rectTransform = GetComponent<RectTransform>();
        initialized = rectTransform != null && targetGenerator != null && handController != null;
    }

    private void Update()
    {
        if (!initialized || rectTransform == null || targetGenerator == null)
            return;

        RectTransform targetRect = targetGenerator.transform as RectTransform;
        if (targetRect == null)
            return;

        Vector3 targetPosition = targetRect.position;

        rectTransform.position = Vector3.Lerp(
            rectTransform.position,
            targetPosition,
            Time.deltaTime * flySpeed
        );

        if (Vector3.Distance(rectTransform.position, targetPosition) < 20f)
        {
            handController.MoveExistingCardToGenerator(gameObject, targetGenerator);
            Destroy(this);
        }
    }
}