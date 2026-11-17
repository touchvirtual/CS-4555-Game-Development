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
        Debug.Log("Upgrade button clicked. Index: " + index);

        if (index < 0 || index >= CurrentChoices.Length || CurrentChoices[index] == null)
        {
            Debug.LogError("Invalid upgrade selection!");
            return;
        }

        ApplyUpgrade(CurrentChoices[index]);

        if (UpgradeSelectionUI.Instance != null)
            UpgradeSelectionUI.Instance.HideChoices();

        LevelManager level = FindObjectOfType<LevelManager>();

        if (level != null)
        {
            Debug.Log("Loading next level...");
            level.ContinueAfterUpgrade();
        }
        else
        {
            Debug.LogError("LevelManager was not found!");
            if (GameManager.Instance != null)
                GameManager.Instance.SetPaused(false);
        }
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
                if (health != null)
                {
                    health.maxHealth += Mathf.RoundToInt(data.amount);
                    health.ResetHealthToMax();
                }
                break;
            case UpgradeType.Stealth:
                if (stealth != null)
                {
                    stealth.stealthDuration = data.amount;
                    stealth.UnlockStealth();
                }
                else
                {
                    Debug.LogWarning("PlayerStealth component is missing from Player!");
                }
                break;
            case UpgradeType.ExplosionCooldown:
                if (boom != null) boom.cooldown = Mathf.Max(0.2f, boom.cooldown - data.amount);
                break;
        }
    }
}
