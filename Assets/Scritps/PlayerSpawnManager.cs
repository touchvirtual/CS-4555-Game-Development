using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerSpawnManager : MonoBehaviour
{
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(MoveToSpawnPoint());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(MoveToSpawnPoint());
    }

    private IEnumerator MoveToSpawnPoint()
    {
        yield return null;
        yield return new WaitForFixedUpdate();

        GameObject spawnObject =
            GameObject.FindGameObjectWithTag("SpawnPoint");

        if (spawnObject == null)
        {
            Debug.LogWarning("No SpawnPoint found in this scene!");
            yield break;
        }

        characterController.enabled = false;

        transform.position = spawnObject.transform.position;
        transform.rotation = spawnObject.transform.rotation;

        characterController.enabled = true;

        Physics.SyncTransforms();

        Debug.Log("Player moved to SpawnPoint!");
    }
}
