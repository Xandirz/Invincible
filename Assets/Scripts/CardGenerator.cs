using System.Collections.Generic;
using UnityEngine;

public class CardGenerator : MonoBehaviour
{
    [Header("Zone")]
    public RectTransform zoneRect;

    [Header("Layout")]
    public float cardSpacing = 160f;
    public float layoutSpeed = 12f;

    [Header("Rules")]
    public CardType acceptedCardType;
    public GeneratedStatType generatedStatType;
    public float statsPerCard = 1f;

    [Header("References")]
    public PlayerStats playerStats;

    private readonly List<CardDrag> cards = new();
    private float shieldRefreshTimer;

    public IReadOnlyList<CardDrag> Cards => cards;
    public int Count => cards.Count;

    private void Awake()
    {
        if (zoneRect == null)
            zoneRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        UpdateLayout();
        ApplyGeneratorEffect();
    }

    public bool IsInsideZone(Vector2 screenPoint, Camera uiCamera)
    {
        if (zoneRect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(zoneRect, screenPoint, uiCamera);
    }

    public bool CanAcceptCard(CardDrag card)
    {
        if (card == null)
            return false;

        CardData cardData = card.GetCardData();
        if (cardData == null)
            return false;

        return cardData.cardType == acceptedCardType;
    }

    public void AddCard(CardDrag card)
    {
        if (card == null || cards.Contains(card) || !CanAcceptCard(card))
            return;

        cards.Add(card);
        card.SetCurrentZone(CardZone.Generator);
        card.SetCurrentGenerator(this);
        RebuildOrder();
        ApplyInstantBonusIfNeeded();
    }

    public void RemoveCard(CardDrag card)
    {
        if (card == null)
            return;

        if (cards.Remove(card))
        {
            RebuildOrder();
            ApplyInstantBonusIfNeeded();
        }
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

            if (draggedX > cards[i].GeneratorTargetLocalPosition.x)
                newIndex = i + 1;
        }

        newIndex = Mathf.Clamp(newIndex, 0, cards.Count - 1);

        if (oldIndex == newIndex)
            return;

        cards.Remove(draggedCard);
        cards.Insert(newIndex, draggedCard);
        RebuildOrder();
    }

    private void RebuildOrder()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetSiblingIndex(i);
            cards[i].SetGeneratorIndex(i);
        }
    }

    private void UpdateLayout()
    {
        int count = cards.Count;
        if (count == 0)
            return;

        float totalWidth = (count - 1) * cardSpacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector3 targetPos = new Vector3(startX + i * cardSpacing, 0f, 0f);
            Quaternion targetRot = Quaternion.identity;

            cards[i].SetGeneratorTarget(targetPos, targetRot);
            cards[i].AnimateGeneratorZone(layoutSpeed);
        }
    }

    private void ApplyGeneratorEffect()
    {
        if (playerStats == null)
            return;

        float totalValue = cards.Count * statsPerCard;

        switch (generatedStatType)
        {
            case GeneratedStatType.InvisibilityShield:
                shieldRefreshTimer += Time.deltaTime;

                if (shieldRefreshTimer >= 1f)
                {
                    playerStats.SetGeneratorBonus(GeneratedStatType.InvisibilityShield, totalValue);
                    shieldRefreshTimer = 0f;
                }
                break;

            case GeneratedStatType.Damage:
                playerStats.SetGeneratorBonus(GeneratedStatType.Damage, totalValue);
                break;

            case GeneratedStatType.AttackSpeed:
                playerStats.SetGeneratorBonus(GeneratedStatType.AttackSpeed, totalValue);
                break;
        }
    }

    private void ApplyInstantBonusIfNeeded()
    {
        if (playerStats == null)
            return;

        float totalValue = cards.Count * statsPerCard;

        if (generatedStatType == GeneratedStatType.Damage)
        {
            playerStats.SetGeneratorBonus(GeneratedStatType.Damage, totalValue);
        }
        else if (generatedStatType == GeneratedStatType.AttackSpeed)
        {
            playerStats.SetGeneratorBonus(GeneratedStatType.AttackSpeed, totalValue);
        }
        else if (generatedStatType == GeneratedStatType.InvisibilityShield && cards.Count == 0)
        {
            playerStats.SetGeneratorBonus(GeneratedStatType.InvisibilityShield, 0f);
        }
    }
}