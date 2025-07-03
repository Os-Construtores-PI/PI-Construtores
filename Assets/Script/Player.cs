using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput), typeof(Collider))]
public class Player : CombatEntity
{
    #region Configurações

    [Header("Movimento")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float friction = 2f;
    [SerializeField] private float airFriction = 2f;

    [Header("Pulo")]
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
    [SerializeField] private Transform handTransform;

    #endregion

    #region Estados

    private Vector3 movementVector;
    private Vector3 direction;
    private Vector3 dashDirection;
    private Vector2 moveInput;

    private int currentJumpCount;
    private bool isGrounded;
    private bool canDash = true;
    private bool isDashing = false;

    private float dashDuration;

    private InputAction moveAction;

    private readonly Inventory inventory = new();
    public Inventory Inventario => inventory;

    private readonly Equipament equipament = new();
    public Equipament EquipClassRef => equipament;

    #endregion

    #region Unity

    public override void Awake()
    {
        base.Awake();
        characterController = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Move");
        equipament.Initialize(handTransform);
    }

    public override void Start()
    {
        base.Start();
        DOTween.Init();
        SetHUD();
    }

    private void FixedUpdate()
    {
        isGrounded = characterController.isGrounded;

        if (isDashing)
            HandleDash();
        else
            HandleMovement();

        characterController.Move(movementVector * Time.deltaTime);
    }

    private void OnDestroy() => DOTween.KillAll();

    #endregion

    #region Input

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();

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

    #region Movimento

    private void HandleMovement()
    {
        ApplyRotationAndMovement();
        ApplyGravityAndFriction();
    }

    private void ApplyRotationAndMovement()
    {
        if (cinemachineCamera == null || moveInput == Vector2.zero) return;

        Vector3 forward = cinemachineCamera.transform.forward;
        Vector3 right = cinemachineCamera.transform.right;
        forward.y = right.y = 0;
        forward.Normalize();
        right.Normalize();

        direction = forward * moveInput.y + right * moveInput.x;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

        float targetX = direction.x * speed;
        float targetZ = direction.z * speed;

        movementVector.x = SmoothLerp(movementVector.x, targetX, acceleration);
        movementVector.z = SmoothLerp(movementVector.z, targetZ, acceleration);
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
            ApplyFriction(ref movementVector.x, airFriction);
            ApplyFriction(ref movementVector.z, airFriction);
            movementVector.y += gravity * Time.deltaTime;
        }
    }

    private void ApplyFriction(ref float value, float frictionAmount)
    {
        if (moveInput == Vector2.zero)
            value = SmoothLerp(value, 0, frictionAmount);
    }

    private void Jump()
    {
        if (isGrounded || currentJumpCount < maxJumpCount)
        {
            float multiplier = Mathf.Max(1f - 0.3f * currentJumpCount, 0.2f);
            movementVector.y = jumpForce * multiplier;
            currentJumpCount++;
        }
    }

    #endregion

    #region Dash

    private void StartDash()
    {
        dashDirection = moveInput != Vector2.zero ? direction : transform.forward;
        dashDuration = dashDistance / dashSpeed;
        isDashing = true;
        canDash = false;
        movementVector.y = 0;

        moveAction.Disable();
        DashVisualSequence();
        Invoke(nameof(ResetDash), dashCooldown);
    }

    private void HandleDash()
    {
        characterController.Move(dashSpeed * Time.deltaTime * dashDirection);
        dashDuration -= Time.deltaTime;

        if (dashDuration <= 0)
        {
            isDashing = false;
            moveAction.Enable();
        }
    }

    private void ResetDash() => canDash = true;

    #endregion

    #region HUD

    private void SetHUD()
    {
        foreach (var hudObj in GameObject.FindGameObjectsWithTag("HealthHUD"))
        {
            if (hudObj.TryGetComponent(out HealthHUDComponent hud) && hud.IdHealth == ID && hud.HUDType == HealthHUDType.PLAYER)
            {
                _healthHUD = hud;
                _OnHealthChanged.AddListener(hud.UpdateSlider);
                break;
            }
        }

        if (GameObject.FindWithTag("GameController").TryGetComponent(out HUDDirector hudDir))
        {
            _OnDamage.AddListener(hudDir.ShakeCamera);
        }
    }

    #endregion

    #region Utilitários

    private float SmoothLerp(float from, float to, float smoothing)
        => Mathf.Lerp(from, to, 1f - Mathf.Exp(-smoothing * Time.deltaTime));

    private void DashVisualSequence()
    {
        DOTween.Sequence()
            .Append(transform.DOScaleY(0.65f, dashDuration * 0.6f))
            .Append(transform.DOScaleY(1f, dashDuration * 0.4f))
            .SetEase(Ease.InOutSine)
            .SetUpdate(UpdateType.Fixed)
            .Play();
    }

    #endregion

    #region Equipamento

    public class Equipament
    {
        public EquipableItemData currentItem;
        public GameObject equippedWeapon;
        private Transform handTransform;

        public void Initialize(Transform hand)
        {
            handTransform = hand;
        }

        public void Equip(EquipableItemData item)
        {
            Unequip();

            if (item?.item != null)
            {
                equippedWeapon = Object.Instantiate(item.item, handTransform);
                equippedWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                currentItem = item;
            }
        }

        public void Unequip()
        {
            if (equippedWeapon != null)
            {
                Object.Destroy(equippedWeapon);
                equippedWeapon = null;
            }

            currentItem = null;
        }
    }

    #endregion
}
