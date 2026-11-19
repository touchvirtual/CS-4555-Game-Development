using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool IsPaused { get; private set; }
    public int defeatedEnemies { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        IsPaused = false;
        Time.timeScale = 1f;
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void RegisterEnemyDefeated(int value)
    {
        defeatedEnemies += value;
        if (UIManager.Instance != null) UIManager.Instance.UpdateEnemyCount(defeatedEnemies);
    }

    public void RestoreEnemyCount(int count)
    {
        defeatedEnemies = Mathf.Max(0, count);
        if (UIManager.Instance != null) UIManager.Instance.UpdateEnemyCount(defeatedEnemies);
    }

    public void GameOver()
    {
        SetPaused(true);
        if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
    }

    private bool restarting;

    public void RestartGame()
    {
        if (restarting) return;
        restarting = BombaCatCheckpointService.ReloadCheckpoint();
    }

    // Pause-menu restart starts this area again, retaining upgrades brought into it.
    public void RestartLevel()
    {
        if (restarting) return;
        restarting = BombaCatCheckpointService.ReloadLevelStart();
    }

    // A new run is deliberately separate from retrying after a defeat.
    public void RestartRun()
    {
        if (!restarting) StartCoroutine(RestartRunRoutine());
    }

    private IEnumerator RestartRunRoutine()
    {
        restarting = true;
        SetPaused(false);
        BombaCatCheckpointService.ClearRun();
        foreach (var player in FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            player.gameObject.SetActive(false);
            Destroy(player.transform.root.gameObject);
        }
        yield return null;
        SceneManager.LoadScene(0);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void QuitGame() { Application.Quit(); }
}
