using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.SceneManagement;
using static Constants.PlayerShakes;
using static TutorialGlobal;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput), typeof(Collider))]
[RequireComponent(typeof(Animator), typeof(AudioSource))]
[DefaultExecutionOrder(-100)]
public class Player : CombatEntities
{
  // ─────────────────────────────────────────────────────────────
  //  MOVIMENTO
  // ─────────────────────────────────────────────────────────────
  #region Movimento – Stats
  private float _speed = 10f;
  private float _runningSpeed = 20f;

  [HideInInspector]
  [Stat(nameof(Speed))]
  public float Speed
  {
    get => _speed;
    set => _speed = value;
  }

  [HideInInspector]
  [Stat(nameof(RunningSpeed))]
  public float RunningSpeed
  {
    get => _runningSpeed;
    set => _runningSpeed = value;
  }

  [HideInInspector]
  public QualityTier WallSpeedMultiplier = QualityTier.RARE;

  [HideInInspector]
  public float Acceleration = 5f;

  [HideInInspector]
  public float AccelerationRunning = 10f;

  [HideInInspector]
  public float Friction = 2f;

  [HideInInspector]
  public float AirFriction = 2f;
  #endregion

  #region Movimento – Pulo
  private float _jumpForce = 10f;

  [HideInInspector]
  [Stat(nameof(JumpForce))]
  public float JumpForce
  {
    get => _jumpForce;
    set => _jumpForce = value;
  }

  internal int MaxJumpCount = 2;
  internal float WallJumpMultiplier = 5f;
  internal float GravityValue;
  internal float GravityUpMultiplier = 2.2f;
  internal float GravityDownMultiplier = 0.6f;
  internal float MaxFallSpeed = -26f;
  internal float InitialGravityValue;
  #endregion

  #region Movimento – Dash
  internal float DashSpeed = 30f;
  internal float DashDistance = 5f;
  internal float DashCooldown = 1f;
  internal ShiftDashScript DashHudScript;
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  COMPONENTES
  // ─────────────────────────────────────────────────────────────
  #region Componentes

  [Header("Componentes")]
  [SerializeField]
  private Transform _cameraTarget;

  [SerializeField]
  private AudioSource _playerAudioSource;

  [HideInInspector]
  public CharacterController CharacterController;

  [HideInInspector]
  public CinemachineCamera MainCamera;

  [HideInInspector]
  public CinemachineCamera BoostCamera;

  [HideInInspector]
  public CinemachineInputAxisController _cinemachineInput;

  [HideInInspector]
  public CinemachineOrbitalFollow _cinemachineOrbital;

  public HurtboxComponent HurtboxCollider;
  public Collider DashHitboxCollider;
  public Collider GroundSlamHitboxCollider;
  public Animator AnimatorComponent;
  public PlayerInput PlayerInput;

  protected Camera _myCamera;

  public void SetCamera(CinemachineCamera mainCam, CinemachineCamera boostCam, Camera camera)
  {
    MainCamera = mainCam;
    BoostCamera = boostCam;
    _myCamera = camera;
  }
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  STATE MACHINES
  // ─────────────────────────────────────────────────────────────
  #region State Machines & Estados
  public StateMachine<Player> LocomotionLayer = new();
  public StackStateMachine<Player> ActionLayer = new();

  // Action states
  public readonly PlayerActionStateDash Dash = new();
  public readonly PlayerActionStateInteraction Interaction = new();
  public readonly PlayerActionStateWallSliding WallSliding = new();
  public readonly PlayerActionStateGroundSlam GroundSlam = new();
  public readonly PlayerActionStateBoost Boost = new();
  public readonly PlayerActionStateBounce Bounce = new();
  public readonly PlayerActionStateJump Jump = new();
  public BoostSlashDashButton DashSlashBoostButton;

