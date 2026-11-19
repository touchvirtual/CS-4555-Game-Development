using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>In-session checkpoints. Snapshots intentionally do not write PlayerPrefs or save files.</summary>
public static class BombaCatCheckpointService
{
    private sealed class Snapshot
    {
        public string scenePath, checkpointId;
        public Vector3 position;
        public Quaternion rotation;
        public float moveSpeed, jumpHeight, radius, cooldown, knockback, stealthDuration;
        public int damage, maxHealth, score, defeatedForLevel;
        public bool stealthUnlocked;
        public HashSet<string> defeated = new HashSet<string>(StringComparer.Ordinal);

        public Snapshot Copy()
        {
            var copy = (Snapshot)MemberwiseClone();
            copy.defeated = new HashSet<string>(defeated, StringComparer.Ordinal);
            return copy;
        }
    }

    private static Snapshot levelStart, checkpoint, pendingRestore;
    private static readonly HashSet<string> defeated = new HashSet<string>(StringComparer.Ordinal);
    private static string currentScene;
    public static PlayerController ActivePlayer { get; private set; }
    public static bool IsReloading { get; private set; }
    public static string StatusMessage { get; private set; } = "";
    public static float StatusUntil { get; private set; }
    public static bool HasCheckpoint => checkpoint != null && !string.IsNullOrEmpty(checkpoint.checkpointId);
    public static string CurrentCheckpointId => checkpoint != null ? checkpoint.checkpointId : "";
    public static Vector3 CheckpointPosition => checkpoint != null ? checkpoint.position : Vector3.zero;
    public static Vector3 LevelStartPosition => levelStart != null ? levelStart.position : Vector3.zero;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { ClearRun(); }

    public static void ClearRun()
    {
        levelStart = checkpoint = pendingRestore = null;
        ActivePlayer = null;
        currentScene = null;
        defeated.Clear();
        IsReloading = false;
        StatusMessage = "";
        StatusUntil = 0f;
    }

    public static PlayerController ResolvePlayer()
    {
        var players = UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        if (ActivePlayer == null)
        {
            // A reloaded Level1 briefly contains its newly authored player as well as the
            // persistent cat. Prefer the persistent object, even before delayed destruction.
            foreach (var player in players)
                if (player.gameObject.scene.name == "DontDestroyOnLoad") { ActivePlayer = player; break; }
            if (ActivePlayer == null && players.Length > 0) ActivePlayer = players[0];
        }
        foreach (var player in players)
        {
            if (player == ActivePlayer) continue;
            player.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(player.transform.root.gameObject);
        }
        return ActivePlayer;
    }

