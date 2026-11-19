using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -20f;
    public Transform cameraTransform;
    public PlayerHealth health;
    public ExplosionController explosion;

    [Header("Movement feel")]
    [Min(0.1f)] public float acceleration = 35f;
    [Min(0.1f)] public float deceleration = 45f;
    [Min(1f)] public float turnSpeed = 720f;
    [Range(0f, 0.25f)] public float coyoteTime = 0.1f;
    [Range(0f, 0.25f)] public float jumpBuffer = 0.12f;
    [Min(1f)] public float sprintMultiplier = 1.65f;
    [Range(0f, 0.15f)] public float jumpAnticipation = 0.075f;
    [Header("Cat animation")]
    public bool useBombaCatLocomotion = true;
    [Tooltip("World distance per complete walk cycle. Tune to match paw contact.")]
    [Min(0.1f)] public float strideLength = 2.8f;
    [Min(0.1f)] public float runStrideLength = 3.8f;
    [Min(0.01f)] public float animationBlendTime = 0.12f;
    public float HorizontalSpeed { get; private set; }
    public float NormalizedSpeed => HorizontalSpeed / Mathf.Max(0.01f, moveSpeed);
    public bool IsGrounded { get; private set; }
    public bool IsRunning { get; private set; }
    public float VerticalSpeed => verticalSpeed;
    public Vector3 MovementDirection => horizontalVelocity.sqrMagnitude > 0.001f ? horizontalVelocity.normalized : transform.forward;
    public float JumpPoseTime { get; private set; }
    public bool IsPreparingJump => preparingJump;

    private CharacterController controller;
    private Animator catAnimator;
    private Vector3 horizontalVelocity;
    private float verticalSpeed;
    private float oilSpeedMultiplier = 1f;
    private float lastGroundedTime = float.NegativeInfinity;
    private float lastJumpPressedTime = float.NegativeInfinity;
    private bool animationReady;
    private bool advancedAnimation;
    private bool preparingJump;
    private float preparationElapsed;
    private float takeoffVelocity;
    private float airTime;
    private Vector3 movementForward = Vector3.forward;
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int WalkRateParameter = Animator.StringToHash("WalkRate");
    private static readonly int GroundedParameter = Animator.StringToHash("Grounded");
    private static readonly int RunRateParameter = Animator.StringToHash("RunRate");
    private static readonly int RunningParameter = Animator.StringToHash("Running");
    private static readonly int JumpingParameter = Animator.StringToHash("Jumping");
    private static readonly int JumpTimeParameter = Animator.StringToHash("JumpTime");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (health == null) health = GetComponent<PlayerHealth>();
        if (explosion == null) explosion = GetComponent<ExplosionController>();
        controller.skinWidth = Mathf.Clamp(controller.skinWidth,
            Mathf.Max(0.001f, controller.radius * 0.05f), Mathf.Max(0.002f, controller.radius * 0.15f));
        controller.stepOffset = Mathf.Min(controller.stepOffset, Mathf.Max(0.001f, controller.height * 0.35f));
        controller.minMoveDistance = 0f;
        ConfigureAnimation();
    }
    private void ConfigureAnimation()
    {
        catAnimator = GetComponentInChildren<Animator>(true);
        if (!useBombaCatLocomotion || catAnimator == null) return;
        var locomotion = Resources.Load<RuntimeAnimatorController>("BombaCatLocomotion");
        if (locomotion == null)
        {
            Debug.LogWarning("BombaCat locomotion is missing. Run Tools > BombaCat > Build Cat Locomotion.", this);
            return;
        }
        // The source scene overrides the cat's controller with null. Keep its native Avatar.
        catAnimator.runtimeAnimatorController = locomotion;
        catAnimator.applyRootMotion = false;
        catAnimator.updateMode = AnimatorUpdateMode.Normal;
        catAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        catAnimator.SetFloat(SpeedParameter, 0f);
        catAnimator.SetFloat(WalkRateParameter, 1f);
        catAnimator.SetBool(GroundedParameter, true);
        foreach (var parameter in catAnimator.parameters)
            if (parameter.nameHash == RunningParameter) advancedAnimation = true;
        animationReady = true;
        ResetAnimation();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ResetForRespawn(false);
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ResetAnimation();
    }
    public void ResetForRespawn(bool resetCamera = true)
    {
        horizontalVelocity = Vector3.zero;
        verticalSpeed = 0f;
        HorizontalSpeed = 0f;
        IsGrounded = false;
        IsRunning = false;
        preparingJump = false;
        preparationElapsed = 0f;
        takeoffVelocity = 0f;
        airTime = 0f;
        JumpPoseTime = 0f;
        oilSpeedMultiplier = 1f;
        lastGroundedTime = float.NegativeInfinity;
        lastJumpPressedTime = float.NegativeInfinity;
        ResetAnimation();
        if (resetCamera && cameraTransform != null)
        {
            var follow = cameraTransform.GetComponent<CameraFollow>();
            if (follow != null) follow.ResetForRespawn();
        }
    }
    private void ResetAnimation()
    {
        if (!animationReady || catAnimator == null) return;
        catAnimator.SetFloat(SpeedParameter, 0f);
        catAnimator.SetFloat(WalkRateParameter, 1f);
        if (advancedAnimation) { catAnimator.SetFloat(RunRateParameter, 1f); catAnimator.SetFloat(JumpTimeParameter, 0f); }
        catAnimator.SetBool(GroundedParameter, true);
        if (advancedAnimation) { catAnimator.SetBool(RunningParameter, false); catAnimator.SetBool(JumpingParameter, false); }
        if (catAnimator.isActiveAndEnabled) catAnimator.Play("Grounded locomotion", 0, 0f);
    }
    private void Start() { FindCamera(); }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { FindCamera(); ResetForRespawn(); }
    private void FindCamera()
    {
        if (Camera.main != null) cameraTransform = Camera.main.transform;
    }
    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || (GameManager.Instance != null && GameManager.Instance.IsPaused)) return;
        if (health != null && health.IsDead)
        {
            horizontalVelocity = Vector3.zero; HorizontalSpeed = 0f; IsRunning = false;
            preparingJump = false; UpdateAnimation(dt); return;
        }
        if (!controller.enabled) return;
        var keyboard = Keyboard.current;
        Vector2 input = Vector2.zero;
        bool sprintHeld = false;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.spaceKey.wasPressedThisFrame) lastJumpPressedTime = Time.time;
            sprintHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        }
        // In demo steering mode, turning and forward/backward motion are separate.
        var follow = cameraTransform != null ? cameraTransform.GetComponent<CameraFollow>() : null;
        bool simple = follow == null || follow.simpleFollow;
        if(!simple) input=Vector2.ClampMagnitude(input,1f);
        bool hasInput = simple ? Mathf.Abs(input.y)>.001f : input.sqrMagnitude>.001f;
        if(simple) transform.rotation=Quaternion.Euler(0f,transform.eulerAngles.y+input.x*100f*dt,0f);
        movementForward = follow != null ? follow.PlanarForward : cameraTransform != null
            ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, movementForward);
        Vector3 desiredDirection = simple ? transform.forward*input.y : movementForward * input.y + right * input.x;
        if(simple && input.y<=0f) sprintHeld=false;
        Vector3 desiredVelocity = desiredDirection * Mathf.Max(0f, moveSpeed) * oilSpeedMultiplier
            * (sprintHeld ? sprintMultiplier : 1f);
        float response = hasInput ? acceleration : deceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredVelocity, Mathf.Max(0.1f, response) * dt);
        if (!simple && desiredDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(desiredDirection, Vector3.up), Mathf.Max(1f, turnSpeed) * dt);
        bool wasGrounded = controller.isGrounded && verticalSpeed <= 0f;
        if (wasGrounded)
        {
            lastGroundedTime = Time.time; verticalSpeed = -2f;
        }
        float effectiveGravity = -Mathf.Max(0.01f, Mathf.Abs(gravity));
        if (!preparingJump && Time.time - lastJumpPressedTime <= jumpBuffer && Time.time - lastGroundedTime <= coyoteTime)
        {
            lastJumpPressedTime = float.NegativeInfinity; lastGroundedTime = float.NegativeInfinity;
            preparingJump = true;
            preparationElapsed = wasGrounded ? 0f : jumpAnticipation;
        }
        if (preparingJump)
        {
            preparationElapsed += dt;
            JumpPoseTime = 0.18f * Mathf.Clamp01(preparationElapsed / Mathf.Max(0.001f, jumpAnticipation));
            if (preparationElapsed >= jumpAnticipation)
            {
                preparingJump = false;
                takeoffVelocity = Mathf.Sqrt(2f * Mathf.Max(0f, jumpHeight) * -effectiveGravity);
                verticalSpeed = takeoffVelocity;
                lastGroundedTime = float.NegativeInfinity;
                airTime = 0f;
                JumpPoseTime = 0.23f;
            }
        }
        verticalSpeed += effectiveGravity * dt;
        Vector3 before = transform.position;
        CollisionFlags collisions = controller.Move((horizontalVelocity + Vector3.up * verticalSpeed) * dt);
        if ((collisions & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
        IsGrounded = (collisions & CollisionFlags.Below) != 0 && verticalSpeed <= 0f;
        if (IsGrounded)
        {
            verticalSpeed = -2f; lastGroundedTime = Time.time; airTime = 0f;
            if (!preparingJump) JumpPoseTime = 0f;
        }
        else if (!preparingJump)
        {
            airTime += dt;
            float reference = Mathf.Max(1f, takeoffVelocity);
            JumpPoseTime = verticalSpeed > 0f
                ? Mathf.Lerp(0.23f, 0.56f, 1f - Mathf.Clamp01(verticalSpeed / reference))
                : Mathf.Lerp(0.56f, 0.96f, Mathf.Clamp01(-verticalSpeed / reference));
        }
        HorizontalSpeed = Vector3.ProjectOnPlane(transform.position - before, Vector3.up).magnitude / dt;
        IsRunning = sprintHeld && hasInput && HorizontalSpeed > 0.4f && IsGrounded && !preparingJump;
        UpdateAnimation(dt);
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame && explosion != null) explosion.TryExplode();
    }
    private void UpdateAnimation(float dt)
    {
        if (!animationReady || catAnimator == null) return;
        float speed = Mathf.Clamp(HorizontalSpeed / 5f, 0f, 3f);
        catAnimator.SetBool(GroundedParameter, IsGrounded);
        if (advancedAnimation)
        {
            catAnimator.SetBool(RunningParameter, IsRunning);
            catAnimator.SetBool(JumpingParameter, preparingJump || (!IsGrounded && (verticalSpeed > 0f || airTime > 0.04f)));
            catAnimator.SetFloat(JumpTimeParameter, JumpPoseTime);
        }
        catAnimator.SetFloat(SpeedParameter, speed, animationBlendTime, dt);
        float rate = HorizontalSpeed > 0.05f ? Mathf.Clamp(speed * 2.8f / Mathf.Max(0.1f, strideLength), 0.15f, 4f) : 1f;
        catAnimator.SetFloat(WalkRateParameter, rate, animationBlendTime, dt);
        if (advancedAnimation) catAnimator.SetFloat(RunRateParameter,
            Mathf.Clamp(HorizontalSpeed * 0.46f / Mathf.Max(0.1f, runStrideLength), 0.2f, 3f),
            animationBlendTime, dt);
    }
    public void SetOilSlow(float multiplier) { oilSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f); }
}
