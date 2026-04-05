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

    [Header("Base Enemy Stats")]
    public float startEnemyHealth = 30f;
    public float startEnemyDamage = 5f;
    public float startEnemyMoveSpeed = 2f;

    [Header("Growth Before Wave 10")]
    public float healthGrowthPerWave = 4f;
    public float damageGrowthPerWave = 0.5f;
    public float moveSpeedGrowthPerWave = 0.03f;
    public int enemiesGrowthPerWave = 2;

    [Header("Growth After Wave 10")]
    public int difficultyRampWave = 10;
    public float healthGrowthPerWaveAfter10 = 10f;
    public float damageGrowthPerWaveAfter10 = 1.5f;
    public float moveSpeedGrowthPerWaveAfter10 = 0.06f;
    public int enemiesGrowthPerWaveAfter10 = 4;
    [Header("Growth After Wave 25")]
    public int difficultyRampWave25 = 25;
    public float healthGrowthPerWaveAfter25 = 18f;
    public float damageGrowthPerWaveAfter25 = 2.5f;
    public float moveSpeedGrowthPerWaveAfter25 = 0.1f;
    public int enemiesGrowthPerWaveAfter25 = 6;
    public System.Action<int> OnWaveCompleted;

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
        if (waveNumber <= difficultyRampWave)
        {
            int count = startEnemiesPerWave + (waveNumber - 1) * enemiesGrowthPerWave;
            return Mathf.Clamp(count, 1, maxEnemiesPerWave);
        }
        else if (waveNumber <= difficultyRampWave25)
        {
            int countAtWave10 = startEnemiesPerWave + (difficultyRampWave - 1) * enemiesGrowthPerWave;
            int extraWaves = waveNumber - difficultyRampWave;
            int count = countAtWave10 + extraWaves * enemiesGrowthPerWaveAfter10;
            return Mathf.Clamp(count, 1, maxEnemiesPerWave);
        }
        else
        {
            int countAtWave10 = startEnemiesPerWave + (difficultyRampWave - 1) * enemiesGrowthPerWave;
            int countAtWave25 = countAtWave10 + (difficultyRampWave25 - difficultyRampWave) * enemiesGrowthPerWaveAfter10;
            int extraWaves = waveNumber - difficultyRampWave25;
            int count = countAtWave25 + extraWaves * enemiesGrowthPerWaveAfter25;
            return Mathf.Clamp(count, 1, maxEnemiesPerWave);
        }
    }

    private float GetHealthForWave(int waveNumber)
    {
        if (waveNumber <= difficultyRampWave)
        {
            return startEnemyHealth + (waveNumber - 1) * healthGrowthPerWave;
        }
        else if (waveNumber <= difficultyRampWave25)
        {
            float healthAtWave10 = startEnemyHealth + (difficultyRampWave - 1) * healthGrowthPerWave;
            int extraWaves = waveNumber - difficultyRampWave;
            return healthAtWave10 + extraWaves * healthGrowthPerWaveAfter10;
        }
        else
        {
            float healthAtWave10 = startEnemyHealth + (difficultyRampWave - 1) * healthGrowthPerWave;
            float healthAtWave25 = healthAtWave10 + (difficultyRampWave25 - difficultyRampWave) * healthGrowthPerWaveAfter10;
            int extraWaves = waveNumber - difficultyRampWave25;
            return healthAtWave25 + extraWaves * healthGrowthPerWaveAfter25;
        }
    }

    private float GetDamageForWave(int waveNumber)
    {
        if (waveNumber <= difficultyRampWave)
        {
            return startEnemyDamage + (waveNumber - 1) * damageGrowthPerWave;
        }
        else if (waveNumber <= difficultyRampWave25)
        {
            float damageAtWave10 = startEnemyDamage + (difficultyRampWave - 1) * damageGrowthPerWave;
            int extraWaves = waveNumber - difficultyRampWave;
            return damageAtWave10 + extraWaves * damageGrowthPerWaveAfter10;
        }
        else
        {
            float damageAtWave10 = startEnemyDamage + (difficultyRampWave - 1) * damageGrowthPerWave;
            float damageAtWave25 = damageAtWave10 + (difficultyRampWave25 - difficultyRampWave) * damageGrowthPerWaveAfter10;
            int extraWaves = waveNumber - difficultyRampWave25;
            return damageAtWave25 + extraWaves * damageGrowthPerWaveAfter25;
        }
    }

    private float GetMoveSpeedForWave(int waveNumber)
    {
        if (waveNumber <= difficultyRampWave)
        {
            return startEnemyMoveSpeed + (waveNumber - 1) * moveSpeedGrowthPerWave;
        }
        else if (waveNumber <= difficultyRampWave25)
        {
            float speedAtWave10 = startEnemyMoveSpeed + (difficultyRampWave - 1) * moveSpeedGrowthPerWave;
            int extraWaves = waveNumber - difficultyRampWave;
            return speedAtWave10 + extraWaves * moveSpeedGrowthPerWaveAfter10;
        }
        else
        {
            float speedAtWave10 = startEnemyMoveSpeed + (difficultyRampWave - 1) * moveSpeedGrowthPerWave;
            float speedAtWave25 = speedAtWave10 + (difficultyRampWave25 - difficultyRampWave) * moveSpeedGrowthPerWaveAfter10;
            int extraWaves = waveNumber - difficultyRampWave25;
            return speedAtWave25 + extraWaves * moveSpeedGrowthPerWaveAfter25;
        }
    }
    public void NotifyEnemyDied()
    {
        aliveEnemiesInWave = Mathf.Max(0, aliveEnemiesInWave - 1);
    }
}