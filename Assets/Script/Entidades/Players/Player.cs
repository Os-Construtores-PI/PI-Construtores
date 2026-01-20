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
    internal float _gravityUpMultiplier   = 2.2f; // sobe rápido, perde força cedo
    internal float _gravityDownMultiplier = 0.6f; // cai mais lento
    internal float _maxFallSpeed          = -26f; // limite da queda
    internal float _initialGravityValue;

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
    public string _ultimoDispositivo = "Keyboard";
    

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

    internal Vector3 _movementVector;
    internal Vector3 _direction;
    internal Vector3 _dashDirection;
    internal Vector2 _moveInput;
    internal Vector3 _lastWallNormal;
    internal Transform _modelTransform;

    internal int _currentJumpCount;
    [SerializeField] internal bool _isGrounded;
    internal bool _wallSpeedApplied;
    internal bool _touchingWall;

    internal bool _canDash = true;
    internal bool _canMove = true;


    [Stat(nameof(CanMove))]
    public bool CanMove { get => _canMove; set => _canMove = value; } // nova flag para controle de movimento

    [Stat(nameof(CanDash))]
    public bool CanDash { get => _canDash; set => _canDash = value; }
    internal bool _isDashing = false;
    internal float _dashCount = 1;
    internal float _dashCurrent = 0;
    internal float _dashDuration;
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





    #region Coletáveis



    // === AMETISTAS ===
    private int amethysts = 0;
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
    #region === Inicialização Unity ===


    public override void Awake()
    {
        base.Awake();
        canPulse = false;
        _initialGravityValue = gravityValue;
        _initialGravityValue = gravityValue;
        Context = new(this);

        characterController = GetComponent<CharacterController>();
        animatorComp = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        //_playerIinpuut = playerInput;
        DetectarDispositivo(playerInput);

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

        _modelTransform = transform.Find("Pandora.014");
    }

    public override void Update()
    {
        base.Update();
        ScanEnemies(transform.position);
        ScanObjects();
        KnockbackTimer();
        //ChangeCharacterTimer();

        if (_moveInput.sqrMagnitude < 0.0001f)
    {
        var gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 stick = gp.leftStick.ReadValue();
            // deadzone pequena
            if (Mathf.Abs(stick.x) > 0.09f || Mathf.Abs(stick.y) > 0.09f)
            {
                _moveInput = stick;
                // opcional: log para ver que o fallback está pegando a entrada
                Debug.Log($"[Fallback] Gamepad stick read: {stick}");
                Move(); // chama Move() como se viesse por callback
            }
        }
    }

        VerticalLayer.Update(Context);
        HorizontalLayer.Update(Context);
        ActionLayer.Update(Context);
#if DEBUG
        // print(@$"[STATEMACHINE HORIZONTAL - CURRENT STATE : ] {HorizontalLayer.CurrentState}
        // //[STATEMACHINE VERTICAL - CURRENT STATE : ] {VerticalLayer.CurrentState}
        // //[STATEMACHINE ACTIONLAYER - CURRENT STATE : ] {ActionLayer.CurrentState}");
        // print($"CANATTACK: {canAttack}");
        // print($"WILLATTACK: {willAttack}");
