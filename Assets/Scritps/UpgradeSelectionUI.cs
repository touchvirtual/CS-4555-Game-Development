using UnityEngine;
using TMPro;

public class UpgradeSelectionUI : MonoBehaviour
{
    public static UpgradeSelectionUI Instance { get; private set; }

    public GameObject upgradePanel;
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public UpgradeCardUI[] cards;

    private void Awake()
    {
        Instance = this;

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    private void Start()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    public void ShowChoices(UpgradeData[] choices)
    {
        if (upgradePanel == null || choices == null)
            return;

        upgradePanel.SetActive(true);

        if (titleText != null)
            titleText.text = "CHOOSE YOUR UPGRADE";

        if (subtitleText != null)
            subtitleText.text = "Pick one to improve your powers";

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null)
                continue;

            if (i < choices.Length && choices[i] != null)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(choices[i], i);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideChoices()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
}