  // Locomotion states
  public readonly PlayerLocomotionStateGrounded GroundedS = new();
  public readonly PlayerLocomotionStateAirborne AirborneS = new();
  public readonly PlayerLocomotionStateLocked LockedS = new();
  public readonly PlayerLocomotionStateHLocked HLockedS = new();
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  ESTADO INTERNO DO PLAYER
  // ─────────────────────────────────────────────────────────────
  #region Estado Interno
  [HideInInspector]
  public Vector3 MovementVector;

  [HideInInspector]
  public Vector3 Direction;

  [HideInInspector]
  public Vector3 DashDirection;

  [HideInInspector]
  public Vector2 MoveInput;

  [HideInInspector]
  public Vector3 LastWallNormal;

  [HideInInspector]
  public bool IsRunning;

  [HideInInspector]
  public bool IsImpulsioned;

  [HideInInspector]
  public bool WallSpeedApplied;

  [HideInInspector]
  public bool TouchingWall;

  [HideInInspector]
  public bool IsDashBlocked;

  [HideInInspector]
  public int CurrentJumpCount = 0;

  [SerializeField]
  internal bool IsGrounded;

  private bool _canMove = true;
  private bool _canDash = true;

  [Stat(nameof(CanMove))]
  public bool CanMove
  {
    get => _canMove;
    set => _canMove = value;
  }

  [Stat(nameof(CanDash))]
  public bool CanDash
  {
    get => _canDash;
    set => _canDash = value;
  }

  [HideInInspector]
  public bool IsDashing = false;

  [HideInInspector]
  public float MaxDashCount = 1f;

  [HideInInspector]
  public float CurrentDashCount = 0f;

  [HideInInspector]
  public float DashDuration;

  [HideInInspector]
  public float GroundSlamImpactSpeed { get; set; } = 0f;
  public Transform _modelTransform;

  public InputType _ultimoDispositivo = InputType.Keyboard;
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  FLAGS DE INPUT DE PULO
  // ─────────────────────────────────────────────────────────────
  #region Flags de Input – Pulo
  public bool JumpInteractionPressed = false;

  public void ConsumeJumpInteraction() => JumpInteractionPressed = false;
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  Trails
  // ─────────────────────────────────────────────────────────────
  #region Trails
  public TrailsWorker TrailsSystem = new();
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  FLAGS DE CONTEXTO
  // ─────────────────────────────────────────────────────────────
  #region Flags de Contexto
  public bool CameraLocked { get; set; } = false;
  public bool IsHardLocked { get; set; } = false;
  public bool IgnoreGameplayInputThisFrame { get; set; } = false;
  public bool WaitForJumpRelease { get; set; } = false;
  public bool BlockJumpByDialogue { get; set; } = false;
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  SCANNERS
  // ─────────────────────────────────────────────────────────────
  #region Scanners – Declarações
  [Header("Scanner – Inimigos")]
  [SerializeField, Min(10)]
  private float enemyScanRadius = 10f;

  // Parâmetros do camera scanner (centralizados para fácil ajuste)
  private const float CameraScanSphereRadius = 6f;
  private const float CameraScanMaxDistance = 20f;
  private const float CameraScanDotThreshold = 0.5f;

  // Parâmetro do wall scanner
  private const float WallScanDistance = 5f;

  // Parâmetros do ground check em TryJump
  private const float GroundCheckMaxDistance = 50f;
  private const float GroundProximityThreshold = 1.1f;
  private const float CoyoteTimeThreshold = 0.2f;

  private Camera _selectedCamera = null;
  private readonly RaycastHit[] _sphereCastResults = new RaycastHit[20];

