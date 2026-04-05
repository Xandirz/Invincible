using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseAttackSpeed = 1f;
    public float attackRange = 8f;

    [Header("Runtime Stats")]
    public float damage;
    public float attackSpeed;
    public float invisibilityShield;
    public float projectileCount = 1f;
    
    [Header("Bonuses")]
    public float damageBonus;
    public float attackSpeedBonus;
    public float projectileCountBonus;
    
    [Header("Shield")]
    public PlayerShield playerShield;

    [Header("Popup")]
    public Canvas popupCanvas;
    public GameObject damagePopupPrefab;

    private void Awake()
    {
        currentHealth = maxHealth;
        RecalculateStats();

        if (popupCanvas == null)
            popupCanvas = FindObjectOfType<Canvas>();

        UpdateShieldVisual();
    }

    public void RecalculateStats()
    {
        damage = baseDamage + damageBonus;
        attackSpeed = baseAttackSpeed + attackSpeedBonus;
        projectileCount = Mathf.Max(1f, 1f + projectileCountBonus);
    }

    public void SetGeneratorBonus(GeneratedStatType statType, float value)
    {
        switch (statType)
        {
            case GeneratedStatType.Damage:
                damageBonus = value;
                RecalculateStats();
                break;

            case GeneratedStatType.AttackSpeed:
                attackSpeedBonus = value;
                RecalculateStats();
                break;

            case GeneratedStatType.InvisibilityShield:
                invisibilityShield = Mathf.Max(0f, value);
                UpdateShieldVisual();
                break;
            
            case GeneratedStatType.ProjectileCount:
                projectileCountBonus = value;
                RecalculateStats();
                break;
        }
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
            Color.red
        );

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public bool HasShield()
    {
        return invisibilityShield >= 1f;
    }

    public bool TryAbsorbDamageWithShield(float damageAmount)
    {
        if (damageAmount <= 0f || invisibilityShield <= 0f)
            return false;

        invisibilityShield -= damageAmount;

        DamagePopup.Create(
            popupCanvas,
            damagePopupPrefab,
            transform.position,
            damageAmount,
            new Color(0.3f, 0.8f, 1f)
        );

        if (invisibilityShield < 0f)
            invisibilityShield = 0f;

        UpdateShieldVisual();
        return true;
    }

    public void UpdateShieldVisual()
    {
        if (playerShield != null)
            playerShield.SetActiveVisual(HasShield());
    }

    private void Die()
    {
        Debug.Log("Player died");
    }
}