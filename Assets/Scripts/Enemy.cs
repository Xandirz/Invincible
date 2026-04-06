using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 30f;
    public float currentHealth = 30f;
    public float moveSpeed = 2f;
    public float contactDamage = 5f;

    [Header("Popup")]
    public Canvas popupCanvas;
    public GameObject damagePopupPrefab;

    [Header("Sprites")]
    public Sprite[] randomGoblinSprites;
    public Sprite goblinElectrocutedSprite;

    [Header("Electric Effect")]
    public float electrocutedDuration = 1f;

    private EnemySpawner spawner;
    private Transform playerTarget;
    private SpriteRenderer spriteRenderer;
    private Sprite normalSprite;
    private Coroutine electrocutedCoroutine;
    private bool isElectrocuted;

    private void Start()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();

        SetRandomStartSprite();

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

        if (isElectrocuted)
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

    public void TakeElectricDamage(float amount)
    {
        if (amount <= 0f)
            return;

        currentHealth -= amount;

        DamagePopup.Create(
            popupCanvas,
            damagePopupPrefab,
            transform.position,
            amount,
            Color.cyan
        );

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (electrocutedCoroutine != null)
            StopCoroutine(electrocutedCoroutine);

        electrocutedCoroutine = StartCoroutine(ElectrocutedRoutine());
    }

    private IEnumerator ElectrocutedRoutine()
    {
        isElectrocuted = true;

        if (spriteRenderer != null && goblinElectrocutedSprite != null)
            spriteRenderer.sprite = goblinElectrocutedSprite;

        yield return new WaitForSeconds(electrocutedDuration);

        if (spriteRenderer != null)
            spriteRenderer.sprite = normalSprite;

        isElectrocuted = false;
        electrocutedCoroutine = null;
    }

    private void SetRandomStartSprite()
    {
        if (spriteRenderer == null)
            return;

        if (randomGoblinSprites != null && randomGoblinSprites.Length > 0)
        {
            int index = Random.Range(0, randomGoblinSprites.Length);
            Sprite selectedSprite = randomGoblinSprites[index];

            if (selectedSprite != null)
            {
                spriteRenderer.sprite = selectedSprite;
                normalSprite = selectedSprite;
                return;
            }
        }

        normalSprite = spriteRenderer.sprite;
    }

    private void Die()
    {
        if (EnemyDeathRewardManager.Instance != null)
            EnemyDeathRewardManager.Instance.RegisterEnemyDeath(transform.position);

        if (spawner != null)
            spawner.NotifyEnemyDied();
        AudioManager.Instance?.PlaySfx(GameSound.EnemyDied);
        Destroy(gameObject);
        
    }
}