using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 10f;
    public float lifeTime = 5f;

    private Transform target;
    private float damage;
    private PlayerStats ownerStats;
    private bool hasHit;

    public void Initialize(Transform targetTransform, float projectileDamage, PlayerStats stats)
    {
        target = targetTransform;
        damage = projectileDamage;
        ownerStats = stats;
        hasHit = false;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (hasHit)
            return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null)
            return;

        if (target != null && other.transform != target)
            return;

        hasHit = true;

        enemy.TakeDamage(damage);
        TryTriggerChainLightning(enemy);

        Destroy(gameObject);
    }

    private void TryTriggerChainLightning(Enemy firstEnemy)
    {
        if (ownerStats == null || firstEnemy == null)
            return;

        if (ownerStats.lightningChance <= 0f)
            return;

        if (ownerStats.lightningDamage <= 0f)
            return;

        if (ownerStats.lightningChains <= 0)
            return;

        if (ownerStats.lightningRange <= 0f)
            return;

        if (Random.value > ownerStats.lightningChance)
            return;

        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
        hitEnemies.Add(firstEnemy);

        Enemy currentEnemy = firstEnemy;
        int remainingChains = Mathf.Max(0, ownerStats.lightningChains);

        for (int i = 0; i < remainingChains; i++)
        {
            Enemy nextEnemy = FindNearestEnemyInRange(
                currentEnemy.transform.position,
                ownerStats.lightningRange,
                hitEnemies
            );

            if (nextEnemy == null)
                break;

            SpawnLightningVfx(currentEnemy.transform.position, nextEnemy.transform.position);
            nextEnemy.TakeElectricDamage(ownerStats.lightningDamage);
            hitEnemies.Add(nextEnemy);
            currentEnemy = nextEnemy;
        }
    }

    private Enemy FindNearestEnemyInRange(Vector3 fromPosition, float range, HashSet<Enemy> excluded)
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        Enemy nearestEnemy = null;
        float bestSqrDistance = range * range;

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];

            if (enemy == null)
                continue;

            if (excluded != null && excluded.Contains(enemy))
                continue;

            float sqrDistance = (enemy.transform.position - fromPosition).sqrMagnitude;
            if (sqrDistance > bestSqrDistance)
                continue;

            bestSqrDistance = sqrDistance;
            nearestEnemy = enemy;
        }

        return nearestEnemy;
    }

    private void SpawnLightningVfx(Vector3 start, Vector3 end)
    {
        if (ownerStats == null)
            return;

        if (ownerStats.lightningVfxPrefab == null)
            return;

        LightningEffect effect = Instantiate(ownerStats.lightningVfxPrefab);
        effect.Play(start, end);
    }
}