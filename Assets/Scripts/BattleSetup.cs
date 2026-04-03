using UnityEngine;

public class BattleSetup : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject goblinPrefab;

    [Header("Slots")]
    public BattleSlot[] playerSlots; // 4 слота
    public BattleSlot[] enemySlots;  // 6 слотов

    private void Start()
    {
        SpawnInitialUnits();
    }

    public void SpawnInitialUnits()
    {
        SpawnPlayer();
        SpawnEnemies();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("BattleSetup: playerPrefab не назначен");
            return;
        }

        if (playerSlots == null || playerSlots.Length == 0)
        {
            Debug.LogWarning("BattleSetup: playerSlots не назначены");
            return;
        }

        GameObject playerObj = Instantiate(playerPrefab);
        BattleUnit unit = playerObj.GetComponent<BattleUnit>();

        if (unit == null)
            unit = playerObj.AddComponent<BattleUnit>();

        unit.unitName = "Player";
        unit.team = UnitTeam.Player;

        playerSlots[0].SetUnit(unit);
    }

    private void SpawnEnemies()
    {
        if (goblinPrefab == null)
        {
            Debug.LogWarning("BattleSetup: goblinPrefab не назначен");
            return;
        }

        if (enemySlots == null || enemySlots.Length == 0)
        {
            Debug.LogWarning("BattleSetup: enemySlots не назначены");
            return;
        }

        for (int i = 0; i < enemySlots.Length; i++)
        {
            GameObject enemyObj = Instantiate(goblinPrefab);
            BattleUnit unit = enemyObj.GetComponent<BattleUnit>();

            if (unit == null)
                unit = enemyObj.AddComponent<BattleUnit>();

            unit.unitName = $"Goblin {i + 1}";
            unit.team = UnitTeam.Enemy;

            enemySlots[i].SetUnit(unit);
        }
    }
}