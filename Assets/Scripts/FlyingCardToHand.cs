using UnityEngine;

public class FlyingCardToHand : MonoBehaviour
{
    public float flySpeed = 8f;

    private Transform target;
    private HandController handController;
    private bool initialized;

    public void Initialize(Transform targetTransform, HandController controller)
    {
        target = targetTransform;
        handController = controller;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || target == null || handController == null)
            return;

        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            Time.deltaTime * flySpeed
        );

        if (Vector3.Distance(transform.position, target.position) <= 20f)
        {
            handController.ConvertFlyingCardToHandCard(gameObject);
            Destroy(this);
        }
    }
}