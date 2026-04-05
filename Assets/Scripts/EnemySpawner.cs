using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public Enemy enemyPrefab;
    public float spawnRadius = 8f;

    [Header("Wave")]
    public int startEnemiesPerWave = 5;
    public int maxEnemiesPerWave = 500;
    public float delayBetweenWaves = 1f;
    public float spawnIntervalInWave = 0.05f;

    [Header("Enemy Stats Growth")]
    public float startEnemyHealth = 30f;
    public float startEnemyDamage = 5f;
    public float startEnemyMoveSpeed = 2f;

    public float healthGrowthPerWave = 5f;
    public float damageGrowthPerWave = 1f;
    public float moveSpeedGrowthPerWave = 0.05f;
    public System.Action<int> OnWaveCompleted;
    [Header("Count Growth")]
    public int enemiesGrowthPerWave = 2;

    public int currentWave = 0;
    private bool waveInProgress;
    private int aliveEnemiesInWave;

    private void Start()
    {
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        while (true)
        {
            yield return StartCoroutine(SpawnWave());

            while (aliveEnemiesInWave > 0)
                yield return null;

            OnWaveCompleted?.Invoke(currentWave);

            yield return new WaitForSeconds(delayBetweenWaves);
        }
    }

    private IEnumerator SpawnWave()
    {
        if (enemyPrefab == null)
            yield break;

        waveInProgress = true;
        currentWave++;

        int enemiesToSpawn = GetEnemiesCountForWave(currentWave);
        float waveHealth = GetHealthForWave(currentWave);
        float waveDamage = GetDamageForWave(currentWave);
        float waveMoveSpeed = GetMoveSpeedForWave(currentWave);

        aliveEnemiesInWave = enemiesToSpawn;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy(waveHealth, waveDamage, waveMoveSpeed);

            if (spawnIntervalInWave > 0f)
                yield return new WaitForSeconds(spawnIntervalInWave);
            else
                yield return null;
        }

        waveInProgress = false;
    }

    private void SpawnEnemy(float health, float damage, float moveSpeed)
    {
        Vector2 randomOffset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Enemy enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        enemyInstance.maxHealth = health;
        enemyInstance.currentHealth = health;
        enemyInstance.contactDamage = damage;
        enemyInstance.moveSpeed = moveSpeed;
        enemyInstance.SetSpawner(this);
    }

    private int GetEnemiesCountForWave(int waveNumber)
    {
        int count = startEnemiesPerWave + (waveNumber - 1) * enemiesGrowthPerWave;
        return Mathf.Clamp(count, 1, maxEnemiesPerWave);
    }

    private float GetHealthForWave(int waveNumber)
    {
        return startEnemyHealth + (waveNumber - 1) * healthGrowthPerWave;
    }

    private float GetDamageForWave(int waveNumber)
    {
        return startEnemyDamage + (waveNumber - 1) * damageGrowthPerWave;
    }

    private float GetMoveSpeedForWave(int waveNumber)
    {
        return startEnemyMoveSpeed + (waveNumber - 1) * moveSpeedGrowthPerWave;
    }

    public void NotifyEnemyDied()
    {
        aliveEnemiesInWave = Mathf.Max(0, aliveEnemiesInWave - 1);
    }
}