  private Scanner<Ray, (bool, RaycastHit)> _cameraScanner;
  private Scanner<Vector3, bool> _enemyScanner;
  private Scanner<(Ray, Ray), RaycastHit?> _wallScanner;
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  LOCK-ON
  // ─────────────────────────────────────────────────────────────
  #region Lock-On
  public ILockable LockedTarget;
  private ILockable _lockCandidate;
  private RaycastHit _lastLockHit;
  private bool _isLockOnActive = false;
  protected RaycastHit _playerRayHit;
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  INTERAÇÃO
  // ─────────────────────────────────────────────────────────────
  #region Interação
  [HideInInspector]
  public InteractableObject InteractionObject;
  protected InteractableObject _lastInteractionObject;
  protected Type _interactionObjectType;
  protected (bool success, RaycastHit hit) _lastValidResult;
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  INVENTÁRIO & COLETÁVEIS
  // ─────────────────────────────────────────────────────────────
  #region Inventário
  private readonly Inventory _inventory = new();
  public Inventory Inventory => _inventory;
  #endregion

  #region Coletáveis – Ametistas
  private int amethysts = 0;
  public int Amethysts => amethysts;

  public void SetAmethysts(int value, Vector3? amethystPos)
  {
    if (amethysts == value)
      return;

    Vector3? positionInCamera = amethystPos.HasValue
      ? _myCamera.WorldToScreenPoint(amethystPos.Value)
      : (Vector3?)null;

    amethysts = Mathf.Max(0, value);
    GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.Invoke(amethysts, positionInCamera);
  }

  public void AddAmethysts(int amount, Vector3? amethystPos) =>
    SetAmethysts(amethysts + amount, amethystPos);

  public bool SpendAmethysts(int amount)
  {
    if (amount <= 0 || amethysts < amount)
      return false;
    SetAmethysts(amethysts - amount, null);
    return true;
  }
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  EVENTOS
  // ─────────────────────────────────────────────────────────────
  #region Eventos
  public readonly UnityEvent<bool> SpeedLines = new();
  public readonly UnityEvent<bool> RunningShake = new();
  public readonly UnityEvent<int, float, float, float> CustomShake = new();
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  EVENTOS
  // ─────────────────────────────────────────────────────────────
  #region Sons
  [Header("Sons do Jogador")]
  [SerializeField]
  private List<PlayerSFX> _playerSFX = new();

  public readonly SoundsWorker<PlayerAudioType> PlayerSoundSystem = new();
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  ATAQUE
  // ─────────────────────────────────────────────────────────────
  #region Ataque
  internal float AttackCooldown;
  public bool CanAttack = true;
  public bool WillAttack = true;

  private void Attack()
  {
    if (CanDash && LockedTarget != null)
    {
      ActionLayer.PushState(Dash, this);
      return;
    }

    if (CanExecuteAttack())
      OnExecuteAttack();
  }

  protected virtual bool CanExecuteAttack() => true;

  protected virtual void OnExecuteAttack() { }
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  WALL RUNNING
  // ─────────────────────────────────────────────────────────────
  #region Wall Running
  [Header("Wall Exit")]
  internal float WallExitDuration = 0.2f;
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  CAMERA
  // ─────────────────────────────────────────────────────────────
  #region Camera – Configuração
  [Header("Inverter Y Camera")]
  [SerializeField]
  private bool _willInvertYAxis = false;

  private void SetupCamera()
  {
    foreach (Camera cam in Camera.allCameras)
    {
      if (cam.TryGetComponent(out CameraLogic cameraLogic) && cameraLogic.ID == ID)
      {
        _selectedCamera = cam;
        return;
      }
    }
    Debug.LogError("[Player] Câmera com ID correspondente não encontrada.");
  }

  public void LockCamera(bool state) => CameraLocked = state;
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  UNITY LIFECYCLE
  // ─────────────────────────────────────────────────────────────
  #region Unity – Awake / Start / Update / FixedUpdate / OnDestroy
  public override void Awake()
  {
    base.Awake();
    canPulse = false;
    GravityValue = -16.62f;
    InitialGravityValue = GravityValue;

    CharacterController = GetComponent<CharacterController>();
    AnimatorComponent = GetComponent<Animator>();
    PlayerInput = GetComponent<PlayerInput>();

    DetectarDispositivo(PlayerInput);
    DashSlashBoostButton = new(this, 100, 20, .5f);
    LocomotionLayer.ChangeState(GroundedS, this);
  }

