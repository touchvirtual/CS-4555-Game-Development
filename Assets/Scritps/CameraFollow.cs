
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 8, -7);
    public float followSpeed = 8f;
    public float lookHeight = 1.2f;

    private bool hasPositioned = false;

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
        FindPlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
            hasPositioned = false;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindPlayer();
            if (target == null) return;
        }

        Vector3 desired = target.position + target.TransformDirection(offset);

        if (!hasPositioned)
        {
            transform.position = desired;
            hasPositioned = true;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desired,
                followSpeed * Time.deltaTime
            );
        }

        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
