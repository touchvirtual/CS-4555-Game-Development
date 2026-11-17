
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 2;
    public GameObject deathVfxPrefab;
    public int scoreValue = 1;
    public LevelManager levelManager;

    public int CurrentHealth { get; private set; }

    void Awake()
    {
        CurrentHealth = maxHealth;

        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();
    }

    public void TakeDamage(int amount)
    {
        if (CurrentHealth <= 0 || amount <= 0)
            return;

        CurrentHealth -= amount;

        // Check this enemy and its parent objects for BossAI.
        BossAI boss = GetComponentInParent<BossAI>();

        if (boss != null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                BossPowerUp powerUp =
                    player.GetComponent<BossPowerUp>();

                if (powerUp != null)
                {
                    powerUp.AddCharge(25f);
                    Debug.Log("Boss damaged! Vehicle charge increased.");
                }
                else
                {
                    Debug.LogError("BossPowerUp missing from Player!");
                }
            }
        }

        if (CurrentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (deathVfxPrefab != null)
        {
            Instantiate(
                deathVfxPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyDefeated(scoreValue);

        if (levelManager != null)
            levelManager.EnemyDefeatedForLevel();

        Destroy(gameObject);
    }
}
