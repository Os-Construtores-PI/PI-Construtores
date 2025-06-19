using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementComponent : ComponentBehaviour
{
    #region Variáveis
    [Header("Movimento")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 5;
    [SerializeField] private float friction = 2f;
    [SerializeField] private float airfriction = 2f;

    [Header("Parametros de Pulo")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float gravity = -9.81f;
    [Header("Dash Parametros")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 5f;

    [Header("Componentes")]
    [SerializeField] private CharacterController characterController;
    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private Vector3 movementVector = Vector3.zero;
    private Vector3 dir;
    private Vector2 moveInput;
    private int currentJumpCount;
    private bool isGrounded;
    private bool canDash = true;
    private bool isDashing = false;
    private InputAction move_action;
    #endregion


    private void Start()
    {
        StartAtributes();

        SubscribeToAttribute(nameof(speed), (newSpeed) =>
        {
            speed = (float) newSpeed;
        });
        SubscribeToAttribute(nameof(jumpForce), (newJumpForce) =>
        {
            jumpForce = (float) newJumpForce;
        });
    }
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        move_action = InputSystem.actions.FindAction("Move");
    }
    private void FixedUpdate()
    {
        isGrounded = characterController.isGrounded;

        DashLogica();
        RotationAndMovement();

        characterController.Move(movementVector * Time.deltaTime);
    }

    #region Input
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
        {
            if (movementVector.x != 0 && movementVector.z != 0)
            {
                dir = new Vector3(movementVector.x, 0, movementVector.z).normalized;
            }
            else
            {
                dir = transform.forward;
            }
            StartDash();
            move_action.Disable();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpLogica();
        }
    }
    #endregion


    #region Dash
    private void StartDash()
    {
        characterController.Move(Vector3.zero);
        canDash = false;
        isDashing = true;
        Invoke(nameof(ResetDash), dashCooldown);

    }
    void DashLogica()
    {
        if (isDashing)
        {
            characterController.Move(dashSpeed * Time.deltaTime * dir);
            dashDuration -= Time.deltaTime;

            if (dashDuration <= 0f)
            {
                isDashing = false;
                dashDuration = 0.4f;
            }
        }
        else
        {
            ApplyGravity();
        }
    }
    private void ResetDash()
    {
        move_action.Enable();
        canDash = true;
    }
    #endregion
    #region movimento Y
    private void ApplyGravity()
    {
        if (isGrounded && movementVector.y < 0)
        {
            movementVector.y = 0f;
            currentJumpCount = 0;
            if (moveInput == Vector2.zero)
            {
                movementVector.x = Interp(movementVector.x, 0, friction);
                movementVector.z = Interp(movementVector.z, 0, friction);
            }
        }
        else
        {
            if (moveInput == Vector2.zero)
            {
                movementVector.x = Interp(movementVector.x, 0, airfriction);
                movementVector.z = Interp(movementVector.z, 0, airfriction);
            }
            movementVector.y += gravity * Time.deltaTime;
        }
    }

    private void JumpLogica()
    {
        if (isGrounded || currentJumpCount < maxJumpCount)
        {
            movementVector.y = jumpForce;
            currentJumpCount++;
        }
    }
    #endregion

    #region Movimentos
    private void RotationAndMovement()
    {
        if (cinemachineCamera && moveInput != Vector2.zero)
        {
            var cameraTransform = cinemachineCamera.transform;
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            Vector3 direction = forward * moveInput.y + right * moveInput.x;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            float targetX = direction.x * speed;
            float targetZ = direction.z * speed;
            targetX = Interp(movementVector.x, targetX, acceleration);
            targetZ = Interp(movementVector.z, targetZ, acceleration);
            movementVector = new(targetX, movementVector.y, targetZ);
        }
    }

    #endregion








    #region Utils
    private float Interp(float from, float target, float smooth)
    {
        float newvalue = Mathf.Lerp(from, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        return newvalue;

    }
    private void StartAtributes()
    {
        SetAttribute(nameof(speed), speed);
        SetAttribute(nameof(jumpForce), jumpForce);
    }
    #endregion
}
