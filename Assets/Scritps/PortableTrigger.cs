using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        if (UpgradeManager.Instance == null)
            return;

        activated = true;

        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);

        UpgradeManager.Instance.ShowChoices();
    }
}
