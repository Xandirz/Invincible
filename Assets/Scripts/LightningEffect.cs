using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningEffect : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 0.12f;

    [Header("Shape")]
    [SerializeField] private int segments = 7;
    [SerializeField] private float jaggedOffset = 0.25f;

    [Header("Width")]
    [SerializeField] private float startWidth = 0.15f;
    [SerializeField] private float endWidth = 0.05f;

    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 50;

    [Header("Color")]
    [SerializeField] private Color startColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color endColor = new Color(0.5f, 1f, 1f, 0.85f);

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        segments = Mathf.Max(2, segments);
        lineRenderer.positionCount = segments;
        lineRenderer.useWorldSpace = true;

        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
            lineRenderer.material = new Material(spriteShader);

        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.sortingLayerName = sortingLayerName;
        lineRenderer.sortingOrder = sortingOrder;

        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;

        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.numCapVertices = 2;
        lineRenderer.numCornerVertices = 2;
    }

    public void Play(Vector3 startPoint, Vector3 endPoint)
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        SetupLineRenderer();
        BuildLightning(startPoint, endPoint);

        Destroy(gameObject, lifeTime);
    }

    private void BuildLightning(Vector3 startPoint, Vector3 endPoint)
    {
        segments = Mathf.Max(2, segments);
        lineRenderer.positionCount = segments;

        Vector3 direction = endPoint - startPoint;
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.forward);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 point = Vector3.Lerp(startPoint, endPoint, t);

            if (i != 0 && i != segments - 1)
            {
                float randomOffset = Random.Range(-jaggedOffset, jaggedOffset);
                point += perpendicular * randomOffset;
            }

            lineRenderer.SetPosition(i, point);
        }
    }
}