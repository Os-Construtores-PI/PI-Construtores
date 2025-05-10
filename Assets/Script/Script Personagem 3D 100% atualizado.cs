using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float rotationSpeed = 100f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int maxJumpCount = 2; // Allow for double jump
    [SerializeField] private float gravity = -9.81f;

    private CharacterController characterController;
    private Vector3 movementVector = Vector3.zero;
    private Vector2 moveInput;
    private bool isSprinting = false;
    private int currentJumpCount;
    private bool isGrounded;

    // Input Action Callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void GetSprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
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

    private void Update()
    {
        isGrounded = characterController.isGrounded;

        ApplyGravity();
        HandleRotation();
        CalculateMovementVector();
        ApplyMovement();
    }

    private void ApplyGravity()
    {
        if (isGrounded && movementVector.y < 0)
        {
            movementVector.y = -2f; // Small downward force to keep grounded
            currentJumpCount = 0; // Reset jump count when grounded
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

    private void HandleRotation()
    {
        // Rotate the character based on horizontal input
        Vector3 rotationDelta = new Vector3(0, Time.deltaTime * rotationSpeed * moveInput.x, 0);
        transform.Rotate(rotationDelta);
    }

    private void CalculateMovementVector()
    {
        // Determine the current speed based on sprint state
        float currentSpeed = isSprinting ? sprintSpeed : speed;

        // Calculate the desired movement direction in local space (forward/backward)
        // Only use the vertical input (moveInput.y) for forward/backward movement
        Vector3 localMoveDirection = new Vector3(0, 0, moveInput.y * currentSpeed);

        // Transform the local movement direction into world space based on the character's current rotation
        Vector3 worldMoveDirection = transform.TransformDirection(localMoveDirection);

        // Combine the world horizontal movement with the existing vertical movement (from ApplyGravity)
        // Preserve the Y component calculated by ApplyGravity
        movementVector = new Vector3(worldMoveDirection.x, movementVector.y, worldMoveDirection.z);
    }

    private void ApplyMovement()
    {
        // Apply the calculated movement vector to the character controller
        characterController.Move(movementVector * Time.deltaTime);
    }
}
