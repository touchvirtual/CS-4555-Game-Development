
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
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentIndex + 1;

        Debug.Log("Current Scene Index: " + currentIndex);
        Debug.Log("Next Scene Index: " + nextSceneIndex);

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetPaused(false);

            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("Next level is not in Build Profiles!");
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.ResetHealthToMax();
            }
        }
    }
}
