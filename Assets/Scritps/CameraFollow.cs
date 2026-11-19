using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-50)]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 8, -7);
    public float followSpeed = 8f;
    public float lookHeight = 1.2f;
    [HideInInspector] public bool useWorldSpaceOffset;
    [HideInInspector] public bool orbitBehindMovement;
    public bool mouseLook = true;
    public bool simpleFollow = true;
    [Range(.02f, .5f)] public float mouseSensitivity = .14f;
    [Range(.05f, .8f)] public float collisionRadius = .22f;
    private bool positioned, initialized;
    private float yaw, pitch, distance;
    public Vector3 PlanarForward => Quaternion.Euler(0, yaw, 0) * Vector3.forward;
    private readonly RaycastHit[] hits = new RaycastHit[32];
    private void OnEnable() { SceneManager.sceneLoaded += SceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= SceneLoaded; }
    private void Start() { FindPlayer(); }
    private void SceneLoaded(Scene scene, LoadSceneMode mode) { FindPlayer(); }
    private void FindPlayer()
    {
        var player = BombaCatCheckpointService.ActivePlayer;
        if (player == null) player = FindAnyObjectByType<PlayerController>();
        if (player != null) { target = player.transform; ResetForRespawn(); }
    }
    public void ResetForRespawn()
    {
        positioned = false;
        if (target != null) yaw = target.eulerAngles.y;
        float horizontal = Mathf.Max(1f, new Vector2(offset.x, offset.z).magnitude);
        pitch = Mathf.Clamp(Mathf.Atan2(offset.y-lookHeight, horizontal)*Mathf.Rad2Deg, 15f, 65f);
        distance = Mathf.Max(2f, new Vector3(offset.x,offset.y-lookHeight,offset.z).magnitude);
        initialized = target != null;
    }
    private void Update()
    {
        if (target == null) FindPlayer();
        if (!initialized) ResetForRespawn();
        if (Time.deltaTime <= 0f || (GameManager.Instance != null && GameManager.Instance.IsPaused)) return;
        if (!simpleFollow && mouseLook && Mouse.current != null && (Cursor.lockState == CursorLockMode.Locked || Mouse.current.rightButton.isPressed))
        {
            var delta = Mouse.current.delta.ReadValue();
            yaw = Mathf.Repeat(yaw + delta.x * mouseSensitivity, 360f);
            pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, 15f, 65f);
        }
    }
    private void LateUpdate()
    {
        if (target == null || (Time.deltaTime <= 0f && positioned)) return;
        Vector3 focus = target.position + Vector3.up * lookHeight;
        if(simpleFollow) yaw=target.eulerAngles.y;
        Vector3 direction = -(Quaternion.Euler(pitch, yaw, 0) * Vector3.forward);
        float allowed = distance;
        int count = Physics.SphereCastNonAlloc(focus, collisionRadius, direction, hits, distance, ~(1 << 5), QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            var collider = hits[i].collider;
            if (collider.transform.IsChildOf(target) || collider.GetComponentInParent<EnemyHealth>() != null) continue;
            allowed = Mathf.Min(allowed, Mathf.Max(.65f, hits[i].distance - .1f));
        }
        Vector3 desired = focus + direction * allowed;
        if (simpleFollow || !positioned || allowed < distance-.1f) transform.position = desired;
        else transform.position = Vector3.Lerp(transform.position, desired, 1f-Mathf.Exp(-Mathf.Max(.1f,followSpeed)*Time.deltaTime));
        positioned = true;
        Vector3 look = focus-transform.position;
        if (look.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(look,Vector3.up);
    }
}
