using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public TMP_Text healthText, enemyCountText, statusText, bossHealthText;
    public Slider bossMeterSlider;
    public GameObject gameOverPanel, victoryPanel, levelCompletePanel, upgradePanel;
    public Button[] upgradeButtons;
    public TMP_Text[] upgradeButtonLabels;
    public GameObject endDemoPanel; private BombaCatHUD hud; public void ShowEndDemo(){HideLegacyPanels(); if(endDemoPanel!=null) endDemoPanel.SetActive(false); hud.ShowResult("DEMO COMPLETE","Thanks for playing!","You saved the village.",true);}
    private void Awake()
    {
        Instance = this;
        hud = gameObject.AddComponent<BombaCatHUD>();
        hud.Build(healthText != null ? healthText.font : TMP_Settings.defaultFontAsset);
        foreach (var label in new[] { healthText, enemyCountText, statusText, bossHealthText })
            if (label != null) label.enabled = false;
        if (bossMeterSlider != null) bossMeterSlider.gameObject.SetActive(false);
        HideLegacyPanels();
    }
    private void Start()
    {
        HideAllPanels();
        if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    private void HideLegacyPanels()
    {
        foreach (var panel in new[] { gameOverPanel, victoryPanel, levelCompletePanel, upgradePanel })
            if (panel != null && panel != gameObject) panel.SetActive(false);
    }
    public void UpdateHealth(int current, int max) { if (hud != null) hud.SetHealth(current, max); }
    public void UpdateEnemyCount(int count) { if (hud != null) hud.SetEnemyCount(count); }
    public void UpdateBossHealth(int current, int max) { if (hud != null) hud.SetBossHealth(current, max); }
    public void UpdateBossMeter(float normalized) { if (hud != null) hud.SetBossMeter(normalized); }
    public void ShowGameOver() { HideLegacyPanels(); hud.ShowResult("THE FUSE WENT OUT", "Cat down", "Pick up from your latest checkpoint.", false); }
    public void ShowVictory() { HideLegacyPanels(); hud.ShowResult("A PURRFECT FINISH", "Village saved!", "Small paws. Spectacular destruction.", true); }
    public void ShowLevelComplete() { HideLegacyPanels(); hud.ShowResult("AREA CLEARED", "Nicely done!", "Your next adventure is waiting.", true, true); }
    public void ShowUpgradeChoices(UpgradeData[] choices) { HideLegacyPanels(); if (hud != null) hud.ShowUpgrades(choices); }
    public void HideUpgradePanel() { if (upgradePanel != null) upgradePanel.SetActive(false); if (hud != null) hud.CloseModal(); }
    public void HideAllPanels() { HideLegacyPanels(); if (hud != null) hud.CloseModal(); }
    public void OnRestartButton() { if (GameManager.Instance != null) GameManager.Instance.RestartGame(); }
}

