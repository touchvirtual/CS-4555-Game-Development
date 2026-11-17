
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BossPowerUp : MonoBehaviour
{
    [Header("Vehicle")]
    public GameObject catModel;
    public GameObject vehicleModel;

    [Header("Meter")]
    public Slider vehicleMeter;
    public float chargePerHit = 10f;

    [Header("Healing")]
    public float healInterval = 5f;
    public int healAmount = 1;

    private float chargeAmount;
    private float healTimer;

    private bool bossBattleActive;
    private bool vehicleActivated;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();

        catModel = FindChildByName(transform, "cat");
        vehicleModel = FindChildByName(transform, "StarSparrow1");

        if (catModel != null)
            catModel.SetActive(true);
        else
            Debug.LogError("Could not find cat model!");

        if (vehicleModel != null)
            vehicleModel.SetActive(false);
        else
            Debug.LogError("Could not find StarSparrow1!");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private GameObject FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child.gameObject;

            GameObject result = FindChildByName(child, childName);

            if (result != null)
                return result;
        }

        return null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindMeter();
    }

    private void Start()
    {
        FindMeter();
    }

    private void FindMeter()
    {
        vehicleMeter = null;

        GameObject meterObject =
            GameObject.Find("VehiclePowerMeter");

        if (meterObject != null)
        {
            vehicleMeter = meterObject.GetComponent<Slider>();

            if (vehicleMeter != null)
            {
                vehicleMeter.minValue = 0;
                vehicleMeter.maxValue = 100;
                vehicleMeter.value = chargeAmount;
                vehicleMeter.gameObject.SetActive(bossBattleActive);
            }
        }
    }

    private void Update()
    {
        if (!bossBattleActive || vehicleActivated)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.IsPaused)
            return;

        // Heal the player over time.
        healTimer += Time.deltaTime;

        if (healTimer >= healInterval)
        {
            healTimer = 0f;

            if (playerHealth != null)
                playerHealth.Heal(healAmount);
        }
    }

    public void StartBossBattle()
    {
        if (bossBattleActive)
            return;

        bossBattleActive = true;

        FindMeter();

        if (vehicleMeter != null)
            vehicleMeter.gameObject.SetActive(true);

        Debug.Log("Boss battle started!");
    }

    // Called whenever the player damages the boss.
    public void AddCharge(float amount)
    {
        Debug.Log("AddCharge called! Boss battle active: " + bossBattleActive);

        if (!bossBattleActive || vehicleActivated)
            return;

        chargeAmount += amount;
        chargeAmount = Mathf.Clamp(chargeAmount, 0f, 100f);

        if (vehicleMeter != null)
            vehicleMeter.value = chargeAmount;

        Debug.Log("Vehicle Meter: " + chargeAmount + "%");

        if (chargeAmount >= 100f)
            ActivateVehicle();
    }

    private void ActivateVehicle()
    {
        vehicleActivated = true;

        if (catModel != null)
            catModel.SetActive(false);

        if (vehicleModel != null)
            vehicleModel.SetActive(true);

        Debug.Log("Vehicle Power-Up Activated!");
    }
}
