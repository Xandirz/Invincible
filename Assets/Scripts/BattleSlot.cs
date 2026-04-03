using UnityEngine;

public class BattleSlot : MonoBehaviour
{
    public UnitTeam team;
    public int slotIndex;

    [HideInInspector] public BattleUnit currentUnit;

    public bool IsOccupied => currentUnit != null;

    public void SetUnit(BattleUnit unit)
    {
        currentUnit = unit;

        if (unit == null)
            return;

        unit.transform.SetParent(transform, false);
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localRotation = Quaternion.identity;
    }
}