using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerShield : MonoBehaviour
{
    public PlayerStats playerStats;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();
    }

    public void SetActiveVisual(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null || playerStats == null)
            return;

        bool absorbed = playerStats.TryAbsorbDamageWithShield(enemy.contactDamage);
        if (!absorbed)
            return;

        DamagePopup.Create(
            playerStats.popupCanvas,
            playerStats.damagePopupPrefab,
            enemy.transform.position,
            enemy.contactDamage,
            new Color(0.7f, 0.3f, 1f)
        );

        Destroy(enemy.gameObject);
    }
}