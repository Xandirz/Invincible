using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float moveSpeed = 40f;
    [SerializeField] private float lifeTime = 1f;

    private Color textColor;
    private RectTransform rectTransform;

    public static void Create(Canvas canvas, GameObject prefab, Vector3 worldPosition, float value, Color color)
    {
        if (canvas == null || prefab == null)
            return;

        GameObject obj = Instantiate(prefab, canvas.transform);
        DamagePopup popup = obj.GetComponent<DamagePopup>();
        popup.Setup(worldPosition, value, color, canvas);
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textColor = text.color;
    }

    public void Setup(Vector3 worldPosition, float value, Color color, Canvas canvas)
    {
        text.text = Mathf.RoundToInt(value).ToString();
        text.color = color;
        textColor = text.color;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPoint
        );

        rectTransform.anchoredPosition = localPoint;
    }

    private void Update()
    {
        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            textColor.a -= 4f * Time.deltaTime;
            text.color = textColor;

            if (textColor.a <= 0f)
                Destroy(gameObject);
        }
    }
}