using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningLineVFX : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.12f;
    [SerializeField] private int segments = 6;
    [SerializeField] private float jaggedOffset = 0.25f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Play(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        segments = Mathf.Max(2, segments);
        lineRenderer.positionCount = segments;

        Vector3 direction = end - start;
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.forward);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            if (i != 0 && i != segments - 1)
            {
                float offset = Random.Range(-jaggedOffset, jaggedOffset);
                point += perpendicular * offset;
            }

            lineRenderer.SetPosition(i, point);
        }

        Destroy(gameObject, lifetime);
    }
}