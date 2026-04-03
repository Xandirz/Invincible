using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject cardPrefab;
    public Transform hand;

    [Header("Arc Layout")]
    public float radius = 260f;
    public float angleStep = 12f;
    public float maxTotalAngle = 90f;

    [Header("Animation")]
    public float layoutLerpSpeed = 12f;
    public float returnSpeed = 14f;
    public float dragScale = 1.1f;

    private readonly List<CardDrag> cards = new();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddCard();
        }

        UpdateHandLayout();
    }

    public void AddCard()
    {
        if (cardPrefab == null || hand == null)
        {
            Debug.LogWarning("HandController: не назначены cardPrefab или hand");
            return;
        }

        GameObject cardObj = Instantiate(cardPrefab, hand);
        CardDrag card = cardObj.GetComponent<CardDrag>();

        if (card == null)
            card = cardObj.AddComponent<CardDrag>();

        card.Initialize(this);
        cards.Add(card);

        RefreshSiblingOrder();
        ForceSnapTargets();
    }

    public void RemoveCard(CardDrag card)
    {
        if (cards.Contains(card))
            cards.Remove(card);

        RefreshSiblingOrder();
    }

    public void BringToFront(CardDrag card)
    {
        card.transform.SetAsLastSibling();
    }

    public void ReorderCardByPosition(CardDrag draggedCard)
    {
        if (draggedCard == null) return;

        float draggedX = draggedCard.transform.localPosition.x;

        int targetIndex = 0;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == draggedCard) continue;

            float candidateX = cards[i].TargetLocalPosition.x;
            float distance = Mathf.Abs(draggedX - candidateX);

            if (draggedX > candidateX)
                targetIndex = i + 1;

            if (distance < bestDistance)
                bestDistance = distance;
        }

        targetIndex = Mathf.Clamp(targetIndex, 0, cards.Count - 1);

        cards.Remove(draggedCard);
        cards.Insert(targetIndex, draggedCard);

        RefreshSiblingOrder();
    }

    private void RefreshSiblingOrder()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetSiblingIndex(i);
            cards[i].SetLogicalIndex(i, cards.Count);
        }
    }

    private void UpdateHandLayout()
    {
        int count = cards.Count;
        if (count == 0) return;

        float totalAngle = Mathf.Min(maxTotalAngle, angleStep * (count - 1));
        float startAngle = -totalAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = (count == 1) ? 0f : startAngle + (totalAngle / (count - 1)) * i;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * radius;
            float y = Mathf.Cos(rad) * radius - radius;

            Vector3 targetPos = new Vector3(x, y, 0f);
            Quaternion targetRot = Quaternion.Euler(0f, 0f, -angle);

            cards[i].SetTarget(targetPos, targetRot);
            cards[i].AnimateToTarget(layoutLerpSpeed, returnSpeed, dragScale);
        }
    }

    private void ForceSnapTargets()
    {
        foreach (var card in cards)
        {
            card.SnapInstant();
        }
    }
}