using UnityEngine;

public class EnemyDeathRewardManager : MonoBehaviour
{
    public static EnemyDeathRewardManager Instance { get; private set; }

    [Header("Reward Settings")]
    public int rewardEveryKills = 10;

    [Header("References")]
    public HandController handController;

    private int killedEnemiesCount;
    public int maxCardsInHandForReward = 50;
    private void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemyDeath(Vector3 worldPosition)
    {
        killedEnemiesCount++;

        if (rewardEveryKills <= 0)
            return;

        if (killedEnemiesCount % rewardEveryKills != 0)
            return;

        if (handController == null)
            return;

        if (handController.transform.childCount > maxCardsInHandForReward)
            return;


        handController.SpawnRewardCardFromWorld(worldPosition);
    }
}