using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Restores gameplay UI when the persistent cat enters the unfinished second scene.</summary>
public sealed class BombaCatSceneServices : MonoBehaviour
{
    private LevelManager generatedLevel;
    private bool finished;
    private static Scene preparedScene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        preparedScene = default;
        SceneManager.sceneLoaded -= EnsureServices;
        SceneManager.sceneLoaded += EnsureServices;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInitialScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (preparedScene != scene) EnsureServices(scene, LoadSceneMode.Single);
    }

    private static void EnsureServices(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;
        BombaCatDemoCollision.Install(scene);
        var player = BombaCatCheckpointService.ResolvePlayer();
        if (player == null) return;
        preparedScene = scene;
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();
        if (UIManager.Instance == null)
            new GameObject("UIManager").AddComponent<UIManager>();

        // Respect authored level rules. Supply an enemy-clearing goal only where none exist.
        var level = Object.FindFirstObjectByType<LevelManager>();
        var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        if (level == null && enemies.Length > 0)
        {
            var support = new GameObject("BombaCat Scene Support").AddComponent<BombaCatSceneServices>();
            support.generatedLevel = support.gameObject.AddComponent<LevelManager>();
            support.generatedLevel.StartLevel(scene.buildIndex + 1, enemies.Length);
            level = support.generatedLevel;
        }
        foreach (var enemy in enemies) enemy.levelManager = level;
        BombaCatCheckpointService.OnSceneReady(scene, player, level);
    }

    private void Update()
    {
        if (finished || generatedLevel == null || generatedLevel.EnemiesRemaining > 0) return;
        finished = true;
        if (GameManager.Instance != null) GameManager.Instance.SetPaused(true);
        if (UIManager.Instance != null)
        {
            bool last = SceneManager.GetActiveScene().buildIndex >= SceneManager.sceneCountInBuildSettings - 1;
            if (last) UIManager.Instance.ShowVictory();
            else UIManager.Instance.ShowLevelComplete();
        }
    }
}
