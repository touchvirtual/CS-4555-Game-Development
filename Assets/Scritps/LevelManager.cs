
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public int currentLevel = 1;
    public int enemiesRequired = 8;
    private int defeatedThisLevel;
    public NPCArrowIndicator npcArrow;
    public int DefeatedThisLevel => defeatedThisLevel;
    private bool completing;

    private IEnumerator Start()
    {
        // NPCArrowIndicator initializes its visibility in Start, so restore after it.
        yield return null; var cat=BombaCatCheckpointService.ActivePlayer; if(cat!=null && cat.health!=null) cat.health.ResetHealthToMax();
        RefreshObjective();
    }

    public void StartLevel(int levelNumber, int requiredEnemies)
    {
        currentLevel = levelNumber;
        enemiesRequired = requiredEnemies;
        defeatedThisLevel = 0;
        completing = false;
    }

    public void EnemyDefeatedForLevel()
    {
        defeatedThisLevel++;

        RefreshObjective();
    }

    public void RestoreProgress(int defeatedCount)
    {
        defeatedThisLevel = Mathf.Clamp(defeatedCount, 0, Mathf.Max(0, enemiesRequired));
        completing = false;
        RefreshObjective();
    }

    private void RefreshObjective()
    {
        if (npcArrow == null) return;
        if (EnemiesRemaining <= 0) npcArrow.ShowArrow();
        else npcArrow.HideArrow();
    }

    public int EnemiesRemaining
    {
        get { return Mathf.Max(0, enemiesRequired - defeatedThisLevel); }
    }

    public void CompleteLevel()
    {
        if (completing) return;
        completing = true;
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ShowChoices();
        else if (UIManager.Instance != null)
            UIManager.Instance.ShowLevelComplete();
    }

    public void ContinueAfterUpgrade()
    {
        int nextSceneIndex = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetPaused(false);

            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetPaused(true);
            if (UIManager.Instance != null)
                UIManager.Instance.ShowVictory();
        }
    }
}

