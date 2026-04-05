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

    [Header("Reward Spawn")]
    public Canvas rootCanvas;
    public float rewardCardFlySpeed = 8f;

    [Header("Auto Move To Generator")]
    public float autoMoveFlySpeed = 10f;

    private readonly List<CardDrag> cards = new();

    private void Awake()
    {
        if (rootCanvas == null)
            rootCanvas = FindObjectOfType<Canvas>();
    }

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
        card.SetCurrentGenerator(null);

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
        card.SetCurrentGenerator(null);

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
        if (card == null)
            return;

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

            int typeA = dataA == null ? int.MaxValue : (int)dataA.cardType;
            int typeB = dataB == null ? int.MaxValue : (int)dataB.cardType;

            return typeB.CompareTo(typeA);
        });

        RefreshSiblingOrder();
    }

    public void SendMatchingCardsToGenerator(CardGenerator generator, int maxCount)
    {
        if (generator == null || maxCount <= 0)
            return;

        List<CardDrag> matchingCards = new List<CardDrag>();

        for (int i = 0; i < cards.Count; i++)
        {
            CardDrag card = cards[i];
            if (card == null)
                continue;

            if (!generator.CanAcceptCard(card))
                continue;

            matchingCards.Add(card);

            if (matchingCards.Count >= maxCount)
                break;
        }

        for (int i = 0; i < matchingCards.Count; i++)
        {
            StartFlyingCardToGenerator(matchingCards[i], generator);
        }
    }
    private void StartFlyingCardToGenerator(CardDrag card, CardGenerator generator)
    {
        if (card == null || generator == null)
            return;

        RemoveCardFromHand(card);

        FlyingCardToGenerator flying = card.GetComponent<FlyingCardToGenerator>();
        if (flying == null)
            flying = card.gameObject.AddComponent<FlyingCardToGenerator>();

        flying.flySpeed = autoMoveFlySpeed;
        flying.Initialize(generator, this);
    }

    public void MoveExistingCardToGenerator(GameObject cardObj, CardGenerator generator)
    {
        if (cardObj == null || generator == null)
            return;

        CardDrag card = cardObj.GetComponent<CardDrag>();
        if (card == null)
            return;

        card.transform.SetParent(generator.transform, false);
        generator.AddCard(card);
        card.SetCurrentGenerator(generator);
        card.SetCurrentZone(CardZone.Generator);
        card.SnapInstant();
    }
    public void SpawnGeneratedCardFromGenerator(CardType cardType, RectTransform generatorRect)
    {
        if (hand == null || rootCanvas == null || generatorRect == null)
            return;

        GameObject prefab = GetCardPrefabByType(cardType);
        if (prefab == null)
        {
            Debug.LogWarning($"HandController: не найден prefab карты с типом {cardType}");
            return;
        }

        GameObject cardObj = Instantiate(prefab, rootCanvas.transform);

        RectTransform rect = cardObj.GetComponent<RectTransform>();
        if (rect == null)
        {
            Destroy(cardObj);
            return;
        }

        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            generatorRect.position
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out Vector2 localPoint
        );

        rect.anchoredPosition = localPoint;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        CardDrag existingDrag = cardObj.GetComponent<CardDrag>();
        if (existingDrag != null)
            Destroy(existingDrag);

        FlyingCardToHand flying = cardObj.GetComponent<FlyingCardToHand>();
        if (flying == null)
            flying = cardObj.AddComponent<FlyingCardToHand>();

        flying.flySpeed = rewardCardFlySpeed;
        flying.Initialize(hand, this);
    }    public void SpawnRewardCardFromWorld(Vector3 worldPosition)
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0 || hand == null || rootCanvas == null)
            return;

        GameObject rewardPrefab = GetCardPrefabByType(CardType.White);

   

        GameObject cardObj = Instantiate(rewardPrefab, rootCanvas.transform);
        RectTransform rect = cardObj.GetComponent<RectTransform>();
        if (rect == null)
            return;

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPoint
        );

        rect.anchoredPosition = localPoint;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        CardDrag existingDrag = cardObj.GetComponent<CardDrag>();
        if (existingDrag != null)
            Destroy(existingDrag);

        FlyingCardToHand flying = cardObj.AddComponent<FlyingCardToHand>();
        flying.flySpeed = rewardCardFlySpeed;
        flying.Initialize(hand, this);
    }
    private GameObject GetCardPrefabByType(CardType cardType)
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0)
            return null;

        for (int i = 0; i < cardPrefabs.Count; i++)
        {
            GameObject prefab = cardPrefabs[i];
            if (prefab == null)
                continue;

            CardData cardData = prefab.GetComponent<CardData>();
            if (cardData == null)
                continue;

            if (cardData.cardType == cardType)
                return prefab;
        }

        return null;
    }
    public void ConvertFlyingCardToHandCard(GameObject cardObj)
    {
        if (cardObj == null || hand == null)
            return;

        cardObj.transform.SetParent(hand, false);

        CardDrag card = cardObj.GetComponent<CardDrag>();
        if (card == null)
            card = cardObj.AddComponent<CardDrag>();

        card.Initialize(this);
        card.SetCurrentZone(CardZone.Hand);
        card.SetCurrentGenerator(null);

        if (!cards.Contains(card))
            cards.Add(card);

        RefreshSiblingOrder();
        ForceSnapTargets();
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
    public void AddCardByType(CardType cardType)
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0 || hand == null)
        {
            Debug.LogWarning("HandController: не назначены cardPrefabs или hand");
            return;
        }

        GameObject matchedPrefab = null;

        for (int i = 0; i < cardPrefabs.Count; i++)
        {
            GameObject prefab = cardPrefabs[i];
            if (prefab == null)
                continue;

            CardData cardData = prefab.GetComponent<CardData>();
            if (cardData == null)
                continue;

            if (cardData.cardType == cardType)
            {
                matchedPrefab = prefab;
                break;
            }
        }

        if (matchedPrefab == null)
        {
            Debug.LogWarning($"HandController: не найден prefab карты с типом {cardType}");
            return;
        }

        GameObject cardObj = Instantiate(matchedPrefab, hand);
        CardDrag card = cardObj.GetComponent<CardDrag>();

        if (card == null)
            card = cardObj.AddComponent<CardDrag>();

        card.Initialize(this);
        card.SetCurrentZone(CardZone.Hand);
        card.SetCurrentGenerator(null);

        cards.Add(card);

        RefreshSiblingOrder();
        ForceSnapTargets();
    }
    private void ForceSnapTargets()
    {
        foreach (var card in cards)
            card.SnapInstant();
    }
}