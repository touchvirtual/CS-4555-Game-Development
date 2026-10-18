using UnityEngine;

public enum UpgradeType { ExplosionDamage, ExplosionRadius, MoveSpeed, MaxHealth, Heal, ExplosionCooldown }

[CreateAssetMenu(fileName = "Upgrade", menuName = "Catastrophe/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName = "Bigger Boom";
    [TextArea] public string description = "Increase explosion radius.";
    public UpgradeType type = UpgradeType.ExplosionRadius;
    public float amount = 1f;
}
