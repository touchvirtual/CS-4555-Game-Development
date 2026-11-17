
using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    [Header("Player Detection")]
    public Transform player;
    public float detectionRange = 15f;

    [Header("Attack Settings")]
    public float attackRange = 10f;
    public int attackDamage = 2;
    public float attackCooldown = 3f;

    [Header("Attack Warning")]
    public BossAttackWarning attackWarning;

    private PlayerHealth playerHealth;
    private bool isAttacking;
    private bool bossBattleStarted;
    private float nextAttackTime;

    private void Start()
    {
        FindPlayer();

        if (attackWarning == null)
            attackWarning = GetComponent<BossAttackWarning>();

        if (attackWarning == null)
            Debug.LogWarning("BossAttackWarning component is missing!");
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            playerHealth =
                playerObject.GetComponent<PlayerHealth>();

            Debug.Log("Boss found the persistent Player!");
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsPaused)
            return;

        // Find the persistent Player if necessary.
        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (isAttacking)
            return;

        if (playerHealth == null)
        {
            playerHealth =
                player.GetComponent<PlayerHealth>();
        }

        if (playerHealth == null || playerHealth.IsDead)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Start the boss battle and player power-up once.
        if (!bossBattleStarted &&
            distance <= detectionRange)
        {
            bossBattleStarted = true;

            Debug.Log("Boss battle started!");

            BossPowerUp powerUp =
                player.GetComponent<BossPowerUp>();

            if (powerUp != null)
            {
                powerUp.StartBossBattle();
            }
            else
            {
                Debug.LogWarning(
                    "BossPowerUp component is missing from Player!"
                );
            }
        }

        // Attack when the player is within attack range.
        if (distance <= detectionRange &&
            distance <= attackRange &&
            Time.time >= nextAttackTime)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;

        // Show the red warning zone before attacking.
        if (attackWarning != null)
        {
            yield return StartCoroutine(
                attackWarning.ShowWarning()
            );
        }

        // Damage the player if they are still in range.
        if (player != null &&
            playerHealth != null &&
            !playerHealth.IsDead)
        {
            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            if (distance <= attackRange)
            {
                playerHealth.TakeDamage(attackDamage);

                Debug.Log("Dragon attacked the Player!");
            }
        }

        nextAttackTime = Time.time + attackCooldown;

        isAttacking = false;
    }
}
