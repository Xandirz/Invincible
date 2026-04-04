using UnityEngine;

public enum CardType
{
   
    Blue,
    Yellow,
    Pink
}

public class CardData : MonoBehaviour
{
    public string cardName = "Card";
    public CardType cardType;
}