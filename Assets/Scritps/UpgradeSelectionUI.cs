using UnityEngine;
using TMPro;
public class UpgradeSelectionUI : MonoBehaviour
{
    public static UpgradeSelectionUI Instance { get; private set; }
    public GameObject upgradePanel;
    public TMP_Text titleText, subtitleText;
    public UpgradeCardUI[] cards;
    private void Awake() { Instance = this; if (upgradePanel != null) upgradePanel.SetActive(false); }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    public void ShowChoices(UpgradeData[] choices)
    {
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (UIManager.Instance != null) UIManager.Instance.ShowUpgradeChoices(choices);
    }
    public void HideChoices()
    {
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (UIManager.Instance != null) UIManager.Instance.HideUpgradePanel();
    }
}
