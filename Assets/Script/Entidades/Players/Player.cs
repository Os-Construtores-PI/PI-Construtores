using System;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput), typeof(Collider))]
[RequireComponent(typeof(Animator))]
[DefaultExecutionOrder(-100)]
public class Player : CombatEntities
{
    #region --- Configurações de Movimento ---

    [Header("Movimento")]
    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private QualityTier wallSpeedMultiplier = QualityTier.RARE;
    public QualityTier WallSpeedMultiplier { get; internal set; }


    [HideInInspector]
    [Stat(nameof(Speed))]
    public float Speed
    {
        get => speed;
        set => speed = value;
    }
    public float Acceleration { get; internal set; } = 5f;
    public float Friction { get; internal set; } = 2f;
    public float AirFriction { get; internal set; } = 2f;

    [SerializeField]
    internal ShiftDashScript dashHUDScript; // adicionado para ter uma animação no Shift

    [Header("Pulo")]
    [SerializeField]
    private float jumpForce = 10f;
    public float WallJumpMultiplier { get; internal set; } = 5;

    [HideInInspector]
    [Stat(nameof(JumpForce))]
    public float JumpForce
    {
        get => jumpForce;
        set => jumpForce = value;
    }

    [SerializeField]
    private int maxJumpCount = 2;
    public float Gravity { get; internal set; } = -16.62f;
    private float initialGravity;

    [Header("Dash")]
    public float DashSpeed { get; internal set; } = 30f;

    [SerializeField]
    internal float dashDistance = 10f;

    [SerializeField]
    internal float dashCooldown = 5f;

    [Header("Componentes")]
    [SerializeField]
    protected CharacterController characterController;
    public CharacterController Charactercontroller => characterController;

    [SerializeField]
    protected CinemachineCamera cinemachineCamera;
    public CinemachineCamera Cinemachinecamera => cinemachineCamera;

    public void SetCinemachineCamera(CinemachineCamera cam)
    {
        cinemachineCamera = cam;
    }

    [SerializeField]
    protected Animator animatorComp;
    public Animator AnimatorComp => animatorComp;

    #endregion

    #region --- Overrides ---
    // === GLOBAL ===
    public bool OverrideGlobal { get; set; } = false;
    public float GlobalOverride { get; set; } = 0f;

    // === HORIZONTAL ===
    public bool OverrideHorizontal { get; set; } = false;
    public float HorizontalOverride { get; set; } = 0f;

    // === VERTICAL ===
    public bool OverrideVertical { get; set; } = false;
    public float VerticalOverride { get; set; } = 0f;
    #endregion

    #region --- Estados Internos ---
    internal StateMachine<PlayerContext> HorizontalLayer;
    internal StateMachine<PlayerContext> VerticalLayer;
    internal StateMachine<PlayerContext> ActionLayer;

    public PlayerContext Context { get; internal set; }

    public Vector3 MovementVector { get; internal set; }
    public Vector3 Direction { get; internal set; }
    public Vector3 DashDirection { get; internal set; }
    public Vector2 MoveInput { get; internal set; }
    public Vector3 LastWallNormal { get; internal set; }

    public int CurrentJumpCount { get; internal set; }
    public bool IsGrounded { get; internal set; }
    public bool WallSpeedApplied { get; internal set; }
    public bool TouchingWall { get; internal set; }
    internal bool canDash = true;
    internal bool canMove = true;

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
    public bool IsDashing { get; internal set; } = false;
    private float dashCount = 1;
    public float DashCurrent { get; internal set; } = 0;
    public float DashDuration { get; internal set; }
    #endregion


    #region === EnemyScan ===
    [Header("SCANNER DE SPAWN DE INIMIGOS PARÂMETROS")]
    [SerializeField, Min(10)]
    private float enemyScanRadius = 10;

    [SerializeField, Min(1)]
    private float enemyScanCooldown = 2.0f;
    private float enemyScanWalker = 0.0f;
    #endregion


    #region === Interação ===
    [Header("SCANNER DE OBJETOS INTERAGÍVEIS PARÂMETROS")]
    [SerializeField]
    private float interactionScanCooldown = .1f;
    protected InteractableObject interactableRef;
    private float interactionScanCooldownWalker = 0.0f;
    private Camera selectedcamera = null;
    #endregion

    #region === Inventário ===
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
        if (amethysts == value)
            return;
        int oldValue = amethysts;

