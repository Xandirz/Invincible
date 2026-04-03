using System.Collections;
using UnityEngine;

public class BattleTurnController : MonoBehaviour
{
    [Header("References")]
    public CardsToPlayOrder cardsToPlayOrder;
    public BattleSlot[] enemySlots; // 6 слотов

    [Header("Timing")]
    public float delayBetweenActions = 0.5f;

    private bool isPlayingTurn;

    public void PlayHand()
    {
        if (isPlayingTurn)
        {
            Debug.Log("BattleTurnController: turn is already playing");
            return;
        }

        StartCoroutine(PlayHandRoutine());
    }

    private IEnumerator PlayHandRoutine()
    {
        isPlayingTurn = true;

        Debug.Log("=== TURN START ===");

        int maxSteps = 6;

        for (int step = 0; step < maxSteps; step++)
        {
            Debug.Log($"--- STEP {step + 1} ---");

            BattleContext context = new BattleContext();
            context.turnStep = step;

            if (enemySlots != null && step < enemySlots.Length)
            {
                context.currentEnemySlot = enemySlots[step];
                context.currentEnemyUnit = enemySlots[step] != null ? enemySlots[step].currentUnit : null;
            }

            // 1. Наша карта
            CardDrag card = cardsToPlayOrder != null ? cardsToPlayOrder.GetCardAt(step) : null;
            if (card != null)
            {
                CardData data = card.GetComponent<CardData>();
                if (data != null)
                {
                    data.PlayEffect(context);
                }
                else
                {
                    Debug.Log($"Step {step + 1}: card has no CardData");
                }
            }
            else
            {
                Debug.Log($"Step {step + 1}: no player card");
            }

            yield return new WaitForSeconds(delayBetweenActions);

            // 2. Ход врага из соответствующего слота
            if (context.currentEnemyUnit != null)
            {
                Debug.Log($"Enemy slot {step + 1} action:");
                context.currentEnemyUnit.PerformTurnAction();
            }
            else
            {
                Debug.Log($"Enemy slot {step + 1} is empty");
            }

            yield return new WaitForSeconds(delayBetweenActions);
        }

        Debug.Log("=== TURN END ===");

        isPlayingTurn = false;
    }
}