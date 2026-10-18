using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 1;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        // Find PlayerHealth on the player or its parent.
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        // Do not collect the pickup if health is already full.
        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            return;

        // Heal the player.
        playerHealth.Heal(healAmount);

        // Remove the pickup after successful collection.
        collected = true;
        Destroy(gameObject);
    }
}
