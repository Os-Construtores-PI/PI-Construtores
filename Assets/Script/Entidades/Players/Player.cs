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
    #region === Configurações de Movimento ===
    [Header("Movimento")]
     private float speed = 10f;
     internal QualityTier wallSpeedMultiplier = QualityTier.RARE;

    [HideInInspector]
    [Stat(nameof(Speed))]
    public float Speed { get => speed; set => speed = value; }
    internal float acceleration = 5f;
    internal float friction = 2f;
    internal float airFriction = 2f;


    [Header("Pulo")]
     private float jumpForce = 10f;
    internal float wallJumpMultiplier = 5;

    [HideInInspector]
    [Stat(nameof(JumpForce))]
    public float JumpForce { get => jumpForce; set => jumpForce = value; }

    
    internal int maxJumpCount = 2;
    internal float gravityValue = -16.62f;
    internal float initialGravityValue;

    [Header("Dash")] 
    internal float DashSpeed = 30f;
     internal float dashDistance = 5f;
     internal float dashCooldown = 1f;
     internal ShiftDashScript dashHUDScript; // adicionado para ter uma animação no Shift

    [Header("Componentes")]
     protected CharacterController characterController;
    public CharacterController Charactercontroller => characterController;
     protected CinemachineCamera cinemachineCamera;
    public CinemachineCamera Cinemachinecamera => cinemachineCamera;

    public void SetCinemachineCamera(CinemachineCamera cam)
    {
        cinemachineCamera = cam;
    }

    
    protected internal Animator animatorComp;
    
    internal PlayerInput playerInput;
    #endregion

    #region === Overrides ===
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

    #region === Estados Internos ===
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

    internal bool canDash = true;
    internal bool canMove = true;

    private ConditionalGate idleConditional = new();

    [Stat(nameof(CanMove))]
    public bool CanMove { get => canMove; set => canMove = value; } // nova flag para controle de movimento

    [Stat(nameof(CanDash))]
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
    private float enemyScanInterval = 2.0f;
    #endregion

    #region === Interação ===
    [Header("SCANNER DE OBJETOS INTERAGÍVEIS PARÂMETROS")]
    
    private float interactionScanCooldown = .1f;
    private readonly Timer interactionScanTimer = new();
    internal protected InteractableObject interactableRef;
    private Camera selectedcamera = null;
    #endregion

    #region === Inventário ===
    private readonly Inventory inventory = new();
    public Inventory Inventory => inventory;
    #endregion

    #region  === Scanner ===
    private Scanner<Ray,(bool, RaycastHit)> objectScanner;
    private Scanner<Vector3,bool> enemyScanner;
    #endregion
    #region === Inicialização Unity ===
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
        playerInput = GetComponent<PlayerInput>();

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

        objectScanner = new Scanner<Ray, (bool, RaycastHit)>(
            interactionScanCooldown,
            r =>
            {
                bool hit = Physics.SphereCast(r, radius:1.25f, out RaycastHit info,40f,layerMask:LayerMask.GetMask("Object"));
                return (hit, info);
            }
        );
        enemyScanner = new Scanner<Vector3, bool>(
            enemyScanInterval,
            ScanEnemies // <-- injeta o método diretamente
        );

        idleConditional.Setup(() => animatorComp.SetTrigger(Constants.AnimatorTriggerNames.Idle),() => animatorComp.ResetTrigger(Constants.AnimatorTriggerNames.Idle)); 
    }

    public override void Update()
    {
        base.Update();
        ScanEnemies(transform.position);
        ScanObjects();
        KnockbackTimer();
        //ChangeCharacterTimer();

        VerticalLayer.Update(Context);
        HorizontalLayer.Update(Context);
        ActionLayer.Update(Context);
#if DEBUG
        //print(@$"[STATEMACHINE HORIZONTAL - CURRENT STATE : ] {HorizontalLayer.CurrentState}
        //[STATEMACHINE VERTICAL - CURRENT STATE : ] {VerticalLayer.CurrentState}
        //[STATEMACHINE ACTIONLAYER - CURRENT STATE : ] {ActionLayer.CurrentState}");
        // print($"CANATTACK: {canAttack}");
        // print($"WILLATTACK: {willAttack}");
#endif
        TryToSkipDialogue();
    }

    private void FixedUpdate()
    {
        if (!characterController.enabled)
            return;
        IsGrounded = characterController.isGrounded;
        idleConditional.Check(
            VerticalLayer.CurrentState.Type == ActionType.Idle &&
            HorizontalLayer.CurrentState.Type == ActionType.Idle &&
            ActionLayer.CurrentState.Type == ActionType.Idle &&
            IsGrounded
        );
        KnockbackTimer();
        HorizontalLayer.FixedUpdate(Context);
        //print($"HORIZONTAL LAYER MOVEMENT: {MovementVector}");
        VerticalLayer.FixedUpdate(Context);
        //print($"VERTICAL LAYER MOVEMENT: {MovementVector}");
        ActionLayer.FixedUpdate(Context);
        //print($"ACTION LAYER MOVEMENT: {MovementVector}");

        // MOVEMENT
        Charactercontroller.Move(MovementVector * Time.deltaTime);


    }

    private void OnDestroy() => DOTween.KillAll();

    #endregion

    #region  === Dialogue ===

    private void TryToSkipDialogue()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            GlobalEventBus.Instance.PLAYERTRIGGEREDSKIPDIALOGUE.Invoke(Context);
        }
    }

    #endregion
    #region === Input Callbacks ===

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

    public void OnPause(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            Pause();
        }
    }


    // public void OnChangeCharacter(InputAction.CallbackContext context)
    // {
    //     float charAxis = context.ReadValue<float>();
    //     print(charAxis + ":" + name);
    //     StartChangeCooldown();
    // }

    // [Header("TROCA DE JOGADOR PARÂMETROS")]
    // private float changeCharacterCooldown = 5f;
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

    #region === Movimento & Pulo ===
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

    #region  === PAUSE ===
    private void Pause()
    {
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(!GameContext.IsPaused);
    }
    #endregion

    #region === Dash ===
    private void StartDash()
    {
        if (isDashBlocked) { return; }
        ;
        ActionLayer.PushState(new PlayerActionStateDash(), Context);
    }
    #endregion

    #region === KNOCKBACK ===
    private Vector3 knockbackVelocity;
    private readonly float knockbackDuration = 0.2f;
    private Timer knockbackTimer = new();
    private bool isKnockbackActive;
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
     internal float wallExitDuration = .2f; // duração do tempo fora da parede

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

    #region === HUD & Feedback ===

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
        {
            _OnDamage.AddListener(hudDir.ShakeCamera);
        }
    }

    #endregion

    #region Scan
    private bool ScanEnemies(Vector3 playerPos)
    {
        int amount = EnemySpawner.enemySpawner.GetAmountPool();

        for (int i = 0; i < amount; i++)
        {
            GameObject enemytmp = EnemySpawner.enemySpawner.GetDisabledObject();

            if (enemytmp != null)
            {
                float distance = Vector3.Distance(enemytmp.transform.position, playerPos);

                if (distance <= enemyScanRadius)
                {
                    enemytmp.SetActive(true);
                }
            }
        }
        return true; // só para cumprir TOutput
    }

    protected RaycastHit playerRayHit;
    protected InteractableObject interactionObject;
    protected Type interactionObjectType;

    // Base
    protected virtual (bool, RaycastHit) ScanObjects()
    {
        if (!selectedcamera)
        {
            SetupCamera();
            return (false, default);
        }

        // 1 — Monta o ray
        var ray = new Ray(
            selectedcamera.transform.position,
            selectedcamera.transform.forward
        );

        // 2 — Usa o scanner genérico
        var (executed, result) = objectScanner.Scan(Time.deltaTime,ray);


        // 3 — Se o scanner não executou, só retorna "não achou"
        if (!executed || !result.Item1)
        {
            ClearInteractable();
            return (false, default);
        }

        // 4 — Tenta pegar o componente de interação
        if (!result.Item2.collider.TryGetComponent(out interactionObject))
        {
            ClearInteractable();
            return (false, default);
        }

        // 5 — Sucesso
        interactionObjectType = interactionObject.GetType();
        interactableRef = interactionObject;

        return (true, result.Item2);
    }



    // === Método auxiliar para limpar estado ===
    protected void ClearInteractable()
    {
        interactableRef = null;
        GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(false, null, ID);
    }
    #endregion


    #region  === Ataque ===
    [Header("ATAQUE PARÂMETROS")]
     internal float attackCooldown;
    protected internal bool canAttack = true;
    protected internal bool willAttack = true;
    protected virtual void Attack() { }
    #endregion

    #region === Camera ===
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
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.Invoke();
    }
    #endregion

}

