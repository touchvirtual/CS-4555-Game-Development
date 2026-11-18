using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleShooter : MonoBehaviour
{
    [Header("Vehicle")]
    public GameObject vehicleModel;
    public Transform firePoint;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float fireCooldown = 0.5f;

    private float nextFireTime;

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsPaused)
            return;

        // Only shoot while the vehicle is active.
        if (vehicleModel == null ||
            !vehicleModel.activeInHierarchy)
            return;

        if (Keyboard.current == null)
            return;

        // Press E to shoot.
        if (Keyboard.current.eKey.wasPressedThisFrame &&
            Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (firePoint == null || projectilePrefab == null)
        {
            Debug.LogWarning(
                "Vehicle shooting references are missing!"
            );
            return;
        }

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Prevent the projectile from hitting the Player.
        Collider projectileCollider =
            projectile.GetComponent<Collider>();

        Collider[] playerColliders =
            GetComponentsInChildren<Collider>();

        if (projectileCollider != null)
        {
            foreach (Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(
                    projectileCollider,
                    playerCollider
                );
            }
        }

        nextFireTime = Time.time + fireCooldown;

        Debug.Log("Vehicle fired projectile!");
    }
}
