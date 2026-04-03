using UnityEngine;

public enum UnitTeam
{
    Player,
    Enemy
}

public class BattleUnit : MonoBehaviour
{
    public string unitName = "Unit";
    public UnitTeam team;
    public int hp = 10;

    public virtual void PerformTurnAction()
    {
        Debug.Log($"{unitName} ({team}) performs action");
    }
}