    public static void OnSceneReady(Scene scene, PlayerController player, LevelManager level)
    {
        if (player == null) return;
        ActivePlayer = player;
        string sceneKey = SceneKey(scene);
        var enemies = UnityEngine.Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        Snapshot restoring = pendingRestore != null && pendingRestore.scenePath == sceneKey ? pendingRestore : null;
        currentScene = sceneKey;
        defeated.Clear();

        if (restoring != null)
        {
            foreach (string id in restoring.defeated) defeated.Add(id);
            RestorePlayer(player, restoring);
            if (level != null) level.RestoreProgress(restoring.defeatedForLevel);
            if (GameManager.Instance != null) GameManager.Instance.RestoreEnemyCount(restoring.score);
            foreach (var enemy in enemies)
                if (enemy.gameObject.scene == scene && defeated.Contains(enemy.CheckpointId))
                    enemy.RestoreDefeatedState();
            ShowStatus(string.IsNullOrEmpty(restoring.checkpointId) ? "AREA RESTARTED" : "BACK AT THE CHECKPOINT");
        }
        else
        {
            // The second scene has no player/spawn authored in it. Preserve the entrance
            // reached through the portal, placing the cat on this scene's supporting floor.
            PlacePlayer(player, GroundPosition(player, scene), player.transform.rotation);
            player.ResetForRespawn();
            var boom = player.GetComponent<ExplosionController>();
            if (boom != null) boom.ResetCooldown();
            var stealth = player.GetComponent<PlayerStealth>();
            if (stealth != null) stealth.ResetAbilityState();
            if (GameManager.Instance != null) GameManager.Instance.RestoreEnemyCount(0);
            if (level != null) level.RestoreProgress(0);
            levelStart = Capture(player, level, "");
            checkpoint = levelStart.Copy();
        }

        pendingRestore = null;
        IsReloading = false;
        player.enabled = true;
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            if (level != null) enemy.levelManager = level;
            var ai = enemy.GetComponent<EnemyAI>();
            if (ai != null) ai.target = player.transform;
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideAllPanels();
            var health = player.GetComponent<PlayerHealth>();
            if (health != null) UIManager.Instance.UpdateHealth(health.CurrentHealth, health.MaxHealth);
        }
        if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
        BombaCatCheckpoint.InstallForScene(scene);
    }

    public static bool SaveCheckpoint(string id, PlayerController player)
    {
        if (player == null || IsReloading || string.IsNullOrEmpty(id)) return false;
        var health = player.GetComponent<PlayerHealth>();
        if (health != null && health.IsDead) return false;
        if (currentScene != SceneKey(SceneManager.GetActiveScene())) return false;
        if (checkpoint != null && checkpoint.checkpointId == id) return false;
        checkpoint = Capture(player, UnityEngine.Object.FindFirstObjectByType<LevelManager>(), id);
        ShowStatus("CHECKPOINT SAVED");
        return true;
    }

    public static bool ReloadCheckpoint() { return Reload(checkpoint ?? levelStart, false); }
    public static bool ReloadLevelStart() { return Reload(levelStart, true); }

    private static bool Reload(Snapshot snapshot, bool clearCheckpoint)
    {
        if (IsReloading || snapshot == null || string.IsNullOrEmpty(snapshot.scenePath)) return false;
        IsReloading = true;
        pendingRestore = snapshot.Copy();
        if (clearCheckpoint) checkpoint = snapshot.Copy();
        if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
        // Keep the persistent cat alive: Level2 has no replacement player in its scene.
        if (ActivePlayer != null) ActivePlayer.enabled = false;
        try { SceneManager.LoadScene(snapshot.scenePath, LoadSceneMode.Single); }
        catch (Exception exception)
        {
            pendingRestore = null;
            IsReloading = false;
            if (ActivePlayer != null) ActivePlayer.enabled = true;
            Debug.LogException(exception);
            return false;
        }
        return true;
    }

    private static Snapshot Capture(PlayerController player, LevelManager level, string id)
    {
        var health = player.GetComponent<PlayerHealth>();
        var boom = player.GetComponent<ExplosionController>();
        var stealth = player.GetComponent<PlayerStealth>();
        return new Snapshot
        {
            scenePath = SceneKey(SceneManager.GetActiveScene()), checkpointId = id,
            position = player.transform.position, rotation = player.transform.rotation,
            moveSpeed = player.moveSpeed, jumpHeight = player.jumpHeight,
            maxHealth = health != null ? health.maxHealth : 5,
            radius = boom != null ? boom.radius : 4f,
            damage = boom != null ? boom.damage : 1,
            cooldown = boom != null ? boom.cooldown : 2f,
            knockback = boom != null ? boom.knockbackForce : 8f,
            stealthDuration = stealth != null ? stealth.stealthDuration : 5f,
            stealthUnlocked = stealth != null && stealth.HasStealth,
            score = GameManager.Instance != null ? GameManager.Instance.defeatedEnemies : 0,
            defeatedForLevel = level != null ? level.DefeatedThisLevel : defeated.Count,
            defeated = new HashSet<string>(defeated, StringComparer.Ordinal)
        };
    }

    private static void RestorePlayer(PlayerController player, Snapshot snapshot)
    {
        player.moveSpeed = snapshot.moveSpeed;
        player.jumpHeight = snapshot.jumpHeight;
        var boom = player.GetComponent<ExplosionController>();
        if (boom != null)
        {
            boom.radius = snapshot.radius; boom.damage = snapshot.damage;
            boom.cooldown = snapshot.cooldown; boom.knockbackForce = snapshot.knockback;
            boom.ResetCooldown();
        }
        var stealth = player.GetComponent<PlayerStealth>();
        if (stealth != null)
        {
            stealth.stealthDuration = snapshot.stealthDuration;
            stealth.RestoreUnlock(snapshot.stealthUnlocked);
            stealth.ResetAbilityState();
        }
        PlacePlayer(player, snapshot.position, snapshot.rotation);
        player.ResetForRespawn();
        var health = player.GetComponent<PlayerHealth>();
        if (health != null) health.RestoreForRespawn(snapshot.maxHealth);
    }

    private static void PlacePlayer(PlayerController player, Vector3 position, Quaternion rotation)
    {
        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        player.transform.SetPositionAndRotation(position, rotation);
        if (controller != null) controller.enabled = true;
        Physics.SyncTransforms();
    }

    private static Vector3 GroundPosition(PlayerController player, Scene scene)
    {
        Vector3 position = player.transform.position;
        Physics.SyncTransforms();
        RaycastHit[] hits = Physics.RaycastAll(position + Vector3.up * 8f, Vector3.down, 48f, ~0, QueryTriggerInteraction.Ignore);
        float nearest = float.PositiveInfinity;
        Vector3 floor = position;
        bool found = false;
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.scene != scene || hit.normal.y < 0.6f ||
                hit.collider.GetComponentInParent<EnemyHealth>() != null ||
                hit.collider.GetComponentInParent<PlayerController>() != null) continue;
            float difference = Mathf.Abs(hit.point.y - position.y);
            if (difference >= nearest) continue;
            nearest = difference; floor = hit.point; found = true;
        }
        if (!found) return position;
        var controller = player.GetComponent<CharacterController>();
        float clearance = controller != null
            ? (controller.height * 0.5f - controller.center.y + controller.skinWidth) * Mathf.Abs(player.transform.lossyScale.y)
            : 0.1f;
        return new Vector3(position.x, floor.y + Mathf.Max(0.03f, clearance), position.z);
    }

    public static void RecordEnemyDefeated(EnemyHealth enemy)
    {
        if (enemy != null && enemy.gameObject.scene.path == SceneManager.GetActiveScene().path)
            defeated.Add(enemy.CheckpointId);
    }

    public static string GetEnemyId(EnemyHealth enemy, string explicitId)
    {
        string prefix = SceneKey(enemy.gameObject.scene) + ":";
        if (!string.IsNullOrEmpty(explicitId)) return prefix + "id/" + explicitId;
        var parts = new List<string>();
        for (Transform item = enemy.transform; item != null; item = item.parent)
        {
            int ordinal = 0;
            if (item.parent != null)
            {
                for (int i = 0; i < item.GetSiblingIndex(); i++)
                    if (item.parent.GetChild(i).name == item.name) ordinal++;
            }
            else
            {
                foreach (var root in item.gameObject.scene.GetRootGameObjects())
                    if (root.transform.GetSiblingIndex() < item.GetSiblingIndex() && root.name == item.name) ordinal++;
            }
            parts.Add(item.name.Replace("/", "%2F") + "[" + ordinal + "]");
        }
        parts.Reverse();
        return prefix + string.Join("/", parts);
    }

    private static string SceneKey(Scene scene) { return string.IsNullOrEmpty(scene.path) ? scene.name : scene.path; }
    private static void ShowStatus(string message) { StatusMessage = message; StatusUntil = Time.unscaledTime + 3.5f; }
}
