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

    public void GameOver()
    {
        SetPaused(true);
        if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame() { Application.Quit(); }
}
