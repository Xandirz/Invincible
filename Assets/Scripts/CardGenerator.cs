using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardGenerator : MonoBehaviour, IPointerClickHandler
{
    [Header("Zone")]
    public RectTransform zoneRect;

    [Header("Layout")]
    public float cardSpacing = 160f;
    public float layoutSpeed = 12f;
    [Header("Adaptive Spacing")]
    public float spacingReducePerExtraCard = 0.2f;
    public float minCardSpacing = 0.1f;
    public int spacingReduceStartFromCard = 18;
    [Header("Rules")]
    public CardType acceptedCardType;
    public GeneratedStatType generatedStatType;
    public float statsPerCard = 1f;
    public CardType generatedCardType;
    [Header("UI")]
    public TMP_Text effectText;

    [Header("References")]
    public PlayerStats playerStats;
    public HandController handController;

    private readonly List<CardDrag> cards = new();
    private float cardGenerationTimeLeft;    
    public IReadOnlyList<CardDrag> Cards => cards;
    public int Count => cards.Count;
    [Header("Capacity")]
    public int maxCards = 20;
    public float baseCardGenerationCooldown = 30f;
    public float cardGenerationCooldownReducePerCard = 3f;
    public float minCardGenerationCooldown = 5f;
    public int FreeSlots => Mathf.Max(0, maxCards - cards.Count);
    public bool IsFull => cards.Count >= maxCards;
    private void Awake()
    {
        if (zoneRect == null)
            zoneRect = GetComponent<RectTransform>();

        if (handController == null)
            handController = FindObjectOfType<HandController>();

        UpdateEffectText();
    }

    private void Update()
    {
        UpdateLayout();
        ApplyGeneratorEffect();
        UpdateEffectText();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (eventData.clickCount < 2)
            return;

        if (handController == null)
            return;

        if (FreeSlots <= 0)
            return;

        handController.SendMatchingCardsToGenerator(this, FreeSlots);
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

        if (cards.Count >= maxCards)
            return false;

        CardData cardData = card.GetCardData();
        if (cardData == null)
            return false;

        return cardData.cardType == acceptedCardType;
    }
    
    private float GetCardGenerationCooldownForCurrentCards()
    {
        if (cards.Count <= 0)
            return 0f;

        float cooldown = baseCardGenerationCooldown - (cards.Count - 1) * cardGenerationCooldownReducePerCard;
        return Mathf.Max(minCardGenerationCooldown, cooldown);
    }

    public void AddCard(CardDrag card)
    {
        if (card == null || cards.Contains(card) || !CanAcceptCard(card))
            return;

        float oldCooldown = GetCardGenerationCooldownForCurrentCards();

        cards.Add(card);
        card.SetCurrentZone(CardZone.Generator);
        card.SetCurrentGenerator(this);
        RebuildOrder();
        ApplyInstantBonusIfNeeded();

        if (generatedStatType == GeneratedStatType.CardGeneration)
        {
            float newCooldown = GetCardGenerationCooldownForCurrentCards();

            if (oldCooldown <= 0f && newCooldown > 0f)
            {
                cardGenerationTimeLeft = newCooldown;
            }
            else if (oldCooldown > 0f && newCooldown > 0f)
            {
                float progressNormalized = 1f - Mathf.Clamp01(cardGenerationTimeLeft / oldCooldown);
                cardGenerationTimeLeft = newCooldown * (1f - progressNormalized);
            }
        }

        UpdateEffectText();
    }
    public void RemoveCard(CardDrag card)
    {
        if (card == null)
            return;

        if (!cards.Contains(card))
            return;

        float oldCooldown = GetCardGenerationCooldownForCurrentCards();

        if (cards.Remove(card))
        {
            RebuildOrder();
            ApplyInstantBonusIfNeeded();

            if (generatedStatType == GeneratedStatType.CardGeneration)
            {
                float newCooldown = GetCardGenerationCooldownForCurrentCards();

                if (newCooldown <= 0f)
                {
                    cardGenerationTimeLeft = 0f;
                }
                else if (oldCooldown > 0f)
                {
                    float progressNormalized = 1f - Mathf.Clamp01(cardGenerationTimeLeft / oldCooldown);
                    cardGenerationTimeLeft = newCooldown * (1f - progressNormalized);
                }
            }

            UpdateEffectText();
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

        float currentSpacing = cardSpacing;

        if (count > spacingReduceStartFromCard)
        {
            int extraCards = count - spacingReduceStartFromCard;
            currentSpacing -= extraCards * spacingReducePerExtraCard;
            currentSpacing = Mathf.Max(minCardSpacing, currentSpacing);
        }

        float totalWidth = (count - 1) * currentSpacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector3 targetPos = new Vector3(startX + i * currentSpacing, 0f, 0f);
            Quaternion targetRot = Quaternion.identity;

            cards[i].SetGeneratorTarget(targetPos, targetRot);
            cards[i].AnimateGeneratorZone(layoutSpeed);
        }
    }

    private void ApplyGeneratorEffect()
    {
        if (playerStats == null && generatedStatType != GeneratedStatType.CardGeneration)
            return;

        float totalValue = cards.Count * statsPerCard;

        switch (generatedStatType)
        {
            case GeneratedStatType.InvisibilityShield:
                break;

            case GeneratedStatType.Damage:
                playerStats.SetGeneratorBonus(GeneratedStatType.Damage, totalValue);
                break;

            case GeneratedStatType.AttackSpeed:
                playerStats.SetGeneratorBonus(GeneratedStatType.AttackSpeed, totalValue);
                break;

            case GeneratedStatType.ProjectileCount:
                playerStats.SetGeneratorBonus(GeneratedStatType.ProjectileCount, totalValue);
                break;

            case GeneratedStatType.LightningChance:
                playerStats.SetGeneratorBonus(GeneratedStatType.LightningChance, totalValue);
                break;

            case GeneratedStatType.LightningDamage:
                playerStats.SetGeneratorBonus(GeneratedStatType.LightningDamage, totalValue);
                break;

            case GeneratedStatType.LightningChains:
                playerStats.SetGeneratorBonus(GeneratedStatType.LightningChains, totalValue);
                break;

            case GeneratedStatType.CardGeneration:
                if (handController == null || cards.Count <= 0)
                    return;

                cardGenerationTimeLeft -= Time.deltaTime;

                if (cardGenerationTimeLeft <= 0f)
                {
                    handController.SpawnGeneratedCardFromGenerator(generatedCardType, zoneRect);
                    ConsumeAllCards();
                }
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
        else if (generatedStatType == GeneratedStatType.InvisibilityShield)
        {
            playerStats.SetGeneratorBonus(GeneratedStatType.InvisibilityShield, totalValue);
        }
        else if (generatedStatType == GeneratedStatType.ProjectileCount)
        {
            playerStats.SetGeneratorBonus(GeneratedStatType.ProjectileCount, totalValue);
        }
        else if (generatedStatType == GeneratedStatType.LightningChance)
        {
            playerStats.SetGeneratorBonus(GeneratedStatType.LightningChance, totalValue);
        }
        else if (generatedStatType == GeneratedStatType.LightningDamage)
        {
            playerStats.SetGeneratorBonus(GeneratedStatType.LightningDamage, totalValue);
        }
        else if (generatedStatType == GeneratedStatType.LightningChains)
        {
            playerStats.SetGeneratorBonus(GeneratedStatType.LightningChains, totalValue);
        }
        else if (generatedStatType == GeneratedStatType.CardGeneration && cards.Count == 0)
        {
            cardGenerationTimeLeft = 0f;
        }
    } 
    private void UpdateEffectText()
    {
        if (effectText == null)
            return;

        float totalValue = cards.Count * statsPerCard;

        if (generatedStatType == GeneratedStatType.CardGeneration)
        {
            if (cards.Count == 0)
            {
                effectText.text = $"place {acceptedCardType} and get {generatedCardType}";
                return;
            }

            effectText.text = $"{Mathf.CeilToInt(cardGenerationTimeLeft)} sec until {generatedCardType}";
            return;
        }

        if (generatedStatType == GeneratedStatType.LightningChance)
        {
            effectText.text = $"+{Mathf.RoundToInt(totalValue * 100f)}% lightning chance";
            return;
        }

        string statName = GetStatDisplayName();

        if (Mathf.Approximately(totalValue, Mathf.Round(totalValue)))
            effectText.text = $"+{Mathf.RoundToInt(totalValue)} {statName}";
        else
            effectText.text = $"+{totalValue:0.##} {statName}";
    }    
    private void ConsumeAllCards()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            CardDrag card = cards[i];
            if (card == null)
                continue;

            cards.RemoveAt(i);
            Destroy(card.gameObject);
        }

        RebuildOrder();
        ApplyInstantBonusIfNeeded();
        cardGenerationTimeLeft = 0f;
        UpdateEffectText();
    }
    private string GetStatDisplayName()
    {
        switch (generatedStatType)
        {
            case GeneratedStatType.Damage:
                return "damage";

            case GeneratedStatType.AttackSpeed:
                return "attack speed";

            case GeneratedStatType.InvisibilityShield:
                return "invincibility";

            case GeneratedStatType.ProjectileCount:
                return "projectile count";

            case GeneratedStatType.LightningChance:
                return "lightning chance";

            case GeneratedStatType.LightningDamage:
                return "lightning damage";

            case GeneratedStatType.LightningChains:
                return "lightning chains";

            case GeneratedStatType.CardGeneration:
                return "card generation";

            default:
                return "stat";
        }
    }
}