  public override void Start()
  {
    base.Start();
    DOTween.Init();
    SetVisibilityLockOnOverlay(false);
    StartCoroutine(DelayedSetupHUD(.1f));
    SetupDashHUD();
    SetupCinemachine();
    SetupScanners();

    // Systems
    TrailsSystem.InitTrails(transform.Find("Trails"));
    PlayerSoundSystem.Init(_playerSFX, playersfx => playersfx.Type, _playerAudioSource);

    _modelTransform = transform.Find("Model");
  }

  public override void Update()
  {
    base.Update();
#if UNITY_EDITOR
    if (Input.GetKeyDown(KeyCode.F1))
      SceneManager.LoadScene(SceneManager.GetActiveScene().name);
#endif
    LocomotionLayer.Update(this);
    ActionLayer.Update(this);
    DashSlashBoostButton.Update();
    ScanWithCamera();
  }

  public void FixedUpdate()
  {
    if (!CharacterController.enabled)
      return;

    IsGrounded = CharacterController.isGrounded;
    UpdateAnimator();
    LocomotionLayer.FixedUpdate(this);
    ActionLayer.FixedUpdate(this);
    CharacterController.Move(MovementVector * Time.deltaTime);
  }

  public void OnDestroy() => DOTween.Kill(this);
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  INICIALIZAÇÃO AUXILIAR (Start helpers)
  // ─────────────────────────────────────────────────────────────
  #region Inicialização Auxiliar
  private void UpdateAnimator()
  {
    Vector3 vel = CharacterController.velocity;
    AnimatorComponent.SetFloat(Constants.AnimatorFloatNames.VelocityY, vel.y);
    AnimatorComponent.SetFloat(
      Constants.AnimatorFloatNames.VelocityX,
      new Vector2(vel.x, vel.z).sqrMagnitude
    );
    AnimatorComponent.SetBool(Constants.AnimatorBoolNames.IsGrounded, IsGrounded);
  }

  private void SetupDashHUD()
  {
    if (DashHudScript != null)
      return;

    GameObject go = GameObject.FindWithTag("DashHUDIcon");
    if (go)
      DashHudScript = go.GetComponent<ShiftDashScript>();
    else
      Debug.LogWarning(
        "[Player] DashHUDIcon não encontrado em cena. Arraste a instância ou coloque tag."
      );
  }

  private void SetupCinemachine()
  {
    InputAction lookAction = InputSystem.actions.FindAction("Look");
    lookAction.ApplyParameterOverride((InvertVector2Processor p) => p.invertY, _willInvertYAxis);
    _cinemachineInput = MainCamera.GetComponent<CinemachineInputAxisController>();
    _cinemachineOrbital = MainCamera.GetComponent<CinemachineOrbitalFollow>();
  }

  private void SetupScanners()
  {
    TickDirector.Instance.OnFiveTick.AddListener(_ => _enemyScanner.Scan(transform.position));
    TickDirector.Instance.OnFiveTick.AddListener(_ => ScanWalls());

    DashSlashBoostButton.StartedChargingEv.AddListener(() =>
      EffectsSystem.PlayEffect(EffectType.ChargingEffect, 1)
    );
    DashSlashBoostButton.StoppedChargingEv.AddListener(() =>
      EffectsSystem.StopEffect(EffectType.ChargingEffect)
    );

    _cameraScanner = new Scanner<Ray, (bool, RaycastHit)>(BuildCameraScanner());
    _enemyScanner = new Scanner<Vector3, bool>(ScanEnemies);
    _wallScanner = new Scanner<(Ray, Ray), RaycastHit?>(ScanWallRays);
  }

