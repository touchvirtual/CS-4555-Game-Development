
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    public bool IsDead => CurrentHealth <= 0;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        UpdateHealthUI();

        if (IsDead)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        CurrentHealth = Mathf.Min(
            CurrentHealth + amount,
            MaxHealth
        );

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(
                CurrentHealth,
                MaxHealth
            );
        }
    }
}
