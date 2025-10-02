using System;
using System.Collections;
using DG.Tweening;

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput), typeof(Collider))]
[RequireComponent(typeof(Animator))]
[DefaultExecutionOrder(-100)]
public class Player : CombatEntities
{
    #region --- Configurações de Movimento ---

    [Header("Movimento")]
    [SerializeField] private float speed = 10f;
    [HideInInspector]
    [Stat(nameof(Speed))]
    public float Speed
    {
        get => speed;
        set => speed = value;
    }
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float friction = 2f;
    [SerializeField] private float airFriction = 2f;

    [Header("Pulo")]
    [SerializeField] private float jumpForce = 10f;

    [HideInInspector]
    [Stat(nameof(JumpForce))]
    public float JumpForce
    {
        get => jumpForce;
        set => jumpForce = value;
    }
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float gravity = -9.81f;
    private float initialGravity;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float dashDistance = 10f;
    [SerializeField] private float dashCooldown = 5f;

    [Header("Componentes")]
    [SerializeField] protected CharacterController characterController;
    public CharacterController Charactercontroller => characterController;
    [SerializeField] protected CinemachineCamera cinemachineCamera;
    public void SetCinemachineCamera(CinemachineCamera cam)
    {
        cinemachineCamera = cam;
    }
    [SerializeField] protected Animator animatorComp;
    public Animator AnimatorComp => animatorComp;

    #endregion

    #region --- Estados Internos ---
    private Vector3 movementVector;
    private Vector3 direction;
    private Vector3 dashDirection;
    private Vector2 moveInput;
    private Vector3 lastWallNormal;

    private int currentJumpCount;
    private bool isGrounded;
    private bool wallSpeedApplied;
    private bool touchingWall;
    private bool canDash = true;
    private bool canMove = true;
    [Stat(nameof(CanMove))]
    public bool CanMove
    {
        get => canMove;
        set => canMove = value;
    } // nova flag para controle de movimento
    [Stat(nameof(CanDash))]
    public bool CanDash
    {
        get => canDash;
        set => canDash = value;
    }
    private bool isDashing = false;
    private float dashDuration;
    #endregion


    #region EnemyScan
    [Header("SCANNER DE SPAWN DE INIMIGOS PARÂMETROS")]
    [SerializeField, Min(10)] private float enemyScanRadius = 10;
    [SerializeField, Min(1)] private float enemyScanCooldown = 2.0f;
    private float enemyScanWalker = 0.0f;
    #endregion


    #region Interação
    [Header("SCANNER DE OBJETOS INTERAGÍVEIS PARÂMETROS")]
    [SerializeField] private float interactionScanCooldown = .1f;
    protected InteractableObject interactableRef;
    private float interactionScanCooldownWalker = 0.0f;
    private Camera selectedcamera = null;
    #endregion

    #region Inventário
    private readonly Inventory inventory = new();
    public Inventory Inventory => inventory;
    #endregion

    #region --- Inicialização Unity ---
    #region Coletáveis


    // === AMETISTAS ===
    private int amethysts;
    public int Amethysts => amethysts;
    public void SetAmethysts(int value)
    {
        if (amethysts == value) return;
        int oldValue = amethysts;

        amethysts = Mathf.Max(0, value); // evita negativo
        GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.Invoke(amethysts);
    }
    public void AddAmethysts(int amount) => SetAmethysts(amethysts + amount);
    public bool SpendAmethysts(int amount)
    {
        if (amount <= 0 || amethysts < amount) return false;
        SetAmethysts(amethysts - amount);
        return true;
    }
    #endregion



    public override void Awake()
    {
        base.Awake();
        initialGravity = gravity;
        characterController = GetComponent<CharacterController>();
        animatorComp = GetComponent<Animator>();
        SetupCamera();
    }

    public override void Start()
    {
        base.Start();
        DOTween.Init();
        StartCoroutine(DelayedSetupHUD(.1f));
    }
    public override void Update()
    {
        base.Update();
        EnemyScanTimer();
        ObjectScanTimer();
        KnockbackTimer();
        ChangeCharacterTimer();
        AttackTimer();
        WallRunningTimer();
        // print("[SPEED] : " + Speed + " // " + "[ACTIVEMODIFICATIONS] : " + stats.GetActiveModifications().Count);
    }