  private RaycastHit? ScanWallRays((Ray left, Ray right) rays)
  {
    int mask = LayerMask.GetMask("RunningWall");
    var interaction = QueryTriggerInteraction.Ignore;

    if (Physics.Raycast(rays.left, out RaycastHit hit, WallScanDistance, mask, interaction))
      return hit;
    if (Physics.Raycast(rays.right, out hit, WallScanDistance, mask, interaction))
      return hit;

    return null;
  }

  private Func<Ray, (bool, RaycastHit)> BuildCameraScanner() =>
    ray =>
    {
      LayerMask targetsMask = LayerMask.GetMask("Object", "Entity");
      LayerMask obstacleMask = LayerMask.GetMask("Default");

      int hitCount = Physics.SphereCastNonAlloc(
        ray.origin,
        CameraScanSphereRadius,
        ray.direction,
        _sphereCastResults,
        CameraScanMaxDistance,
        targetsMask
      );

      Collider bestTarget = null;
      float closestDistance = float.MaxValue;

      for (int i = 0; i < hitCount; i++)
      {
        Collider col = _sphereCastResults[i].collider;
        if (col.CompareTag("Player"))
          continue;

        Vector3 targetCenter = col.bounds.center;
        float dot = Vector3.Dot(ray.direction.normalized, (targetCenter - ray.origin).normalized);
        if (dot < CameraScanDotThreshold)
          continue;

        float distance = Vector3.Distance(ray.origin, targetCenter);
        if (distance > CameraScanMaxDistance)
          continue;

        if (!Physics.Linecast(ray.origin, targetCenter, obstacleMask) && distance < closestDistance)
        {
          closestDistance = distance;
          bestTarget = col;
        }
      }

      if (bestTarget != null)
      {
        Vector3 finalDir = (bestTarget.bounds.center - ray.origin).normalized;
        if (
          Physics.Raycast(
            ray.origin,
            finalDir,
            out RaycastHit finalHit,
            CameraScanMaxDistance + 2f,
            targetsMask | obstacleMask
          )
        )
        {
          if ((targetsMask.value & (1 << finalHit.collider.gameObject.layer)) != 0)
            return (true, finalHit);
        }
      }

      return (false, default);
    };
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  INPUT CALLBACKS
  // ─────────────────────────────────────────────────────────────
  #region Input Callbacks
  public void OnMove(InputAction.CallbackContext context)
  {
    if (IgnoreGameplayInputThisFrame)
      return;
    MoveInput = context.ReadValue<Vector2>();
  }

  public void OnDash(InputAction.CallbackContext context) =>
    DashSlashBoostButton.OnInputAction(context);

  public void OnGroundSlam(InputAction.CallbackContext context)
  {
    if (context.performed && !IsGrounded)
      ActionLayer.PushState(GroundSlam, this);
  }

  public void OnRunning(InputAction.CallbackContext context)
  {
    if (context.performed)
    {
      IsRunning = true;
      SpeedLines.Invoke(true);

      RunningShake.Invoke(true);
      TrailsSystem.PlayEffect(TrailType.MovementTrail);
    }
    else if (context.canceled)
    {
      IsRunning = false;
      RunningShake.Invoke(false);
      TrailsSystem.StopEffect(TrailType.MovementTrail);
      SpeedLines.Invoke(false);
    }
  }

  public void OnJump(InputAction.CallbackContext context)
  {
    if (IsHardLocked || IgnoreGameplayInputThisFrame || BlockJumpByDialogue)
      return;

    if (WaitForJumpRelease)
    {
      if (context.canceled)
        WaitForJumpRelease = false;
      return;
    }

    if (!context.started)
      return;

    if (!IsGrounded)
      JumpInteractionPressed = true;

    TryJump();
  }

  public void OnInteract(InputAction.CallbackContext context)
  {
    if (InteractionObject && context.started)
      ActionLayer.PushState(Interaction, this);
    GlobalEventBus.Instance.PLAYERTRIGGEREDSKIPDIALOGUE.Invoke(this);
  }

  public void OnAttack(InputAction.CallbackContext context)
  {
    if (context.started)
      Attack();
  }

