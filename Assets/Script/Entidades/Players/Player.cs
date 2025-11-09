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
    [SerializeField] internal QualityTier wallSpeedMultiplier = QualityTier.RARE;
    [SerializeField] internal QualityTier wallSpeedMultiplier = QualityTier.RARE;

    [HideInInspector]
    [Stat(nameof(Speed))]
    public float Speed { get => speed; set => speed = value; }
    internal float acceleration = 5f;
    internal float friction = 2f;
    internal float airFriction = 2f;
    internal float acceleration = 5f;
    internal float friction = 2f;
    internal float airFriction = 2f;


    [Header("Pulo")]
    [SerializeField] private float jumpForce = 10f;
    internal float wallJumpMultiplier = 5;
    internal float wallJumpMultiplier = 5;

    [HideInInspector]
    [Stat(nameof(JumpForce))]
    public float JumpForce { get => jumpForce; set => jumpForce = value; }

    [SerializeField]
    internal int maxJumpCount = 2;
    internal float gravityValue = -16.62f;
    internal float initialGravityValue;
    internal float gravityValue = -16.62f;
    internal float initialGravityValue;

    [Header("Dash")] 
    internal float DashSpeed = 30f;
    [Header("Dash")] 
    internal float DashSpeed = 30f;
    [SerializeField] internal float dashDistance = 5f;
    [SerializeField] internal float dashCooldown = 1f;
    [SerializeField] internal ShiftDashScript dashHUDScript; // adicionado para ter uma animação no Shift

    [Header("Componentes")]
    [SerializeField] protected CharacterController characterController;
    public CharacterController Charactercontroller => characterController;
    [SerializeField] protected CinemachineCamera cinemachineCamera;
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
    internal StackStateMachine<PlayerContext> ActionLayer;

    public PlayerContext Context { get; internal set; }

    internal Vector3 MovementVector;
    internal Vector3 Direction;
    internal Vector3 DashDirection;
    internal Vector2 MoveInput;
    internal Vector3 LastWallNormal;

    internal int CurrentJumpCount;
    internal bool IsGrounded;
    internal bool WallSpeedApplied;
    internal bool TouchingWall;
    internal Vector3 MovementVector;
    internal Vector3 Direction;
    internal Vector3 DashDirection;
    internal Vector2 MoveInput;
    internal Vector3 LastWallNormal;

    internal int CurrentJumpCount;
    internal bool IsGrounded;
    internal bool WallSpeedApplied;
    internal bool TouchingWall;
    internal bool canDash = true;
    internal bool canMove = true;

    [Stat(nameof(CanMove))]
    public bool CanMove { get => canMove; set => canMove = value; } // nova flag para controle de movimento
    public bool CanMove { get => canMove; set => canMove = value; } // nova flag para controle de movimento

    [Stat(nameof(CanDash))]
    public bool CanDash { get => canDash; set => canDash = value; }
    internal bool IsDashing = false;
    internal float dashCount = 1;
    internal float DashCurrent = 0;
    internal float DashDuration;
    public bool CanDash { get => canDash; set => canDash = value; }
    internal bool IsDashing = false;
    internal float dashCount = 1;
    internal float DashCurrent = 0;
    internal float DashDuration;
    #endregion

    #region === EnemyScan ===
    [Header("SCANNER DE SPAWN DE INIMIGOS PARÂMETROS")]
    [SerializeField, Min(10)]
    private float enemyScanRadius = 10;

    [SerializeField, Min(1)]
    private float enemyScanCooldown = 2.0f;
    #endregion

    #region === Interação ===
    [Header("SCANNER DE OBJETOS INTERAGÍVEIS PARÂMETROS")]
    [SerializeField]
    private float interactionScanCooldown = .1f;
    private readonly Timer interactionScanTimer = new();
    internal protected InteractableObject interactableRef;
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
        initialGravityValue = gravityValue;
        initialGravityValue = gravityValue;
        Context = new(this);

        characterController = GetComponent<CharacterController>();
        animatorComp = GetComponent<Animator>();

        VerticalLayer = new(new PlayerFallingState(),Context);
        HorizontalLayer = new(new PlayerHorizontalStateIdle(), Context);
        ActionLayer = new(new PlayerActionStateIdle(), Context);
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
        //ChangeCharacterTimer();

        VerticalLayer.Update(Context);
        HorizontalLayer.Update(Context);
        ActionLayer.Update(Context);


        print(@$"[STATEMACHINE HORIZONTAL - CURRENT STATE : ] {HorizontalLayer.CurrentState}
        [STATEMACHINE VERTICAL - CURRENT STATE : ] {VerticalLayer.CurrentState}
        [STATEMACHINE ACTIONLAYER - CURRENT STATE : ] {ActionLayer.CurrentState}");
    }

    private void FixedUpdate()
    {
        if (!characterController.enabled)
            return;
        IsGrounded = characterController.isGrounded;
        KnockbackTimer();
        HorizontalLayer.FixedUpdate(Context);
        print($"HORIZONTAL LAYER MOVEMENT: {MovementVector}");
        VerticalLayer.FixedUpdate(Context);
        print($"VERTICAL LAYER MOVEMENT: {MovementVector}");
        ActionLayer.FixedUpdate(Context);
        print($"ACTION LAYER MOVEMENT: {MovementVector}");

        // MOVEMENT
        Charactercontroller.Move(MovementVector * Time.deltaTime);

    }

    private void OnDestroy() => DOTween.KillAll();

    #endregion
    #region --- Input Callbacks ---

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        Move();
    }

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
            ActionLayer.PushState(new PlayerActionStateInteraction(), Context);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Attack();
        }
    }

    // public void OnChangeCharacter(InputAction.CallbackContext context)
    // {
    //     float charAxis = context.ReadValue<float>();
    //     print(charAxis + ":" + name);
    //     StartChangeCooldown();
    // }

    // [Header("TROCA DE JOGADOR PARÂMETROS")]
    // [SerializeField]private float changeCharacterCooldown = 5f;
    // private Timer changeCharTimer = new();
    // private bool canChangeCharacter = true;

    // private void ChangeCharacterTimer()
    // {
    //     if (!canChangeCharacter && changeCharTimer.Tick(Time.deltaTime))
    //         canChangeCharacter = true;
    // }

    // private void StartChangeCooldown()
    // {
    //     canChangeCharacter = false;
    //     changeCharTimer.Start(changeCharacterCooldown);
    // } 
    // } 
    #endregion

    #region --- Movimento & Pulo ---
    private void Move()
    {
        if (Cinemachinecamera == null || OverrideGlobal || OverrideHorizontal || HorizontalLayer.CurrentState is PlayerActionStateDash) { return; }
        if (Cinemachinecamera == null || OverrideGlobal || OverrideHorizontal || HorizontalLayer.CurrentState is PlayerActionStateDash) { return; }
        HorizontalLayer.ChangeState(new PlayerHorizontalStateMoviment(), Context);
    }

    private void Jump()
    {
        if (OverrideGlobal) return;

        if (TouchingWall)
        if (OverrideGlobal) return;

        if (TouchingWall)
        {
            // wall-jump permitido — segue pra VerticalLayer.ChangeState(...)
            VerticalLayer.ChangeState(new PlayerJumpingState(), Context);
            // wall-jump permitido — segue pra VerticalLayer.ChangeState(...)
            VerticalLayer.ChangeState(new PlayerJumpingState(), Context);
            return;
        }
        if (!(IsGrounded || CurrentJumpCount < maxJumpCount)) return;

        if (OverrideVertical) return;
        if (!(IsGrounded || CurrentJumpCount < maxJumpCount)) return;

        if (OverrideVertical) return;
        VerticalLayer.ChangeState(new PlayerJumpingState(), Context);


    }
    #endregion

    #region --- Dash ---
    private void StartDash()
    {
        if (isDashBlocked) { return; };
        ActionLayer.PushState(new PlayerActionStateDash(), Context);
    }
    #endregion

    #region --- KNOCKBACK ---
    private Vector3 knockbackVelocity;
    private readonly float knockbackDuration = 0.2f;
    private Timer knockbackTimer = new();
    private bool isKnockbackActive;
    internal bool isDashBlocked;
    internal bool isDashBlocked;

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (isKnockbackActive) return;
        knockbackVelocity = direction * force;
        knockbackTimer.Start(knockbackDuration);
        isKnockbackActive = true;
    }

    private void KnockbackTimer()
    {
        if (!isKnockbackActive) return;

        transform.position += knockbackVelocity * Time.deltaTime;

        if (knockbackTimer.Tick(Time.deltaTime))
            isKnockbackActive = false;
    }

    private void BlockPlayerDashToRoutine(float duration)
    {
        if (isDashBlocked) { return; } // já está bloqueado, não chama de novo
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
    [SerializeField] internal float wallExitDuration = .2f; // duração do tempo fora da parede
    [SerializeField] internal float wallExitDuration = .2f; // duração do tempo fora da parede

    #endregion
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag(Constants.Tags.RunningWall.ToString()))
        {
            LastWallNormal = hit.normal;
            ActionLayer.PushState(new PlayerActionStateWallSliding(), Context);
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

        if (GameObject.FindWithTag("GameController").TryGetComponent(out HudDirector hudDir) == true)
        if (GameObject.FindWithTag("GameController").TryGetComponent(out HudDirector hudDir) == true)
        {
            _OnDamage.AddListener(hudDir.ShakeCamera);
        }
    }

    #endregion

    #region Scan
    private Timer enemyScanTimer = new();
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
        if (enemyScanTimer.Tick(Time.deltaTime))
        {
            EnemyScan();
            enemyScanTimer.Start(enemyScanCooldown);
        }
    }

    protected RaycastHit playerRayHit;
    protected InteractableObject interactionObject;
    protected Type interactionObjectType;

    // Base
    protected virtual bool ObjectScan()
    {
        if (!selectedcamera) { SetupCamera(); return false; }
        var ray = new Ray(selectedcamera.transform.position, selectedcamera.transform.forward);
        var layerMask = LayerMask.GetMask("Object");
        if (!Physics.SphereCast(ray, 1.25f, out playerRayHit, 40f, layerMask) || !playerRayHit.collider.TryGetComponent(out interactionObject))
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
        if (interactionScanTimer.Tick(Time.deltaTime))
        {
            ObjectScan();
            interactionScanTimer.Start(interactionScanCooldown);
        }
    }

    #endregion


    #region  --- Ataque ---
    [Header("ATAQUE PARÂMETROS")]
    [SerializeField] internal float attackCooldown;
    protected internal bool canAttack = true;
    protected virtual void Attack() { }
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
    public int PlayerID { get => player.ID; }
    public PlayerContext(Player player) => this.player = player;

    public Stats PlayerStats { get => player.stats; }

    public Stats PlayerStats { get => player.stats; }
    public GameObject PlayerGameObject { get => player.gameObject; }
    public Transform PlayerTransform { get => player.transform; }
    public CharacterController PlayerController { get => player.Charactercontroller; }
    public CinemachineCamera PlayerCamera { get => player.Cinemachinecamera; }
    public InteractableObject PlayerInteractionReference { get => player.interactableRef; }
    public Animator PlayerAnimator { get => player.AnimatorComp; }
    public float Speed { get => player.Speed; set => player.Speed = value; }
    public QualityTier WallSpeedMultiplier { get => player.wallSpeedMultiplier; }
    public float WallJumpMultiplier { get => player.wallJumpMultiplier; set => player.wallJumpMultiplier = value; }
    public float WallExitDuration { get => player.wallExitDuration; }
    public QualityTier WallSpeedMultiplier { get => player.wallSpeedMultiplier; }
    public float WallJumpMultiplier { get => player.wallJumpMultiplier; set => player.wallJumpMultiplier = value; }
    public float WallExitDuration { get => player.wallExitDuration; }
    public float JumpForce { get => player.JumpForce; set => player.JumpForce = value; }
    public int MaxJumpCount { get => player.maxJumpCount; }
    public float Gravity { get => player.gravityValue; set => player.gravityValue = value; }
    public float InitialGravityValue { get => player.initialGravityValue; }
    public float Gravity { get => player.gravityValue; set => player.gravityValue = value; }
    public float InitialGravityValue { get => player.initialGravityValue; }
    public float DashSpeed { get => player.DashSpeed; set => player.DashSpeed = value; }
    public bool IsDashBlocked { get => player.isDashBlocked; set => player.isDashBlocked = value; }
    public bool IsDashBlocked { get => player.isDashBlocked; set => player.isDashBlocked = value; }
    public float DashCooldown { get => player.dashCooldown; }
    public float DashDistance { get => player.dashDistance; }
    public float Acceleration { get => player.acceleration; set => player.acceleration = value; }
    public float Friction { get => player.friction; set => player.friction = value; }
    public float AirFriction { get => player.airFriction; set => player.airFriction = value; }
    public float Acceleration { get => player.acceleration; set => player.acceleration = value; }
    public float Friction { get => player.friction; set => player.friction = value; }
    public float AirFriction { get => player.airFriction; set => player.airFriction = value; }
    public Vector3 MovementVector { get => player.MovementVector; set => player.MovementVector = value; }
    public Vector3 Direction { get => player.Direction; set => player.Direction = value; }
    public Vector3 DashDirection { get => player.DashDirection; set => player.DashDirection = value; }
    public Vector2 MoveInput { get => player.MoveInput; set => player.MoveInput = value; }
    public Vector3 LastWallNormal { get => player.LastWallNormal; set => player.LastWallNormal = value; }
    public int CurrentJumpCount { get => player.CurrentJumpCount; set => player.CurrentJumpCount = value; }
    public bool IsGrounded { get => player.IsGrounded; set => player.IsGrounded = value; }
    public bool WallSpeedApplied { get => player.WallSpeedApplied; set => player.WallSpeedApplied = value; }
    public bool TouchingWall { get => player.TouchingWall; set => player.TouchingWall = value; }
    public bool CanMove { get => player.CanMove; set => player.CanMove = value; }
    public bool CanDash { get => player.canDash; set => player.canDash = value; }
    public bool CanAttack { get => player.canAttack; set => player.canAttack = value; }
    public ShiftDashScript DashScript { get => player.dashHUDScript; }
    public bool IsDashing { get => player.IsDashing; set => player.IsDashing = value; }
    public float DashCurrent { get => player.DashCurrent; set => player.DashCurrent = value; }
    public float DashDuration { get => player.DashDuration; set => player.DashDuration = value; }
    public float AttackCooldown { get => player.attackCooldown; }
    public StateMachine<PlayerContext> HorizontalLayer { get => player.HorizontalLayer; }
    public StateMachine<PlayerContext> VerticalLayer { get => player.VerticalLayer; }
    public StackStateMachine<PlayerContext> ActionLayer { get => player.ActionLayer; }
    public bool OverrideHorizontal { get => player.OverrideHorizontal; set => player.OverrideHorizontal = value; }
    public bool OverrideVertical { get => player.OverrideVertical; set => player.OverrideVertical = value; }
    public bool OverrideGlobal { get => player.OverrideGlobal; set => player.OverrideGlobal = value; }
}
