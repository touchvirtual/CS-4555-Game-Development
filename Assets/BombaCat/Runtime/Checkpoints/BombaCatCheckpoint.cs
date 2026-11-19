using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>One-shot, grounded proximity checkpoint beside the first red oil barrel.</summary>
public sealed class BombaCatCheckpoint : MonoBehaviour
{
    public string checkpointId = "level1-first-red-barrel";
    [Min(0.5f)] public float activationRadius = 5f;
    [Min(0.5f)] public float verticalTolerance = 4f;

    private void Update()
    {
        var player = BombaCatCheckpointService.ActivePlayer;
        if (player == null || !player.IsGrounded || BombaCatCheckpointService.CurrentCheckpointId == checkpointId ||
            (GameManager.Instance != null && GameManager.Instance.IsPaused)) return;
        Vector3 offset = player.transform.position - transform.position;
        if (Mathf.Abs(offset.y) > verticalTolerance) return;
        offset.y = 0f;
        if (offset.sqrMagnitude <= activationRadius * activationRadius)
            BombaCatCheckpointService.SaveCheckpoint(checkpointId, player);
    }

    public static void InstallForScene(Scene scene)
    {
        if (scene.name != "Level1")
        {
            if (Object.FindFirstObjectByType<BombaCatCheckpointBeacon>() == null)
            {
                var entrance = new GameObject("BombaCat Area Checkpoint");
                entrance.transform.position = BombaCatCheckpointService.LevelStartPosition;
                entrance.AddComponent<BombaCatCheckpointBeacon>().isEntrance = true;
            }
            return;
        }
        if (Object.FindFirstObjectByType<BombaCatCheckpoint>() != null) return;
        // Source scene: OilObstacle/Barrel (1), world (-429.487, 21.298, 11.748).
        // This is the first barrel on the route from the small dogs toward the golems.
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name != "OilObstacle") continue;
            Transform barrel = null;
            foreach (var child in root.GetComponentsInChildren<Transform>())
                if (child.name == "Barrel (1)") { barrel = child; break; }
            if (barrel == null) continue;
            var marker = new GameObject("BombaCat Checkpoint - First Red Barrel");
            marker.transform.SetParent(root.transform, true);
            marker.transform.position = barrel.position;
            marker.AddComponent<BombaCatCheckpoint>();
            marker.AddComponent<BombaCatCheckpointBeacon>();
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.35f, 1f, 0.75f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
