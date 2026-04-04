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

        Enemy target = FindClosestEnemyInRange();
        if (target == null)
            return;

        if (attackTimer <= 0f)
        {
            Shoot(target);
            attackTimer = GetAttackCooldown();
        }
    }

    private Enemy FindClosestEnemyInRange()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null)
                continue;

            float distance = Vector2.Distance(transform.position, enemies[i].transform.position);

            if (distance > playerStats.attackRange)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemies[i];
            }
        }

        return closestEnemy;
    }

    private void Shoot(Enemy target)
    {
        Projectile projectileInstance = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        projectileInstance.Initialize(target.transform, playerStats.damage);
    }

    private float GetAttackCooldown()
    {
        if (playerStats.attackSpeed <= 0f)
            return 999f;

        return 1f / playerStats.attackSpeed;
    }
}