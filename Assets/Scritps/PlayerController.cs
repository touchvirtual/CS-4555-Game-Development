
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    public Transform cameraTransform;
    public PlayerHealth health;
    public ExplosionController explosion;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    // Oil slowdown: 1 = normal speed, 0.5 = half speed.
    private float oilSpeedMultiplier = 1f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (health == null)
            health = GetComponent<PlayerHealth>();

        if (explosion == null)
            explosion = GetComponent<ExplosionController>();
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsPaused)
            return;

        if (health != null && health.IsDead)
            return;

        if (Keyboard.current == null)
            return;

        // Movement input using the new Input System.
        float x = 0f;
        float z = 0f;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
            x -= 1f;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
            x += 1f;

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
            z -= 0.5f;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
            z += 1f;

        // Camera-relative movement.
        Transform basis = cameraTransform != null
            ? cameraTransform
            : transform;

        Vector3 forward = Vector3.ProjectOnPlane(
            basis.forward, Vector3.up).normalized;

        Vector3 right = Vector3.ProjectOnPlane(
            basis.right, Vector3.up).normalized;

        Vector3 move = (forward * z + right * x).normalized;

        // Apply oil slowdown.
        controller.Move(
            move * moveSpeed * oilSpeedMultiplier * Time.deltaTime
        );

        // Rotate toward movement direction.
        if (move.sqrMagnitude > 0.01f)
        {
            transform.forward = Vector3.Slerp(
                transform.forward,
                move,
                12f * Time.deltaTime
            );
        }

        // Gravity and jumping.
        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        if (controller.isGrounded &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity.y =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move(verticalVelocity * Time.deltaTime);

        // Explode using E.
        if (Keyboard.current.eKey.wasPressedThisFrame &&
            explosion != null)
        {
            explosion.TryExplode();
        }
    }

    // Called by oil zones.
    public void SetOilSlow(float multiplier)
    {
        oilSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
    }
}
