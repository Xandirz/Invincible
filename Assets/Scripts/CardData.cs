using UnityEngine;

public enum CardType
{
    Damage,
    Heal,
    Buff,
    Invulnerability
}

public class CardData : MonoBehaviour
{
    public string cardName = "Card";
    public CardType cardType;

    public void PlayEffect()
    {
        Debug.Log($"Play card: {cardName} | Type: {cardType}");
    }

    public void PlayEffect(BattleContext context)
    {
        Debug.Log(
            $"Play card: {cardName} | Type: {cardType} | Step: {context.turnStep + 1}"
        );
    }
}