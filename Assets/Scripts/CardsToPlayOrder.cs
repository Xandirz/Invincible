using System.Collections.Generic;
using UnityEngine;

public class CardsToPlayOrder : MonoBehaviour
{
    [Header("Zone")]
    public RectTransform zoneRect;

    [Header("Layout")]
    public float cardSpacing = 160f;
    public float layoutSpeed = 12f;

    private readonly List<CardDrag> queuedCards = new();

    public IReadOnlyList<CardDrag> QueuedCards => queuedCards;
    public int Count => queuedCards.Count;

    private void Awake()
    {
        if (zoneRect == null)
            zoneRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        UpdateQueueLayout();
    }

    public bool IsInsideZone(Vector2 screenPoint, Camera uiCamera)
    {
        if (zoneRect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(zoneRect, screenPoint, uiCamera);
    }

    public void AddCard(CardDrag card)
    {
        if (card == null) return;
        if (queuedCards.Contains(card)) return;

        queuedCards.Add(card);
        card.SetCurrentZone(CardZone.PlayOrder);
        RebuildOrder();
    }

    public void RemoveCard(CardDrag card)
    {
        if (card == null) return;

        if (queuedCards.Remove(card))
            RebuildOrder();
    }



    public void ReorderCardByPosition(CardDrag draggedCard)
    {
        if (draggedCard == null || queuedCards.Count <= 1)
            return;

        float draggedX = draggedCard.transform.localPosition.x;

        int oldIndex = queuedCards.IndexOf(draggedCard);
        int newIndex = 0;

        for (int i = 0; i < queuedCards.Count; i++)
        {
            if (queuedCards[i] == draggedCard)
                continue;

            if (draggedX > queuedCards[i].PlayZoneTargetLocalPosition.x)
                newIndex = i + 1;
        }

        newIndex = Mathf.Clamp(newIndex, 0, queuedCards.Count - 1);

        if (oldIndex == newIndex)
            return;

        queuedCards.Remove(draggedCard);
        queuedCards.Insert(newIndex, draggedCard);
        RebuildOrder();
    }

    private void RebuildOrder()
    {
        for (int i = 0; i < queuedCards.Count; i++)
        {
            queuedCards[i].transform.SetSiblingIndex(i);
            queuedCards[i].SetPlayOrderIndex(i);
        }
    }

    private void UpdateQueueLayout()
    {
        int count = queuedCards.Count;
        if (count == 0)
            return;

        float totalWidth = (count - 1) * cardSpacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector3 targetPos = new Vector3(startX + i * cardSpacing, 0f, 0f);
            Quaternion targetRot = Quaternion.identity;

            queuedCards[i].SetPlayZoneTarget(targetPos, targetRot);
            queuedCards[i].AnimatePlayZone(layoutSpeed);
        }
    }
}