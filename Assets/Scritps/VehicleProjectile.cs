using UnityEngine;

public class VehicleProjectile : MonoBehaviour
{
    public float damage = 5f;
    public float speed = 20f;
    public float lifetime = 4f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (rb != null)
            rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(Mathf.RoundToInt(damage));
            Destroy(gameObject);
        }
    }
}