  public void OnPause(InputAction.CallbackContext context)
  {
    if (context.started)
      Pause();
  }

  public void OnEnable()
  {
    PlayerInput.onControlsChanged += DetectarDispositivo;
    DetectarDispositivo(PlayerInput);
  }

  public void OnDisable()
  {
    PlayerInput.onControlsChanged -= DetectarDispositivo;
  }

  private void DetectarDispositivo(PlayerInput input)
  {
    switch (input.currentControlScheme)
    {
      case "Keyboard&Mouse":
        _ultimoDispositivo = InputType.Keyboard;
        break;
      case "Gamepad":
        var gp = Gamepad.current;
        if (gp == null)
          break;
        _ultimoDispositivo =
          (gp.displayName.Contains("DualSense") || gp.displayName.Contains("DualShock"))
            ? InputType.JoystickPlaystation
            : InputType.JoystickXbox;
        break;
      default:
        _ultimoDispositivo = InputType.Keyboard;
        break;
    }
    GlobalEventBus.Instance.PLAYERINPUTCHANGED.Invoke(_ultimoDispositivo.ToString());
  }
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  PULO
  // ─────────────────────────────────────────────────────────────
  #region Pulo
  private void TryJump()
  {
    if (
      DialogueGlobal.Instance != null
      && (
        DialogueGlobal.Instance.IsDialogueActive
        || DialogueGlobal.Instance._bloquearJumpTemporariamente
      )
    )
      return;

    if (CurrentJumpCount >= MaxJumpCount)
      return;

    if (CurrentJumpCount > 0)
    {
      ActionLayer.PushState(Jump, this);
      return;
    }

    bool didHit = Physics.Raycast(
      new Ray(transform.position, Vector3.down),
      out RaycastHit hit,
      GroundCheckMaxDistance,
      LayerMask.GetMask("Default", "Ground"),
      QueryTriggerInteraction.Ignore
    );

    if (!didHit)
      return;

    float distanceToGround = hit.distance;
    float velocityY = CharacterController.velocity.y;

    if (distanceToGround <= GroundProximityThreshold || velocityY > 0.01f)
    {
      ActionLayer.PushState(Jump, this);
      return;
    }

    if (velocityY < -0.01f)
    {
      float timeToReach = distanceToGround / Mathf.Abs(velocityY);
      if (timeToReach <= CoyoteTimeThreshold)
        ActionLayer.PushState(Jump, this);
    }
  }
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  LOCK-ON (métodos)
  // ─────────────────────────────────────────────────────────────
  #region Lock-On – Métodos
  private void SetLockOn(ILockable target)
  {
    LockedTarget = target;
    bool active = LockedTarget != null;
    SetVisibilityLockOnOverlay(active);
    _isLockOnActive = active;
  }

  private void SetVisibilityLockOnOverlay(bool set)
  {
    Vector3 targetScreenPosition = set
      ? _myCamera.WorldToScreenPoint(LockedTarget.transform.position)
      : Vector3.zero;
    GlobalEventBus.Instance.PLAYERTRIGGEREDLOCKONVISIBILITY.Invoke(ID, set, targetScreenPosition);
  }

  public void DisableLockIn()
  {
    if (!_isLockOnActive)
      return;
    _isLockOnActive = false;
    SetLockOn(null);
  }
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  PAUSE
  // ─────────────────────────────────────────────────────────────
  #region Pause
  private void Pause()
  {
    if (TutorialGlobal.Instance != null && TutorialGlobal.Instance.IsTutorialActive)
      return;
    if (DialogueGlobal.Instance != null && DialogueGlobal.Instance.IsDialogueActive)
      return;
    GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(!GameState.IsPaused);
  }
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  SCAN (câmera, inimigos, paredes)
  // ─────────────────────────────────────────────────────────────
  #region Scan
  private void ScanWalls()
  {
    (bool executed, RaycastHit? hit) = _wallScanner.Scan(
      (new Ray(transform.position, transform.right), new Ray(transform.position, -transform.right))
    );

    if (!executed)
      return;

    if (hit.HasValue)
    {
      ActionLayer.PushState(WallSliding, this);
      LastWallNormal = hit.Value.normal;
    }
    else
    {
      TouchingWall = false;
    }
  }

