using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Projectile projectilePrefab;
    public Transform firePoint;

    private float attackTimer;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (playerStats == null || projectilePrefab == null || firePoint == null)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        Enemy[] targets = FindTargetsInRange(Mathf.RoundToInt(playerStats.projectileCount));
        if (targets.Length == 0)
            return;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
                continue;

            Shoot(targets[i]);
        }

        attackTimer = GetAttackCooldown();
    }
    private void Shoot(Enemy target)
    {
        Projectile projectileInstance = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        projectileInstance.Initialize(target.transform, playerStats.damage, playerStats);
    }
    private Enemy[] FindTargetsInRange(int targetCount)
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        List<Enemy> availableTargets = new List<Enemy>();

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null)
                continue;

            float distance = Vector2.Distance(transform.position, enemies[i].transform.position);

            if (distance > playerStats.attackRange)
                continue;

            availableTargets.Add(enemies[i]);
        }

        availableTargets.Sort((a, b) =>
        {
            float distanceA = Vector2.Distance(transform.position, a.transform.position);
            float distanceB = Vector2.Distance(transform.position, b.transform.position);
            return distanceA.CompareTo(distanceB);
        });

        int finalCount = Mathf.Min(targetCount, availableTargets.Count);
        Enemy[] result = new Enemy[finalCount];

        for (int i = 0; i < finalCount; i++)
            result[i] = availableTargets[i];

        return result;
    }


    private float GetAttackCooldown()
    {
        if (playerStats.attackSpeed <= 0f)
            return 999f;

        return 1f / playerStats.attackSpeed;
    }
}