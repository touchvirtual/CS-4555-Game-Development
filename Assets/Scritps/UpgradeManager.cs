using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    public List<UpgradeData> availableUpgrades = new List<UpgradeData>();
    public UpgradeData[] CurrentChoices { get; private set; } = new UpgradeData[3];

    void Awake() { Instance = this; }

    public void ShowChoices()
    {
        if (availableUpgrades.Count == 0)
        {
            Debug.LogWarning("Add UpgradeData assets to UpgradeManager.");
            if (UIManager.Instance != null) UIManager.Instance.ShowLevelComplete();
            return;
        }

        List<UpgradeData> pool = new List<UpgradeData>(availableUpgrades);
        for (int i = 0; i < CurrentChoices.Length; i++)
        {
            if (pool.Count == 0) pool = new List<UpgradeData>(availableUpgrades);
            int index = Random.Range(0, pool.Count);
            CurrentChoices[i] = pool[index];
            pool.RemoveAt(index);
        }

        if (UpgradeSelectionUI.Instance != null)
        {
            UpgradeSelectionUI.Instance.ShowChoices(CurrentChoices);
        }
        else
        {
            Debug.LogError("UpgradeSelectionUI was not found!");
        }
    }

    public void ChooseUpgrade(int index)
    {
        if (index < 0 || index >= CurrentChoices.Length || CurrentChoices[index] == null)
            return;

        ApplyUpgrade(CurrentChoices[index]);

        // Hide the new three-card upgrade UI
        if (UpgradeSelectionUI.Instance != null)
            UpgradeSelectionUI.Instance.HideChoices();

        // Continue to the next level
        LevelManager level = FindObjectOfType<LevelManager>();

        if (level != null)
            level.ContinueAfterUpgrade();
        else if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);
    }

    void ApplyUpgrade(UpgradeData data)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        ExplosionController boom = player.GetComponent<ExplosionController>();
        PlayerController movement = player.GetComponent<PlayerController>();
        PlayerStealth stealth = player.GetComponent<PlayerStealth>();

        switch (data.type)
        {
            case UpgradeType.ExplosionDamage:
                if (boom != null) boom.damage += Mathf.Max(1, Mathf.RoundToInt(data.amount));
                break;
            case UpgradeType.ExplosionRadius:
                if (boom != null) boom.radius += data.amount;
                break;
            case UpgradeType.MoveSpeed:
                if (movement != null) movement.moveSpeed += data.amount;
                break;
            case UpgradeType.MaxHealth:
                if (health != null) { health.maxHealth += Mathf.Max(1, Mathf.RoundToInt(data.amount)); health.Heal(Mathf.RoundToInt(data.amount)); }
                break;
            case UpgradeType.Stealth:
                if (stealth != null) stealth.ActivateStealth();
                break;
            case UpgradeType.ExplosionCooldown:
                if (boom != null) boom.cooldown = Mathf.Max(0.2f, boom.cooldown - data.amount);
                break;
        }
    }
}
