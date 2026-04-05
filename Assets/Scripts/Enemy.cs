using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 30f;
    public float currentHealth = 30f;
    public float moveSpeed = 2f;
    public float contactDamage = 5f;
    private EnemySpawner spawner;
    [Header("Popup")]
    public Canvas popupCanvas;
    public GameObject damagePopupPrefab;

    private Transform playerTarget;

    private void Start()
    {
        currentHealth = maxHealth;
        
        if (popupCanvas == null)
            popupCanvas = FindObjectOfType<Canvas>();
    

        PlayerCombat player = FindObjectOfType<PlayerCombat>();
        if (player != null)
            playerTarget = player.transform;
    }
    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }
    private void Update()
    {
        if (playerTarget == null)
            return;
        

        transform.position = Vector3.MoveTowards(
            transform.position,
            playerTarget.position,
            moveSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null)
            stats = other.GetComponentInParent<PlayerStats>();

        if (stats == null)
            return;

        if (stats.HasShield())
            return;

        stats.TakeDamage(contactDamage);
        Die();
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f)
            return;

        currentHealth -= amount;

        DamagePopup.Create(
            popupCanvas,
            damagePopupPrefab,
            transform.position,
            amount,
            Color.yellow
        );

        if (currentHealth <= 0f)
            Die();
    }
    
    public void TakeDamageShield(float amount)
    {
        if (amount <= 0f)
            return;

        currentHealth -= amount;

 

        if (currentHealth <= 0f)
            Die();
    }
    
    private void Die()
    {
        if (EnemyDeathRewardManager.Instance != null)
            EnemyDeathRewardManager.Instance.RegisterEnemyDeath(transform.position);

        if (spawner != null)
            spawner.NotifyEnemyDied();

        Destroy(gameObject);
    }
}