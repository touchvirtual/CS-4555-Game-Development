using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveSystem : MonoBehaviour
{
    public static PlayerSaveSystem Instance { get; private set; }

    private ExplosionController explosion;
    private PlayerController movement;
    private PlayerHealth health;
    private PlayerStealth stealth;

    private const string SaveKey = "CatastropheSave";

    [System.Serializable]
    private class SaveData
    {
        public int sceneIndex;
        public float explosionDamage;
        public float explosionRadius;
        public float explosionCooldown;
        public float moveSpeed;
        public int maxHealth;
        public float stealthDuration;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        explosion = GetComponent<ExplosionController>();
        movement = GetComponent<PlayerController>();
        health = GetComponent<PlayerHealth>();
        stealth = GetComponent<PlayerStealth>();

        LoadStats();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            SaveData data = JsonUtility.FromJson<SaveData>(
                PlayerPrefs.GetString(SaveKey)
            );

            if (data.sceneIndex >= 0 &&
                data.sceneIndex < SceneManager.sceneCountInBuildSettings &&
                data.sceneIndex != SceneManager.GetActiveScene().buildIndex)
            {
                SceneManager.LoadScene(data.sceneIndex);
            }
        }
    }

    public void SaveProgress(int nextSceneIndex)
    {
        SaveData data = new SaveData();

        data.sceneIndex = nextSceneIndex;
        data.explosionDamage = explosion != null ? explosion.damage : 1;
        data.explosionRadius = explosion != null ? explosion.radius : 4f;
        data.explosionCooldown = explosion != null ? explosion.cooldown : 2f;
        data.moveSpeed = movement != null ? movement.moveSpeed : 6f;
        data.maxHealth = health != null ? health.maxHealth : 1;
        data.stealthDuration = stealth != null ? stealth.stealthDuration : 5f;

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log("Game progress saved!");
    }

    private void LoadStats()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        SaveData data = JsonUtility.FromJson<SaveData>(
            PlayerPrefs.GetString(SaveKey)
        );

        if (explosion != null)
        {
            explosion.damage = Mathf.RoundToInt(data.explosionDamage);
            explosion.radius = data.explosionRadius;
            explosion.cooldown = data.explosionCooldown;
        }

        if (movement != null)
            movement.moveSpeed = data.moveSpeed;

        if (health != null)
            health.maxHealth = data.maxHealth;

        if (stealth != null)
            stealth.stealthDuration = data.stealthDuration;

        Debug.Log("Saved upgrades loaded!");
    }

    [ContextMenu("Delete Save Data")]
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        Debug.Log("Save data deleted!");
    }
}
