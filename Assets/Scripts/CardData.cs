using UnityEngine;

public enum CardType
{
   
    Blue,
    Pink,
    Yellow,
    White
}

public class CardData : MonoBehaviour
{
    public string cardName = "Card";
    public CardType cardType;
}