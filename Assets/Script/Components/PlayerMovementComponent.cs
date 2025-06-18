using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementComponent : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 5;

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
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

    private Vector3 movementVector = Vector3.zero;
    private Vector3 dir;
    private Vector2 moveInput;
    private Vector2 HorizontalLerp;
    private int currentJumpCount;
    private bool isGrounded;
    private bool canDash = true;
    private bool isDashing = false;
    private InputAction move_action;


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        move_action = InputSystem.actions.FindAction("Move");
        if (cinemachineCamera != null)
        {
            orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
        }
    }
    private void FixedUpdate()
    {
        isGrounded = characterController.isGrounded;

        DashLogica();
        Movement();
        RotateTransform();



        characterController.Move(movementVector * Time.deltaTime);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position,movementVector);
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
        }
        else
        {
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

    #region movimento X
    private void Movement()
    {
        Vector2 target = new(moveInput.x * speed, moveInput.y * speed);
        HorizontalLerp = new(Mathf.Lerp(movementVector.x, target.x, 1 - Mathf.Exp(-acceleration * Time.deltaTime)), Mathf.Lerp(movementVector.z, target.y, 1 - Mathf.Exp(-acceleration * Time.deltaTime)));
    }
    #endregion

    #region Camera
    private void RotateTransform()
    {
        if (orbitalFollow)
        {
            transform.localEulerAngles = new(0f, orbitalFollow.HorizontalAxis.Value, 0f);

            Vector3 horizontalMovement = new(HorizontalLerp.x, 0f, HorizontalLerp.y);
            horizontalMovement = transform.TransformDirection(horizontalMovement);

            movementVector = horizontalMovement + Vector3.up * movementVector.y;
        }
    }
    // Em breve
    #endregion
}