    private void FixedUpdate()
    {
        if (!characterController.enabled) return;
        isGrounded = characterController.isGrounded;

        if (isDashing) HandleDash();
        else HandleMovement();

        //characterController.Move(movementVector * Time.deltaTime);

        Vector3 finalMovement = movementVector;

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            finalMovement += knockbackVelocity;

            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 5f);

        }

        characterController.Move(finalMovement * Time.deltaTime);
    }

    private void OnDestroy() => DOTween.KillAll();

    #endregion
    #region --- Input Callbacks ---

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
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (interactableRef && context.started)
        {
            InfoPlayerInteraction info = new(gameObject, this);
            interactableRef.Interaction(info);
        }
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Attack();
        }
    }
    public void OnChangeCharacter(InputAction.CallbackContext context)
    {
        float charAxis = context.ReadValue<float>();
        print(charAxis + ":" + name);
    }
    [Header("TROCA DE JOGADOR PARÂMETROS")]
    [SerializeField] private float ChangeCharacterCooldown = 5f;
    private float ChangeCharacterCooldownWalker = 0.0f;
    private bool CanChangeCharacter = true;
    private void ChangeCharacterTimer()
    {
        if (!CanChangeCharacter)
        {
            ChangeCharacterCooldownWalker += Time.deltaTime;
            if (ChangeCharacterCooldownWalker >= ChangeCharacterCooldown)
            {
                CanChangeCharacter = true;
                ChangeCharacterCooldownWalker = 0.0f;
            }
        }
    }

    #endregion

    #region --- Movimento & Pulo ---

    private void HandleMovement()
    {
        if (!CanMove) return;
        ApplyRotationAndDirection();
        ApplyGravityAndFriction();
    }

    private void ApplyRotationAndDirection()
    {
        if (cinemachineCamera == null || moveInput == Vector2.zero) return;

        Vector3 forward = cinemachineCamera.transform.forward;
        Vector3 right = cinemachineCamera.transform.right;
        forward.y = right.y = 0f;

        direction = forward.normalized * moveInput.y + right.normalized * moveInput.x;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);

        movementVector.x = SmoothLerp(movementVector.x, direction.x * speed, acceleration);
        movementVector.z = SmoothLerp(movementVector.z, direction.z * speed, acceleration);
    }

    private void ApplyGravityAndFriction()
    {
        if (isGrounded && movementVector.y < 0f)
        {
            movementVector.y = 0f;
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
            value = SmoothLerp(value, 0f, frictionAmount);
    }

    private void Jump()
    {
        if (isGrounded || currentJumpCount < maxJumpCount || touchingWall)
        {
            float multiplier = Mathf.Max(1f - 0.3f * currentJumpCount, 0.2f);

            if (touchingWall) // se estiver na parede → usa vetor mais horizontal
            {
                float horizontalBias = 3.5f; // quanto maior, mais horizontal
                Vector3 jumpDir = (Vector3.up + lastWallNormal * horizontalBias).normalized;
                movementVector = jumpForce * 3 * multiplier * jumpDir;
                touchingWall = false; // evita repetir
            }
            else // pulo normal
            {
                movementVector.y = jumpForce * multiplier;
            }

            currentJumpCount++;
        }
    }



    #endregion

    #region --- Dash ---

    private void StartDash()
    {
        dashDirection = moveInput != Vector2.zero ? direction : transform.forward;
        dashDuration = dashDistance / dashSpeed;
        isDashing = true;
        canDash = false;
        movementVector.y = 0f;

        canMove = false;
        PlayDashVisual();
        Invoke(nameof(ResetDash), dashCooldown);
    }

    private void HandleDash()
    {
        characterController.Move(dashSpeed * Time.deltaTime * dashDirection);
        dashDuration -= Time.deltaTime;

        if (dashDuration <= 0f)
        {
            isDashing = false;
            canMove = true;
        }
    }

    private void ResetDash() => canDash = true;

    private void PlayDashVisual()
    {
        DOTween.Sequence()
            .Append(transform.DOScaleY(0.65f, dashDuration * 0.6f))
            .Append(transform.DOScaleY(1f, dashDuration * 0.4f))
            .SetEase(Ease.InOutSine)
            .SetUpdate(UpdateType.Fixed);
    }

    #endregion

    #region --- KNOCKBACK ---
    private Vector3 knockbackVelocity;
    private readonly float knockbackDuration = 0.2f;
    private float knockbackTimer;
    private bool isKnockbackActive;
    private bool isDashBlocked;

    public void ApplyKnockback(Vector3 direction, float force)
    {
        // Aplica o empurrão apenas se não tiver knockback em andamento
        if (isKnockbackActive) return;

        knockbackVelocity = direction * force;
        knockbackTimer = knockbackDuration;
        isKnockbackActive = true;
    }

    private void KnockbackTimer()
    {
        if (isKnockbackActive)
        {
            transform.position += knockbackVelocity * Time.deltaTime;

            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
                isKnockbackActive = false;
        }
    }
    private void BlockPlayerDash()
    {
        if (isDashBlocked) return;
        isDashBlocked = true;
        stats.ModifyStatImmediate<bool>(
            Constants.StatsNames.CanDash.ToString(),
            ModifyTYPE.NEGATIVE,
            QualityTier.COMMON
        );
    }


    private void UnBlockPlayerDash()
    {
        if (!isDashBlocked) return;
        isDashBlocked = false;
        stats.ModifyStatImmediate<bool>(Constants.StatsNames.CanDash.ToString(), ModifyTYPE.POSITIVE, QualityTier.COMMON);
        stats.RemoveActiveModifications(Constants.StatsNames.CanDash.ToString());
    }

    private void BlockPlayerDashToRoutine(float duration)
    {
        if (isDashBlocked) return; // já está bloqueado, não chama de novo

        StartCoroutine(BlockDashCoroutine(duration));
    }

    private IEnumerator BlockDashCoroutine(float duration)
    {
        isDashBlocked = true;

        // Desativa dash
        yield return stats.ModifyStatCoroutine<bool>(
            Constants.StatsNames.CanDash.ToString(),
            ModifyTYPE.NEGATIVE,
            QualityTier.COMMON,
            duration
        );

        // Depois que o ModifyStatCoroutine terminar, libera de novo
        isDashBlocked = false;
    }
    #endregion
    [Header("WALL EXIT")]
    #region === WALLRUNNING ===
    [SerializeField] private float wallExitDuration = .2f; // duração do tempo fora da parede
    private float wallExitTimer = -1f; // começa desativado

    private void WallRunningTimer()
    {
        if (!touchingWall && wallSpeedApplied)
        {
            if (wallExitTimer < 0f)
            {
                wallExitTimer = wallExitDuration;
            }
        }

        if (wallExitTimer >= 0f)
        {
            wallExitTimer -= Time.deltaTime;

            if (wallExitTimer <= 0f)
            {
                stats.RemoveActiveModifications(Constants.StatsNames.Speed.ToString()); // reseta pro base
                wallSpeedApplied = false;
                touchingWall = false;
                UnBlockPlayerDash();
                gravity = initialGravity;
            }
        }
    }

    private void ResetWallExitTimer()
    {
        wallExitTimer = -1;
    }


    #endregion
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag(Constants.Tags.RunningWall.ToString()))
        {
            touchingWall = true;
            currentJumpCount = 1;
            lastWallNormal = hit.normal;

            // só reseta se já estava fora da parede
            if (wallExitTimer >= 0f)
                ResetWallExitTimer();

            if (!wallSpeedApplied)
            {
                stats.RemoveActiveModifications(Constants.StatsNames.Speed.ToString()); // garante que não acumule
                stats.ModifyStatImmediate<float>(
                    Constants.StatsNames.Speed.ToString(),
                    ModifyTYPE.POSITIVE,
                    QualityTier.UNCOMMON
                );
                wallSpeedApplied = true;
                BlockPlayerDash();
            }

            gravity = -3.5f;
        }
        else
        {
            touchingWall = false;
        }

        if (hit.gameObject.TryGetComponent(out Enemies enemy))
        {
            Vector3 knockbackDirection = (transform.position - hit.transform.position).normalized;
            ApplyKnockback(knockbackDirection, enemy.KnockBackForce);
            BlockPlayerDashToRoutine(enemy.DashBlockDuration);
        }
    }

    #region --- HUD & Feedback ---

    private IEnumerator DelayedSetupHUD(float duration)
    {
        yield return new WaitForSeconds(duration);
        SetupHUD();
    }
    private void SetupHUD()
    {
        foreach (var hudObj in GameObject.FindGameObjectsWithTag("HealthHUD"))
        {
            if (hudObj.TryGetComponent(out HealthHUDComponent hud) && hud.IdHealth == ID && hud.HUDType == HealthHUDType.PLAYER)
            {
                _healthHUD = hud;
                float percent = (float)Health / MaxHealth;
             //   hud.ForceSetSlider(percent); // método sem animação

                _OnHealthChanged.AddListener(hud.UpdateDotSlider);
                break;

            }
        }

        if (GameObject.FindWithTag("GameController").TryGetComponent(out HUDDirector hudDir) == true)
        {
            _OnDamage.AddListener(hudDir.ShakeCamera);
        }
    }





    #endregion

    #region Scan
    private void EnemyScan()
    {
        int amount = EnemySpawner.enemySpawner.GetAmountPool();
        for (int i = 0; i < amount; i++)
        {
            GameObject enemytmp = EnemySpawner.enemySpawner.GetDisabledObject();
            if (enemytmp != null)
            {
                float distance = Vector3.Distance(enemytmp.transform.position, transform.position);
                if (distance <= enemyScanRadius)
                {
                    enemytmp.SetActive(true);
                }
            }
        }
    }
    private void EnemyScanTimer()
    {
        if (enemyScanWalker <= enemyScanCooldown)
        {
            enemyScanWalker += Time.deltaTime;
        }
        else
        {
            EnemyScan();
            enemyScanWalker = 0;
        }
    }
    protected RaycastHit playerRayHit;
    protected InteractableObject interactionObject;
    protected Type interactionObjectType;
    // Base
    protected virtual bool ObjectScan()
    {
        if (!selectedcamera)
        {
            SetupCamera();
            return false;
        }

        var ray = new Ray(selectedcamera.transform.position, selectedcamera.transform.forward);
        var layerMask = LayerMask.GetMask("Object");

        if (!Physics.SphereCast(ray, 1.25f, out playerRayHit, 40f, layerMask))
        {
            ClearInteractable();
            return false;
        }

        if (!playerRayHit.collider.TryGetComponent(out interactionObject))
        {
            ClearInteractable();
            return false;
        }

        // Não filtra tipo aqui
        interactionObjectType = interactionObject.GetType();
        interactableRef = interactionObject;
        return true;
    }
    // --- Método auxiliar para limpar estado ---
    protected void ClearInteractable()
    {
        interactableRef = null;
        GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(false, null, ID);
    }

    private void ObjectScanTimer()
    {
        if (interactionScanCooldownWalker <= interactionScanCooldown)
        {
            interactionScanCooldownWalker += Time.deltaTime;
        }
        else
        {
            ObjectScan();
            interactionScanCooldownWalker = 0;
        }
    }
    #endregion


    #region  --- Ataque ---
    [Header("ATAQUE PARÂMETROS")]
    [SerializeField] private float AttackCooldown;
    private float AttackCooldownWalker = 0f;
    private bool canAttack = true;

    protected virtual bool Attack()
    {
        if (!canAttack) return false;
        canAttack = false;
        return true;
    }
    private void AttackTimer()
    {
        if (!canAttack)
        {
            AttackCooldownWalker += Time.deltaTime;
            if (AttackCooldownWalker >= AttackCooldown)
            {
                canAttack = true;
                AttackCooldownWalker = 0f;
            }
        }
    }
    #endregion


    #region --- Utilitários ---

    private float SmoothLerp(float from, float to, float smoothing)
        => Mathf.Lerp(from, to, 1f - Mathf.Exp(-smoothing * Time.deltaTime));

    #endregion

    #region --- Camera ---
    private void SetupCamera()
    {
        Camera[] cameras = Camera.allCameras;
        foreach (Camera camera in cameras)
        {
            camera.TryGetComponent(out CameraLogic cameraLogic);
            if (cameraLogic && cameraLogic.ID == ID)
            {
                selectedcamera = camera;
            }
        }
    }
    #endregion
    #region === DEATH ===
    public override void DeathHandler()
    {
        base.DeathHandler();
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.Invoke(this);
    }

    #endregion
}