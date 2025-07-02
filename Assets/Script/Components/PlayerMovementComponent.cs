using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementComponent : ComponentBehaviour
{
    #region Variáveis

    [Header("Movimento")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float friction = 2f;
    [SerializeField] private float airfriction = 2f;

    [Header("Parâmetros de Pulo")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float gravity = -9.81f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float dashDistance = 10f;
    [SerializeField] private float dashCooldown = 5f;

    [Header("Componentes")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private Vector3 movementVector = Vector3.zero;
    private Vector3 dashDirection;
    private Vector2 moveInput;
    private Vector3 direction;
    private int currentJumpCount;
    private bool isGrounded;
    private bool canDash = true;
    private bool isDashing = false;
    private InputAction moveAction;
    private float dashDuration;

    #endregion

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Move");
    }

    private void Start()
    {
        InitializeAttributes();
        DOTween.Init();

        SubscribeToAttribute(nameof(speed), newSpeed => speed = (float)newSpeed);
        SubscribeToAttribute(nameof(jumpForce), newJumpForce => jumpForce = (float)newJumpForce);
    }

    private void FixedUpdate()
    {
        isGrounded = characterController.isGrounded;

        if (isDashing)
            HandleDash();
        else
            HandleMovementAndGravity();

        characterController.Move(movementVector * Time.deltaTime);
    }

    #region InputHandlers

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
            StartDash();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            Jump();
    }

    #endregion

    #region Dash

    private void StartDash()
    {
        dashDirection = moveInput != Vector2.zero
            ? direction
            : transform.forward;

        dashDuration = dashDistance / dashSpeed;

        isDashing = true;
        canDash = false;
        movementVector.y = 0f;

        moveAction.Disable();
        DashVisualSequence();

        Invoke(nameof(ResetDash), dashCooldown);
    }

    private void HandleDash()
    {
        characterController.Move(dashSpeed * Time.deltaTime * dashDirection);

        dashDuration -= Time.deltaTime;
        if (dashDuration <= 0f)
        {
            isDashing = false;
            moveAction.Enable();
        }
    }

    private void ResetDash() => canDash = true;

    #endregion

    #region Movimento e Gravidade

    private void HandleMovementAndGravity()
    {
        RotateAndMove();

        ApplyGravityAndFriction();
    }

    private void RotateAndMove()
    {
        if (cinemachineCamera == null || moveInput == Vector2.zero) return;

        Vector3 forward = cinemachineCamera.transform.forward;
        Vector3 right = cinemachineCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        
        direction = forward * moveInput.y + right * moveInput.x;

        // Rotação suave
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

        // Velocidade suavizada
        float targetX = direction.x * speed;
        float targetZ = direction.z * speed;

        targetX = SmoothLerp(movementVector.x, targetX, acceleration);
        targetZ = SmoothLerp(movementVector.z, targetZ, acceleration);

        movementVector.x = targetX;
        movementVector.z = targetZ;
    }

    private void ApplyGravityAndFriction()
    {
        if (isGrounded && movementVector.y < 0)
        {
            movementVector.y = 0;
            currentJumpCount = 0;
            ApplyFriction(ref movementVector.x, friction);
            ApplyFriction(ref movementVector.z, friction);
        }
        else
        {
            ApplyFriction(ref movementVector.x, airfriction);
            ApplyFriction(ref movementVector.z, airfriction);
            movementVector.y += gravity * Time.deltaTime;
        }
    }

    private void ApplyFriction(ref float velocityComponent, float frictionValue)
    {
        if (moveInput == Vector2.zero)
            velocityComponent = SmoothLerp(velocityComponent, 0, frictionValue);
    }

    private void Jump()
    {
        if (isGrounded || currentJumpCount < maxJumpCount)
        {
            float jumpMultiplier = Mathf.Max(1f - 0.3f * currentJumpCount, 0.2f);
            movementVector.y = jumpForce * jumpMultiplier;
            currentJumpCount++;
        }
    }

    #endregion

    #region Utilitários

    private float SmoothLerp(float start, float end, float smooth)
    {
        return Mathf.Lerp(start, end, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }

    private void InitializeAttributes()
    {
        SetAttribute(nameof(speed), speed);
        SetAttribute(nameof(jumpForce), jumpForce);
    }

    #endregion

    #region DOTween - Animações Visuais

    private void DashVisualSequence()
    {
        var dashSequence = DOTween.Sequence();
        dashSequence.Append(transform.DOScaleY(0.65f, dashDuration * 0.6f));
        dashSequence.Append(transform.DOScaleY(1f, dashDuration * 0.4f));
        dashSequence.SetEase(Ease.InOutSine).SetUpdate(UpdateType.Fixed).Play();
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
    }

    #endregion
}
