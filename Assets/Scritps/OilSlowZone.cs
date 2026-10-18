
using UnityEngine;

public class OilSlowZone : MonoBehaviour
{
    [Range(0.1f, 1f)]
    public float speedMultiplier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.SetOilSlow(speedMultiplier);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.SetOilSlow(1f);
        }
    }
}
