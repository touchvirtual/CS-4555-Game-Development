
using UnityEngine;

public class PortalSpawner : MonoBehaviour
{
    public GameObject portal;

    private bool portalShown = false;

    private void Start()
    {
        if (portal != null)
        {
            portal.SetActive(false);
        }

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.OnDialogueCompleted += ShowPortal;
        }
    }

    private void OnDestroy()
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.OnDialogueCompleted -= ShowPortal;
        }
    }

    private void ShowPortal()
    {
        if (portalShown || portal == null) return;

        portalShown = true;
        portal.SetActive(true);
    }
}
