
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public int currentLevel = 1;
    public int enemiesRequired = 8;
    private int defeatedThisLevel;
    public NPCArrowIndicator npcArrow;

    public void StartLevel(int levelNumber, int requiredEnemies)
    {
        currentLevel = levelNumber;
        enemiesRequired = requiredEnemies;
        defeatedThisLevel = 0;
    }

    public void EnemyDefeatedForLevel()
    {
        defeatedThisLevel++;

        if (defeatedThisLevel >= enemiesRequired)
        {
            if (npcArrow != null)
            {
                npcArrow.ShowArrow();
            }
        }
    }

    public int EnemiesRemaining
    {
        get { return Mathf.Max(0, enemiesRequired - defeatedThisLevel); }
    }

    public void CompleteLevel()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ShowChoices();
        else if (UIManager.Instance != null)
            UIManager.Instance.ShowLevelComplete();
    }

    public void ContinueAfterUpgrade()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Congratulations! You completed all levels!");
        }
    }
}