        amethysts = Mathf.Max(0, value); // evita negativo
        GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.Invoke(amethysts);
    }

    public void AddAmethysts(int amount) => SetAmethysts(amethysts + amount);

    public bool SpendAmethysts(int amount)
    {
        if (amount <= 0 || amethysts < amount)
            return false;
        SetAmethysts(amethysts - amount);
        return true;
    }
    #endregion


    public override void Awake()
    {
        base.Awake();
        canPulse = false;
        initialGravity = Gravity;
        Context = new(this);

        characterController = GetComponent<CharacterController>();
        animatorComp = GetComponent<Animator>();


        VerticalLayer = new();
        HorizontalLayer = new();
        ActionLayer = new();
        SetupCamera();
    }

    public override void Start()
    {
        base.Start();
        DOTween.Init();
        StartCoroutine(DelayedSetupHUD(.1f));

        if (dashHUDScript == null)
        {
            var go = GameObject.FindWithTag("DashHUDIcon");
            if (go)
                dashHUDScript = go.GetComponent<ShiftDashScript>();
            Debug.LogWarning(
                "[Player] DashHUDIcon não encontrado em cena. Arraste a instância ou coloque tag"
            );
        }
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

        VerticalLayer.Update(Context);
        HorizontalLayer.Update(Context);
        ActionLayer.Update(Context);

        //print($"[STATEMACHINE VERTICAL - CURRENT STATE : ] {VerticalLayer.CurrentState}");
        //print("[SPEED] : " + Speed + " // " + "[ACTIVEMODIFICATIONS] : " + stats.GetActiveModifications().Count);
    }

    private void FixedUpdate()
    {
        if (!characterController.enabled)
            return;
        IsGrounded = characterController.isGrounded;

        if (IsDashing)
            HandleDash();
        else
            HandleMovement();

        Vector3 finalMovement = MovementVector;

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            finalMovement += knockbackVelocity;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 5f);
        }
        ActionLayer.FixedUpdate(Context);
        HorizontalLayer.FixedUpdate(Context);
        VerticalLayer.FixedUpdate(Context);
        characterController.Move(finalMovement * Time.deltaTime);
    }

    private void OnDestroy() => DOTween.KillAll();

    #endregion
    #region --- Input Callbacks ---

    public void OnMove(InputAction.CallbackContext context) =>
        MoveInput = context.ReadValue<Vector2>();

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash && DashCurrent < dashCount)
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
    [SerializeField]
    private float ChangeCharacterCooldown = 5f;
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
        if (!CanMove)
            return;
        ApplyRotationAndDirection();
        ApplyGravityAndFriction();
    }

    private void ApplyRotationAndDirection()
    {
        if (Cinemachinecamera == null || MoveInput == Vector2.zero)
        {
            return;
        }
        HorizontalLayer.ChangeState(new PlayerMovimentState(), Context);
        // Movimentação
    }

    private void ApplyGravityAndFriction()
    {
        if (IsGrounded && MovementVector.y < 0f)
        {
            VerticalLayer.ChangeState(new PlayerGroundedState(), Context);
        }
        else
        {
            VerticalLayer.ChangeState(new PlayerFallingState(), Context);
        }
    }

    private void Jump()
    {
        if (!(IsGrounded || CurrentJumpCount < maxJumpCount || TouchingWall) && !OverrideVertical)
        {
            return;
        }
        VerticalLayer.ChangeState(new PlayerJumpingState(), Context);
    }

    #endregion

    #region --- Dash ---

    private void StartDash()
    {
        HorizontalLayer.ChangeState(new PlayerDashState(), Context);
    }


    private void HandleDash()
    {
        characterController.Move(DashSpeed * Time.deltaTime * DashDirection);
        DashDuration -= Time.deltaTime;

        if (DashDuration <= 0f)
        {
            IsDashing = false;
            canMove = true;
        }
    }

    private void ResetDash() => canDash = true;

    private void PlayDashVisual()
    {
        DOTween
            .Sequence()
            .Append(transform.DOScaleY(0.65f, DashDuration * 0.6f))
            .Append(transform.DOScaleY(1f, DashDuration * 0.4f))
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
        if (isKnockbackActive)
            return;

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
        if (isDashBlocked)
            return;
        isDashBlocked = true;
        stats.ModifyStatImmediate<bool>(
            Constants.StatsNames.CanDash.ToString(),
            ModifyTYPE.NEGATIVE,
            QualityTier.COMMON
        );
    }

    private void UnBlockPlayerDash()
    {
        if (!isDashBlocked)
            return;
        isDashBlocked = false;
        stats.ModifyStatImmediate<bool>(
            Constants.StatsNames.CanDash.ToString(),
            ModifyTYPE.POSITIVE,
            QualityTier.COMMON
        );
        stats.RemoveActiveModifications(Constants.StatsNames.CanDash.ToString());
    }

    private void BlockPlayerDashToRoutine(float duration)
    {
        if (isDashBlocked)
            return; // já está bloqueado, não chama de novo

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
    [SerializeField]
    private float wallExitDuration = .2f; // duração do tempo fora da parede
    private float wallExitTimer = -1f; // começa desativado

    private void WallRunningTimer()
    {
        if (!TouchingWall && WallSpeedApplied)
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
                WallSpeedApplied = false;
                TouchingWall = false;
                UnBlockPlayerDash();
                Gravity = initialGravity;
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
            TouchingWall = true;
            CurrentJumpCount = 1;
            LastWallNormal = hit.normal;

            // só reseta se já estava fora da parede
            if (wallExitTimer >= 0f)
                ResetWallExitTimer();

            if (!WallSpeedApplied)
            {
                stats.RemoveActiveModifications(Constants.StatsNames.Speed.ToString()); // garante que não acumule
                stats.ModifyStatImmediate<float>(
                    Constants.StatsNames.Speed.ToString(),
                    ModifyTYPE.POSITIVE,
                    wallSpeedMultiplier
                );
                WallSpeedApplied = true;
                BlockPlayerDash();
            }

            Gravity = -2f;
        }
        else
        {
            TouchingWall = false;
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
            if (
                hudObj.TryGetComponent(out HealthHUDComponent hud)
                && hud.IdHealth == ID
                && hud.HUDType == HealthHUDType.PLAYER
            )
            {
                _healthHUD = hud;
                _healthHUD.BindToPlayer(this);
                break;
            }
        }

        if (
            GameObject.FindWithTag("GameController").TryGetComponent(out HUDDirector hudDir) == true
        )
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
    [SerializeField]
    private float AttackCooldown;
    private float AttackCooldownWalker = 0f;
    private bool canAttack = true;

    protected virtual bool Attack()
    {
        if (!canAttack)
            return false;
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

public class PlayerContext
{
    private readonly Player player;

    public PlayerContext(Player player) => this.player = player;

    public Transform PlayerTransform
    {
        get => player.transform;
    }

    public CharacterController PlayerController
    {
        get => player.Charactercontroller;
    }

    public CinemachineCamera PlayerCamera
    {
        get => player.Cinemachinecamera;
    }

    public float Speed
    {
        get => player.Speed;
        set => player.Speed = value;
    }

    public QualityTier WallSpeedMultiplier
    {
        get => player.WallSpeedMultiplier;
    }

    public float WallJumpMultiplier
    {
        get => player.WallJumpMultiplier;
        set => player.WallJumpMultiplier = value;
    }

    public float JumpForce
    {
        get => player.JumpForce;
        set => player.JumpForce = value;
    }

    public float Gravity
    {
        get => player.Gravity;
        set => player.Gravity = value;
    }

    public float DashSpeed
    {
        get => player.DashSpeed;
        set => player.DashSpeed = value;
    }

    public float DashCooldown
    {
        get => player.dashCooldown;
    }

    public float DashDistance
    {
        get => player.dashDistance;
    }

    public float Acceleration
    {
        get => player.Acceleration;
        set => player.Acceleration = value;
    }

    public float Friction
    {
        get => player.Friction;
        set => player.Friction = value;
    }

    public float AirFriction
    {
        get => player.AirFriction;
        set => player.AirFriction = value;
    }

    public Vector3 MovementVector
    {
        get => player.MovementVector;
        set => player.MovementVector = value;
    }

    public Vector3 Direction
    {
        get => player.Direction;
        set => player.Direction = value;
    }

    public Vector3 DashDirection
    {
        get => player.DashDirection;
        set => player.DashDirection = value;
    }

    public Vector2 MoveInput
    {
        get => player.MoveInput;
        set => player.MoveInput = value;
    }

    public Vector3 LastWallNormal
    {
        get => player.LastWallNormal;
        set => player.LastWallNormal = value;
    }

    public int CurrentJumpCount
    {
        get => player.CurrentJumpCount;
        set => player.CurrentJumpCount = value;
    }

    public bool IsGrounded
    {
        get => player.IsGrounded;
        set => player.IsGrounded = value;
    }

    public bool WallSpeedApplied
    {
        get => player.WallSpeedApplied;
        set => player.WallSpeedApplied = value;
    }

    public bool TouchingWall
    {
        get => player.TouchingWall;
        set => player.TouchingWall = value;
    }

    public bool CanMove
    {
        get => player.CanMove;
        set => player.CanMove = value;
    }
    public bool CanDash
    {
        get => player.canDash;
        set => player.canDash = value;
    }

    public ShiftDashScript DashScript
    {
        get => player.dashHUDScript;
    }

    public bool IsDashing
    {
        get => player.IsDashing;
        set => player.IsDashing = value;
    }

    public float DashCurrent
    {
        get => player.DashCurrent;
        set => player.DashCurrent = value;
    }

    public float DashDuration
    {
        get => player.DashDuration;
        set => player.DashDuration = value;
    }

    public StateMachine<PlayerContext> HorizontalLayer
    {
        get => player.HorizontalLayer;
    }
    
    public StateMachine<PlayerContext> VerticalLayer
    {
        get => player.VerticalLayer;
    }

    public StateMachine<PlayerContext> ActionLayer
    {
        get => player.ActionLayer;
    }
}
