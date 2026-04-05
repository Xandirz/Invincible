using UnityEngine;
using UnityEngine.UI;

public class UnlockManager : MonoBehaviour
{
    [Header("References")]
    public Transform generatorContent;
    public GameObject generatorPrefab;
    public HandController handController;
    public PlayerStats playerStats;
    public EnemySpawner enemySpawner;

    [Header("Start Unlock")]
    public int startBlueCardsInHand = 3;

    private void Start()
    {
        if (enemySpawner != null)
            enemySpawner.OnWaveCompleted += HandleWaveCompleted;

        CreateGenerator(
            acceptedCardType: CardType.Blue,
            generatedStatType: GeneratedStatType.InvisibilityShield,
            statsPerCard: 1f,
            imageColor: new Color32(0x58, 0xB9, 0xB9, 74)
        );

        AddStartCards(CardType.Blue, startBlueCardsInHand);
    }

    private void OnDestroy()
    {
        if (enemySpawner != null)
            enemySpawner.OnWaveCompleted -= HandleWaveCompleted;
    }

    private void HandleWaveCompleted(int waveNumber)
    {
        if (waveNumber == 1)
        {
            CreateGenerator(
                acceptedCardType: CardType.White,
                generatedStatType: GeneratedStatType.CardGeneration,
                statsPerCard: 0.01f,
                imageColor: new Color32(0x58, 0xB9, 0xB9, 74),
                generatedCardType: CardType.Blue
            );
        }
        else if (waveNumber == 2)
        {
            CreateGenerator(
                acceptedCardType: CardType.Pink,
                generatedStatType: GeneratedStatType.Damage,
                statsPerCard: 1f,
                imageColor: new Color32(0xFF, 0x00, 0x00, 74)
            );

            CreateGenerator(
                acceptedCardType: CardType.White,
                generatedStatType: GeneratedStatType.CardGeneration,
                statsPerCard: 0.01f,
                imageColor: new Color32(0xFF, 0xFF, 0xFF, 74),
                generatedCardType: CardType.Pink
            );
        }
        else if (waveNumber == 3)
        {
            CreateGenerator(
                acceptedCardType: CardType.Yellow,
                generatedStatType: GeneratedStatType.AttackSpeed,
                statsPerCard: 1f,
                imageColor: new Color32(0xFF, 0xE0, 0x66, 74)
            );

            CreateGenerator(
                acceptedCardType: CardType.White,
                generatedStatType: GeneratedStatType.CardGeneration,
                statsPerCard: 0.01f,
                imageColor: new Color32(0xFF, 0xFF, 0xFF, 74),
                generatedCardType: CardType.Yellow
            );
        }
        else if (waveNumber == 5)
        {
            CreateGenerator(
                acceptedCardType: CardType.Pink,
                generatedStatType: GeneratedStatType.ProjectileCount,
                statsPerCard: 1f,
                imageColor: new Color32(0xFF, 0x00, 0x00, 74)
            );
        }
    }

    private CardGenerator CreateGenerator(
        CardType acceptedCardType,
        GeneratedStatType generatedStatType,
        float statsPerCard,
        Color imageColor,
        CardType generatedCardType = CardType.Blue
    )
    {
        if (generatorContent == null || generatorPrefab == null)
        {
            Debug.LogWarning("UnlockManager: generatorContent или generatorPrefab не назначены");
            return null;
        }

        GameObject generatorObj = Instantiate(generatorPrefab, generatorContent);
        CardGenerator generator = generatorObj.GetComponent<CardGenerator>();

        if (generator == null)
        {
            Debug.LogWarning("UnlockManager: на generatorPrefab нет CardGenerator");
            Destroy(generatorObj);
            return null;
        }

        generator.acceptedCardType = acceptedCardType;
        generator.generatedStatType = generatedStatType;
        generator.statsPerCard = statsPerCard;
        generator.handController = handController;
        generator.playerStats = playerStats;
        generator.generatedCardType = generatedCardType;

        Image image = generatorObj.GetComponent<Image>();
        if (image != null)
            image.color = imageColor;

        return generator;
    }

    private void AddStartCards(CardType cardType, int count)
    {
        if (handController == null)
        {
            Debug.LogWarning("UnlockManager: handController не назначен");
            return;
        }

        for (int i = 0; i < count; i++)
            handController.AddCardByType(cardType);
    }
}