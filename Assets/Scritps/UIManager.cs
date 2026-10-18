using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public TMP_Text healthText;
    public TMP_Text enemyCountText;
    public TMP_Text statusText;
    public TMP_Text bossHealthText;
    public Slider bossMeterSlider;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject levelCompletePanel;
    public GameObject upgradePanel;
    public Button[] upgradeButtons;
    public TMP_Text[] upgradeButtonLabels;

    void Awake() { Instance = this; }

    void Start()
    {
        HideAllPanels();
        if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null) healthText.text = "Health: " + current + " / " + max;
    }

    public void UpdateEnemyCount(int count)
    {
        if (enemyCountText != null) enemyCountText.text = "Enemies defeated: " + count;
    }

    public void UpdateBossHealth(int current, int max)
    {
        if (bossHealthText != null) bossHealthText.text = "Boss: " + current + " / " + max;
    }

    public void UpdateBossMeter(float normalized)
    {
        if (bossMeterSlider != null) bossMeterSlider.value = Mathf.Clamp01(normalized);
    }

    public void ShowGameOver()
    {
        HideAllPanels();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ShowVictory()
    {
        HideAllPanels();
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void ShowLevelComplete()
    {
        HideAllPanels();
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
    }

    public void ShowUpgradeChoices(UpgradeData[] choices)
    {
        HideAllPanels();
        if (upgradePanel != null) upgradePanel.SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            if (upgradeButtonLabels != null && i < upgradeButtonLabels.Length && upgradeButtonLabels[i] != null)
                upgradeButtonLabels[i].text = choices[i].upgradeName + "\n" + choices[i].description;
            if (upgradeButtons != null && i < upgradeButtons.Length && upgradeButtons[i] != null)
            {
                int index = i;
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => UpgradeManager.Instance.ChooseUpgrade(index));
            }
        }
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    public void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    public void OnRestartButton()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    
}
