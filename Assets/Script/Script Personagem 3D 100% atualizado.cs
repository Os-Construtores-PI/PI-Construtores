using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 5;

    [Header("Parametros de Pulo")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int maxJumpCount = 2; // Allow for double jump
    [SerializeField] private float gravity = -9.81f;
    [Header("Dash Parametros")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    private CharacterController characterController;
    private Vector3 movementVector = Vector3.zero;
    private Vector3 dir;
    private Vector2 moveInput;
    private int currentJumpCount;
    private bool isGrounded;
    private bool canDash = true;
    private bool isDashing = false;
    private bool canMove = true;

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
        {
            if (movementVector.x != 0 && movementVector.y != 0)
            {
                dir = new Vector3(movementVector.x, 0, movementVector.z).normalized;
            }
            else
            {
                dir = transform.forward;
            }
            StartDash();
            canMove = false;
        }
    }
    private void StartDash()
    {
        canDash = false;
        isDashing = true;
        Invoke(nameof(ResetDash), dashCooldown);

    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;

        ApplyGravity();
        CalculateMovementVector();
        ApplyMovement();
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
    }

    private void ResetDash()
    {
        canDash = true;
        canMove = true;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            HandleJump();
        }
    }
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    private void ApplyGravity()
    {
        if (isGrounded && movementVector.y < 0)
        {
            movementVector.y = -2f;
            currentJumpCount = 0;
        }
        else
        {
            movementVector.y += gravity * Time.deltaTime;
        }
    }

    private void HandleJump()
    {
        if (isGrounded || currentJumpCount < maxJumpCount)
        {
            movementVector.y = jumpForce;
            currentJumpCount++;
        }
    }
    private void CalculateMovementVector()
    {
        Vector2 target = new(Mathf.Lerp(movementVector.x,moveInput.x * speed,1-Mathf.Exp(-acceleration*Time.deltaTime)),Mathf.Lerp(movementVector.z,moveInput.y*speed,1-Mathf.Exp(-acceleration*Time.deltaTime)));
        movementVector = new Vector3(target.x, movementVector.y,target.y);
    }

    private void ApplyMovement()
    {
        characterController.Move(movementVector * Time.deltaTime);
    }
}