  private bool ScanEnemies(Vector3 playerPos)
  {
    int amount = EnemySpawner.enemySpawner.GetAmountPool();
    for (int i = 0; i < amount; i++)
    {
      GameObject enemy = EnemySpawner.enemySpawner.GetDisabledObject();
      if (enemy == null)
        continue;
      if (Vector3.Distance(enemy.transform.position, playerPos) <= enemyScanRadius)
        enemy.SetActive(true);
    }
    return true;
  }

  protected virtual (bool success, RaycastHit hit) ScanWithCamera()
  {
    if (!_selectedCamera)
    {
      SetupCamera();
      return (false, default);
    }

    Ray ray = new(transform.position, transform.forward);
    var (executed, result) = _cameraScanner.Scan(ray);

    if (!executed)
      return _lastValidResult;

    if (!result.Item1)
    {
      ClearInteractable();
      DisableLockIn();
      return _lastValidResult = (false, default);
    }

    RaycastHit hit = result.Item2;
    if (hit.collider == null)
    {
      DisableLockIn();
      return (false, default);
    }

    bool foundSomething = false;

    // ── Lock-on ──────────────────────────────────────────────────────
    if (hit.collider.TryGetComponent(out ILockable lockable))
    {
      if (lockable.IsActive && hit.distance <= lockable.LockRange)
      {
        SetLockOn(lockable);
        _lockCandidate = lockable;
        _lastLockHit = hit;
        foundSomething = true;
      }
      else
        DisableLockIn();
    }
    else
      DisableLockIn();

    // ── Interagível ───────────────────────────────────────────────────
    if (hit.collider.TryGetComponent(out InteractableObject interactable))
    {
      if (interactable is not LockableInteractableObject && interactable.IsActive)
      {
        InteractionObject = interactable;
        foundSomething = true;
      }
      else
        ClearInteractable();
    }
    else
      ClearInteractable();

    // ── Mantém lock-on válido ─────────────────────────────────────────
    if (_isLockOnActive && LockedTarget != null)
    {
      float dist = Vector3.Distance(transform.position, LockedTarget.transform.position);
      if (!LockedTarget.IsActive || dist > LockedTarget.LockRange)
        DisableLockIn();
    }

    return foundSomething ? _lastValidResult = (true, hit) : (false, default);
  }

  protected void ClearInteractable()
  {
    InteractionObject = null;
    GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(false, null, ID);
  }
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  HUD & FEEDBACK
  // ─────────────────────────────────────────────────────────────
  #region HUD & Feedback
  private IEnumerator DelayedSetupHUD(float duration)
  {
    yield return new WaitForSeconds(duration);
    SetupHUD();
  }

  private void SetupHUD()
  {
    if (!GameObject.FindWithTag("GameController").TryGetComponent(out HudDirector hudDir))
      return;

    SpeedLines.AddListener(hudDir.GetCameraScript(ID).SpeedlinesFX);

    // NOTE: Shake
    _OnDamage.AddListener(() =>
      hudDir.CameraShake(ID, Damage.Amplitude, Damage.Frequency, Damage.Duration)
    );
    CustomShake.AddListener(
      (id, amplitude, frequency, duration) => hudDir.CameraShake(id, amplitude, frequency, duration)
    );
    RunningShake.AddListener(active => hudDir.RunningShake(ID, active));
  }
  #endregion

  // ─────────────────────────────────────────────────────────────
  //  MORTE
  // ─────────────────────────────────────────────────────────────
  #region Morte
  public override void DeathHandler()
  {
    base.DeathHandler();
    GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.Invoke();
  }
  #endregion
}
