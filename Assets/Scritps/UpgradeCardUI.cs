using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardUI : MonoBehaviour
{
    public Image cardBackground;
    public TMP_Text iconText;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text amountText;
    public Button selectButton;

    public Color accentColor = Color.cyan;

    private int upgradeIndex;

    public void Setup(UpgradeData upgrade, int index)
    {
        upgradeIndex = index;

        if (upgrade == null) return;

        if (nameText != null)
            nameText.text = upgrade.upgradeName;

        if (descriptionText != null)
            descriptionText.text = upgrade.description;

        if (amountText != null)
            amountText.text = GetAmountText(upgrade);

        if (cardBackground != null)
            cardBackground.color = new Color(
                accentColor.r * 0.22f,
                accentColor.g * 0.22f,
                accentColor.b * 0.22f,
                1f
            );

        if (selectButton != null)
        {
            Image buttonImage = selectButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = accentColor;

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(SelectUpgrade);
        }
    }

    private string GetAmountText(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.ExplosionDamage:
                return "+" + Mathf.Max(1, Mathf.RoundToInt(upgrade.amount)) + " DAMAGE";

            case UpgradeType.ExplosionRadius:
                return "+" + upgrade.amount + " RADIUS";

            case UpgradeType.MoveSpeed:
                return "+" + upgrade.amount + " SPEED";

            case UpgradeType.MaxHealth:
                return "+" + Mathf.Max(1, Mathf.RoundToInt(upgrade.amount)) + " MAX HP";

            case UpgradeType.Stealth:
                return "+" + Mathf.RoundToInt(upgrade.amount) + " STEALTH";

            case UpgradeType.ExplosionCooldown:
                return "-" + upgrade.amount + "s COOLDOWN";

            default:
                return "";
        }
    }

    private void SelectUpgrade()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ChooseUpgrade(upgradeIndex);
    }
}