public class PlayerContext : CombatEntityContext
{
    private readonly Player player;
    public PlayerContext(Player player) : base(player)
    {
        this.player = player;
    }
    public CharacterController PlayerController { get => player.Charactercontroller; }
    public CinemachineCamera PlayerCamera { get => player.Cinemachinecamera; }
    public InteractableObject PlayerInteractionReference { get => player.interactableRef; }
    public Animator PlayerAnimator { get => player.animatorComp; }
    public PlayerInput PlayerInput { get => player.playerInput;}
    public float PlayerSpeed { get => player.Speed; set => player.Speed = value; }
    public QualityTier PlayerWallSpeedMultiplier { get => player.wallSpeedMultiplier; set => player.wallSpeedMultiplier = value; }
    public float PlayerWallJumpMultiplier { get => player.wallJumpMultiplier; set => player.wallJumpMultiplier = value; }
    public float PlayerWallExitDuration { get => player.wallExitDuration; set => player.wallExitDuration = value; }
    public float PlayerJumpForce { get => player.JumpForce; set => player.JumpForce = value; }
    public int PlayerMaxJumpCount { get => player.maxJumpCount; set => player.maxJumpCount = value; }
    public float PlayerGravity { get => player.gravityValue; set => player.gravityValue = value; }
    public float InitialGravityValue { get => player.initialGravityValue; }
    public float PlayerDashSpeed { get => player.DashSpeed; set => player.DashSpeed = value; }
    public bool IsDashBlocked { get => player.isDashBlocked; set => player.isDashBlocked = value; }
    public float PlayerDashCooldown { get => player.dashCooldown; set => player.dashCooldown = value; }
    public float DashDistance { get => player.dashDistance; }
    public float PlayerAcceleration { get => player.acceleration; set => player.acceleration = value; }
    public float PlayerFriction { get => player.friction; set => player.friction = value; }
    public float PlayerAirFriction { get => player.airFriction; set => player.airFriction = value; }
    public Vector3 PlayerMovementVector { get => player.MovementVector; set => player.MovementVector = value; }
    public Vector3 PlayerDirection { get => player.Direction; set => player.Direction = value; }
    public Vector3 PlayerDashDirection { get => player.DashDirection; set => player.DashDirection = value; }
    public float PlayerDashCurrent { get => player.DashCurrent; set => player.DashCurrent = value; }
    public float PlayerDashDuration { get => player.DashDuration; set => player.DashDuration = value; }
    public Vector2 PlayerMoveInput { get => player.MoveInput; set => player.MoveInput = value; }
    public Vector3 PlayerLastWallNormal { get => player.LastWallNormal; set => player.LastWallNormal = value; }
    public int PlayerCurrentJumpCount { get => player.CurrentJumpCount; set => player.CurrentJumpCount = value; }
    public bool PlayerIsGrounded { get => player.IsGrounded; set => player.IsGrounded = value; }
    public bool PlayerWallSpeedApplied { get => player.WallSpeedApplied; set => player.WallSpeedApplied = value; }
    public bool PlayerTouchingWall { get => player.TouchingWall; set => player.TouchingWall = value; }
    public bool PlayerCanMove { get => player.CanMove; set => player.CanMove = value; }
    public bool PlayerCanDash { get => player.canDash; set => player.canDash = value; }
    public bool PlayerWillAttack { get => player.willAttack; set => player.willAttack = value; }
    public bool PlayerCanAttack { get => player.canAttack; set => player.canAttack = value; }

    public ShiftDashScript PlayerDashScript { get => player.dashHUDScript; }
    public bool PlayerIsDashing { get => player.IsDashing; set => player.IsDashing = value; }
    public float PlayerAttackCooldown { get => player.attackCooldown; set => player.attackCooldown = value; }
    public StateMachine<PlayerContext> PlayerHorizontalLayer { get => player.HorizontalLayer; }
    public StateMachine<PlayerContext> PlayerVerticalLayer { get => player.VerticalLayer; }
    public StackStateMachine<PlayerContext> PlayerActionLayer { get => player.ActionLayer; }
    public bool OverrideHorizontal { get => player.OverrideHorizontal; set => player.OverrideHorizontal = value; }
    public bool OverrideVertical { get => player.OverrideVertical; set => player.OverrideVertical = value; }
    public bool OverrideGlobal { get => player.OverrideGlobal; set => player.OverrideGlobal = value; }
}
