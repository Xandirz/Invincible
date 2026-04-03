using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Spawn")]
    public List<GameObject> cardPrefabs = new();
    public Transform hand;

    [Header("Arc Layout")]
    public float radius = 260f;
    public float angleStep = 12f;
    public float maxTotalAngle = 90f;

    [Header("Animation")]
    public float layoutLerpSpeed = 14f;
    public float returnSpeed = 14f;
    public float dragScale = 1.08f;

    private readonly List<CardDrag> cards = new();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            AddRandomCard();

        UpdateHandLayout();
    }

    public void AddRandomCard()
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0 || hand == null)
        {
            Debug.LogWarning("HandController: не назначены cardPrefabs или hand");
            return;
        }

        int randomIndex = Random.Range(0, cardPrefabs.Count);
        GameObject randomPrefab = cardPrefabs[randomIndex];

        if (randomPrefab == null)
        {
            Debug.LogWarning("HandController: в списке cardPrefabs есть пустой элемент");
            return;
        }

        GameObject cardObj = Instantiate(randomPrefab, hand);
        CardDrag card = cardObj.GetComponent<CardDrag>();

        if (card == null)
            card = cardObj.AddComponent<CardDrag>();

        card.Initialize(this);
        card.SetCurrentZone(CardZone.Hand);

        cards.Add(card);

        RefreshSiblingOrder();
        ForceSnapTargets();
    }

    public void AddExistingCardToHand(CardDrag card)
    {
        if (card == null)
            return;

        if (!cards.Contains(card))
            cards.Add(card);

        card.SetCurrentZone(CardZone.Hand);

        RefreshSiblingOrder();
    }

    public void RemoveCardFromHand(CardDrag card)
    {
        if (card == null)
            return;

        cards.Remove(card);
        RefreshSiblingOrder();
    }

    public void BringToFront(CardDrag card)
    {
        card.transform.SetAsLastSibling();
    }

    public void ReorderCardByPosition(CardDrag draggedCard)
    {
        if (draggedCard == null || cards.Count <= 1)
            return;

        float draggedX = draggedCard.transform.localPosition.x;

        int oldIndex = cards.IndexOf(draggedCard);
        int newIndex = 0;

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == draggedCard)
                continue;

            if (draggedX > cards[i].TargetLocalPosition.x)
                newIndex = i + 1;
        }

        newIndex = Mathf.Clamp(newIndex, 0, cards.Count - 1);

        if (oldIndex == newIndex)
            return;

        cards.Remove(draggedCard);
        cards.Insert(newIndex, draggedCard);

        RefreshSiblingOrder();
        NotifySwapPunch(oldIndex, newIndex);
    }

    public void SortCardsByTypeRightToLeft()
    {
        cards.Sort((a, b) =>
        {
            CardData dataA = a.GetComponent<CardData>();
            CardData dataB = b.GetComponent<CardData>();

            int typeA = GetTypePriority(dataA);
            int typeB = GetTypePriority(dataB);

            return typeB.CompareTo(typeA);
        });

        RefreshSiblingOrder();
    }

    private int GetTypePriority(CardData data)
    {
        if (data == null)
            return int.MaxValue;

        switch (data.cardType)
        {
            case CardType.Damage:
                return 0;
            case CardType.Heal:
                return 1;
            case CardType.Buff:
                return 2;
            case CardType.Invulnerability:
                return 3;
            default:
                return 999;
        }
    }

    private void NotifySwapPunch(int oldIndex, int newIndex)
    {
        int min = Mathf.Min(oldIndex, newIndex);
        int max = Mathf.Max(oldIndex, newIndex);

        for (int i = min; i <= max; i++)
        {
            if (i < 0 || i >= cards.Count)
                continue;

            if (cards[i].Visual == null)
                continue;

            if (cards[i] == cards[newIndex])
                continue;

            float dir = newIndex > oldIndex ? -1f : 1f;
            cards[i].Visual.PlaySwapPunch(dir);
        }
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
        if (count == 0)
            return;

        float totalAngle = Mathf.Min(maxTotalAngle, angleStep * (count - 1));
        float startAngle = -totalAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = count == 1 ? 0f : startAngle + (totalAngle / (count - 1)) * i;
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
            card.SnapInstant();
    }
}