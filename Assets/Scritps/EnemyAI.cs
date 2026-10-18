using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    public Transform target;
    public float detectionRange = 12f;
    public float attackRange = 1.7f;
    public float moveSpeed = 3f;
    public int attackDamage = 1;
    public float attackCooldown = 1.5f;
    public float gravity = -20f;

    private CharacterController controller;
    private float nextAttackTime;
    private Vector3 verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        FindTarget();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        if (target == null) FindTarget();
        if (target == null) return;

        Vector3 delta = target.position - transform.position;
        delta.y = 0;
        float distance = delta.magnitude;

        if (distance <= detectionRange && distance > attackRange)
        {
            Vector3 direction = delta.normalized;
            controller.Move(direction * moveSpeed * Time.deltaTime);
            if (direction.sqrMagnitude > 0.01f)
                transform.forward = Vector3.Slerp(transform.forward, direction, 10f * Time.deltaTime);
        }
        else if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            PlayerHealth player = target.GetComponentInParent<PlayerHealth>();
            if (player != null) player.TakeDamage(attackDamage);
        }

        if (controller.isGrounded && verticalVelocity.y < 0) verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }
}