#endif
        TryToSkipDialogue();
    }

    private void FixedUpdate()
    {
        if (!characterController.enabled)
            return;
        _isGrounded = characterController.isGrounded;
        animatorComp.SetFloat(Constants.AnimatorFloatNames.VelocityY,characterController.velocity.y);
        animatorComp.SetFloat(Constants.AnimatorFloatNames.VelocityX,Vector2.SqrMagnitude(new(characterController.velocity.x,characterController.velocity.z)));
        animatorComp.SetBool(Constants.AnimatorBoolNames.IsGrounded, _isGrounded);
        KnockbackTimer();
        HorizontalLayer.FixedUpdate(Context);
        VerticalLayer.FixedUpdate(Context);
        ActionLayer.FixedUpdate(Context);

        // MOVEMENT
        Charactercontroller.Move(_movementVector * Time.deltaTime);
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
        _moveInput = context.ReadValue<Vector2>();
         
        Move();
    }

    public void LockCamera(bool state)
    {
        Context.CameraLocked = state;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && _canDash && _dashCurrent < _dashCount)
            StartDash();
    }

    private void OnEnable()
    {
        playerInput.onControlsChanged += DetectarDispositivo;

        // Força atualização inicial
        DetectarDispositivo(playerInput);

    // Atualiza no primeiro frame
    
    }

    private void OnDisable()
    {
        playerInput.onControlsChanged -= DetectarDispositivo;
    }

    private void DetectarDispositivo(PlayerInput input)
    {
        string last = input.currentControlScheme;
        

        switch (last)
        {
            case "Keyboard&Mouse":
                _ultimoDispositivo = "Keyboard";
                break;

            case "Gamepad":
                var gp = Gamepad.current;

                if (gp == null)
                {
                    _ultimoDispositivo = "Keyboard";
                    break;
                }

                if (gp.displayName.Contains("DualSense") || gp.displayName.Contains("DualShock"))
                    _ultimoDispositivo = "Playstation";
                else
                    _ultimoDispositivo = "Xbox";

                break;

            default:
                _ultimoDispositivo = "Keyboard";
                break;
        }

        

        GlobalEventBus.Instance.PLAYERINPUTCHANGED.Invoke(_ultimoDispositivo);
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
        HorizontalLayer.ChangeState(new PlayerHorizontalStateMoviment(), Context);
    }

    private void Jump()
    {
        if (OverrideGlobal) return;

        if (_touchingWall)
            if (OverrideGlobal) return;

        if (_touchingWall)
        {
            // wall-jump permitido — segue pra VerticalLayer.ChangeState(...)
            VerticalLayer.ChangeState(new PlayerJumpingState(), Context);
            return;
        }


        if (OverrideVertical) return;
        if (!(_isGrounded || _currentJumpCount < maxJumpCount)) return;
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
        if (_isDashBlocked) { return; }
        ActionLayer.PushState(new PlayerActionStateDash(), Context);
    }
    #endregion

    #region === KNOCKBACK ===
    private Vector3 _knockbackVelocity;
    private readonly float _knockbackDuration = 0.2f;
    private Timer _knockbackTimer = new();
    private bool isKnockbackActive;
    internal bool _isDashBlocked;

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (isKnockbackActive) return;
        _knockbackVelocity = direction * force;
        _knockbackTimer.Start(_knockbackDuration);
        isKnockbackActive = true;
    }

    private void KnockbackTimer()
    {
        if (!isKnockbackActive) return;

        transform.position += _knockbackVelocity * Time.deltaTime;

        if (_knockbackTimer.Tick(Time.deltaTime))
            isKnockbackActive = false;
    }

    private void BlockPlayerDashToRoutine(float duration)
    {
        if (_isDashBlocked) { return; } // já está bloqueado, não chama de novo
        StartCoroutine(BlockDashCoroutine(duration));
    }

    private IEnumerator BlockDashCoroutine(float duration)
    {
        _isDashBlocked = true;
        // Desativa dash
        yield return stats.ModifyStatCoroutine<bool>(
            Constants.StatsNames.CanDash.ToString(),
            ModifyTYPE.NEGATIVE,
            QualityTier.COMMON,
            duration
        );

        // Depois que o ModifyStatCoroutine terminar, libera de novo
        _isDashBlocked = false;
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
            _lastWallNormal = hit.normal;
            ActionLayer.PushState(new PlayerActionStateWallSliding(), Context);
        }
        else
        {
            _touchingWall = false;
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
protected virtual (bool success, RaycastHit hit) ScanObjects()
{
    if (!selectedcamera)
    {
        SetupCamera();
        return (false, default);
    }

    var ray = new Ray(
        selectedcamera.transform.position,
        selectedcamera.transform.forward
    );

    var (executed, scanResult) = objectScanner.Scan(Time.deltaTime, ray);

    if (!executed || !scanResult.Item1)
        return (false, default);

    var hit = scanResult.Item2;

    // tenta pegar o componente
    if (!hit.collider.TryGetComponent(out interactionObject))
        return (false, default);

    // valida range
    if (hit.distance > interactionObject.range)
        return (false, default);

    interactionObjectType = interactionObject.GetType();
    interactableRef = interactionObject;

    return (true, hit);
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
    public Transform PlayerModelTransform {get => player._modelTransform;}
    public PlayerInput PlayerInput { get => player.playerInput;}
    public float PlayerSpeed { get => player.Speed; set => player.Speed = value; }
    public QualityTier PlayerWallSpeedMultiplier { get => player.wallSpeedMultiplier; set => player.wallSpeedMultiplier = value; }
    public float PlayerWallJumpMultiplier { get => player.wallJumpMultiplier; set => player.wallJumpMultiplier = value; }
    public float PlayerWallExitDuration { get => player.wallExitDuration; set => player.wallExitDuration = value; }
    public float PlayerJumpForce { get => player.JumpForce; set => player.JumpForce = value; }
    public int PlayerMaxJumpCount { get => player.maxJumpCount; set => player.maxJumpCount = value; }
    public float PlayerGravity { get => player.gravityValue; set => player.gravityValue = value; }
    public float PlayerGravityUpMultiplier { get => player._gravityUpMultiplier; set => player._gravityUpMultiplier = value;}
    public float PlayerGravityDownMultiplier { get => player._gravityDownMultiplier; set => player._gravityDownMultiplier = value;}
    public float PlayerMaxFallSpeed {get => player._maxFallSpeed; set => player._maxFallSpeed = value;}
    public float InitialGravityValue { get => player._initialGravityValue; }
    public float PlayerDashSpeed { get => player.DashSpeed; set => player.DashSpeed = value; }
    public bool IsDashBlocked { get => player._isDashBlocked; set => player._isDashBlocked = value; }
    public float PlayerDashCooldown { get => player.dashCooldown; set => player.dashCooldown = value; }
    public float DashDistance { get => player.dashDistance; }
    public float PlayerAcceleration { get => player.acceleration; set => player.acceleration = value; }
    public float PlayerFriction { get => player.friction; set => player.friction = value; }
    public float PlayerAirFriction { get => player.airFriction; set => player.airFriction = value; }
    public Vector3 PlayerMovementVector { get => player._movementVector; set => player._movementVector = value; }
    public Vector3 PlayerDirection { get => player._direction; set => player._direction = value; }
    public Vector3 PlayerDashDirection { get => player._dashDirection; set => player._dashDirection = value; }
    public float PlayerDashCurrent { get => player._dashCurrent; set => player._dashCurrent = value; }
    public float PlayerDashDuration { get => player._dashDuration; set => player._dashDuration = value; }
    public Vector2 PlayerMoveInput { get => player._moveInput; set => player._moveInput = value; }
    public Vector3 PlayerLastWallNormal { get => player._lastWallNormal; set => player._lastWallNormal = value; }
    public int PlayerCurrentJumpCount { get => player._currentJumpCount; set => player._currentJumpCount = value; }
    public bool PlayerIsGrounded { get => player._isGrounded; set => player._isGrounded = value; }
    public bool PlayerWallSpeedApplied { get => player._wallSpeedApplied; set => player._wallSpeedApplied = value; }
    public bool PlayerTouchingWall { get => player._touchingWall; set => player._touchingWall = value; }
    public bool PlayerCanMove { get => player.CanMove; set => player.CanMove = value; }
    public bool PlayerCanDash { get => player._canDash; set => player._canDash = value; }
    public bool PlayerWillAttack { get => player.willAttack; set => player.willAttack = value; }
    public bool PlayerCanAttack { get => player.canAttack; set => player.canAttack = value; }

    public ShiftDashScript PlayerDashScript { get => player.dashHUDScript; }
    public bool PlayerIsDashing { get => player._isDashing; set => player._isDashing = value; }
    public float PlayerAttackCooldown { get => player.attackCooldown; set => player.attackCooldown = value; }
    public StateMachine<PlayerContext> PlayerHorizontalLayer { get => player.HorizontalLayer; }
    public StateMachine<PlayerContext> PlayerVerticalLayer { get => player.VerticalLayer; }
    public StackStateMachine<PlayerContext> PlayerActionLayer { get => player.ActionLayer; }
    public bool OverrideHorizontal { get => player.OverrideHorizontal; set => player.OverrideHorizontal = value; }
    public bool OverrideVertical { get => player.OverrideVertical; set => player.OverrideVertical = value; }
    public bool OverrideGlobal { get => player.OverrideGlobal; set => player.OverrideGlobal = value; }
    public GameObject PlayerObject => player.gameObject;
    public bool CameraLocked { get; set; } = false;
    public bool IsHardLocked; // Trava tudo (movimento, dash, ações)
    
}
