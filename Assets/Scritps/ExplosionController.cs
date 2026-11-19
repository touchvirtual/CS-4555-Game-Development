using UnityEngine;
using System.Collections;

public class ExplosionController : MonoBehaviour
{
    public float radius = 4f;
    public int damage = 1;
    public float cooldown = 2f;
    public float knockbackForce = 8f;
    public GameObject explosionVfxPrefab;
    public LayerMask targetLayers = ~0;

    private float nextReadyTime;
    private PlayerHealth playerHealth;

    public float CooldownRemaining => Mathf.Max(0f, nextReadyTime - Time.time);
    public float CooldownNormalized => cooldown > 0f ? Mathf.Clamp01(CooldownRemaining / cooldown) : 0f;
    public bool IsReady => CooldownRemaining <= 0f;
    public void ResetCooldown() { nextReadyTime = 0f; }

    void Awake() { playerHealth = GetComponent<PlayerHealth>(); }

    public void TryExplode(){ var vehicle=GetComponent<BossPowerUp>(); if(vehicle!=null && vehicle.VehicleActivated) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        if (Time.time < nextReadyTime) return;
        if (playerHealth != null && playerHealth.IsDead) return;
        nextReadyTime = Time.time + cooldown;

        if (explosionVfxPrefab != null)
        {
            GameObject effect = Instantiate(
                explosionVfxPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(effect, 1f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayers);
        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform.root) continue;
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Rigidbody rb = hit.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 direction = (rb.worldCenterOfMass - transform.position).normalized;
                    rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.35f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}

