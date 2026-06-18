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
using UnityEngine.Splines;
using static Constants.PlayerShakes;
using static TutorialGlobal;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput), typeof(Collider))]
[RequireComponent(typeof(Animator), typeof(AudioSource))]
[DefaultExecutionOrder(-100)]
public class Player : CombatEntities
{
  #region Constantes (Substituição de Magic Values)

  private const float HUD_INIT_DELAY = 0.1f;
  private const float CAMERA_SCAN_BUFFER = 2f;
  private const float RAIL_SCORE_WEIGHT = 0.2f;
  private const float SQR_EPSILON = 0.01f;

  // Layers
  private const string LAYER_OBJECT = "Object";
  private const string LAYER_ENTITY = "Entity";
  private const string LAYER_DEFAULT = "Default";
  private const string LAYER_RUNNING_WALL = "RunningWall";

  // Tags
  private const string TAG_PLAYER = "Player";
  private const string TAG_DASH_HUD = "DashHUDIcon";
  private const string TAG_GAME_CONTROLLER = "GameController";
  #endregion

  #region Movimento - Stats
  private float _speed;

  [HideInInspector, Stat(StatType.Speed)]
  public float Speed
  {
    get => _speed;
    set => _speed = value;
  }

  [HideInInspector]
  public float RunSpeedMultiplier;

  [HideInInspector]
  public float RunAccelMultiplier;

  [HideInInspector]
  public QualityTier WallSpeedMultiplier;

  [HideInInspector]
  public float Acceleration;

  [HideInInspector]
  public float AccelerationRunning;

  [HideInInspector]
  public float Friction;

  [HideInInspector]
  public float AirFriction;
  #endregion

  #region Movimento - Pulo
  private float _jumpForce;

  [HideInInspector, Stat(StatType.JumpForce)]
  public float JumpForce
  {
    get => _jumpForce;
    set => _jumpForce = value;
  }

  [HideInInspector]
  public int MaxJumpCount;

  [HideInInspector]
  public float WallJumpMultiplier;

  [HideInInspector, Stat(StatType.Gravity)]
  public float GravityValue { get; set; }

  [HideInInspector]
  public float GravityUpMultiplier;

  [HideInInspector]
  public float GravityDownMultiplier;

  [HideInInspector]
  public float MaxFallSpeed;

  [HideInInspector]
  public float InitialGravityValue;
  #endregion

  #region Movimento - Dash
  [HideInInspector]
  public float DashSpeed;

  [HideInInspector]
  public float DashDistance;

  [HideInInspector]
  public float DashCooldown;

  [HideInInspector]
  public ShiftDashScript DashHudScript;
  #endregion

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

  #region State Machines & Estados
  public PlayerStateMachine<Player> LocomotionLayer = new();
  public StackStateMachine<Player> ActionLayer = new();

  [Header("Estados")]
  public PlayerActionStateDash Dash = new();
  public PlayerActionStateInteraction Interaction = new();
  public PlayerActionStateWallSliding WallSliding = new();
  public PlayerActionStateGroundSlam GroundSlam = new();
  public PlayerActionStateBoost Boost = new();
  public PlayerActionStateBounce Bounce = new();
  public PlayerActionStateJump Jump = new();
  public PlayerActionStateRailSlide RailSlide = new();

  public readonly PlayerLocomotionStateMoving Moving = new();
  public readonly PlayerLocomotionStateLocked Locked = new();
  public readonly PlayerLocomotionStateHLocked LockedInHorizontal = new();
  #endregion

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

  [HideInInspector]
  public bool IsGrounded;

  [HideInInspector]
  public bool WantsToCancelRailSlide;

  private bool _canMove = true;
  private bool _canDash = true;

  [Stat(StatType.CanDash)]
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

  #region Flags de Input - Pulo
  public bool JumpInteractionPressed = false;

  public void ConsumeJumpInteraction() => JumpInteractionPressed = false;
  #endregion

  #region Trails
  public TrailsWorker TrailsSystem = new();
  #endregion

  #region Flags de Contexto
  public bool CameraLocked { get; set; } = false;
  public bool IsHardLocked { get; set; } = false;
  public bool IgnoreGameplayInputThisFrame { get; set; } = false;
  public bool WaitForJumpRelease { get; set; } = false;
  public bool BlockJumpByDialogue { get; set; } = false;
  #endregion

  #region Boost

  [HideInInspector]
  public UnityEvent<float> BoostChanged;

  [Header("Boost Value Options")]
  [SerializeField]
  private float _maxBoostValue = 100f;

  public float MaxBoostValue
  {
    get => _maxBoostValue;
  }

  [SerializeField]
  private float _initialBoostValue = 100f;

  [HideInInspector]
  public float BoostValue
  {
    get { return _boostValue; }
    set
    {
      _boostValue = Mathf.Clamp(value, 0f, _maxBoostValue);
      BoostChanged.Invoke(_boostValue / _maxBoostValue);
    }
  }

  private float _boostValue = 0f;

  #endregion Boost

  #region Scanners
  [Header("Scanner - Inimigos")]
  [SerializeField, Min(10)]
  private float enemyScanRadius = 10f;

  private const float CameraScanSphereRadius = 6f;
  private const float CameraScanMaxDistance = 100f;
  private const float CameraScanDotThreshold = 0.5f;
  private const float WallScanDistance = 5f;

  private Camera _selectedCamera = null;
  private readonly RaycastHit[] _sphereCastResults = new RaycastHit[20];

  private Scanner<Ray, (bool, RaycastHit)> _cameraScanner;
  private Scanner<Vector3, bool> _enemyScanner;
  private Scanner<(Ray, Ray), RaycastHit?> _wallScanner;

  [Header("Scanner - Trilho")]
  [SerializeField]
  private float _railEntryRadius = 1.2f;

  [SerializeField]
  private float _railEntryForwardOffset = 0.8f;

  [SerializeField]
  private float _railEntryMinDot = 0.3f;

  [SerializeField]
  private LayerMask _railLayerMask;

  private Scanner<Vector3, RailObject> _railEntryScanner;
  #endregion

  #region Lock-On
  public ILockable LockedTarget;
  private ILockable _lockCandidate;
  private RaycastHit _lastLockHit;
  private bool _isLockOnActive = false;
  protected RaycastHit _playerRayHit;
  #endregion

  #region Interação
  [HideInInspector]
  public InteractableObject InteractionObject;
  protected InteractableObject _lastInteractionObject;
  protected Type _interactionObjectType;
  protected (bool success, RaycastHit hit) _lastValidResult;
  #endregion

  #region Inventário & Coletáveis
  private readonly Inventory _inventory = new();
  public Inventory Inventory => _inventory;

  private int _amethysts = 0;
  public int Amethysts => _amethysts;

  public void SetAmethysts(int value)
  {
    int clamped = Mathf.Max(0, value);
    if (_amethysts == clamped)
      return;

    _amethysts = clamped;
    GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.Invoke(_amethysts);
  }

  public void AddAmethysts(int amount) => SetAmethysts(_amethysts + amount);

  public bool SpendAmethysts(int amount)
  {
    if (amount <= 0 || _amethysts < amount)
      return false;
    SetAmethysts(_amethysts - amount);
    return true;
  }
  #endregion

  #region Eventos
  public readonly UnityEvent<bool> SpeedLines = new();
  public readonly UnityEvent<bool> RunningShake = new();
  public readonly UnityEvent<int, float, float, float> CustomShake = new();
  #endregion

  #region Sons
  [Header("Sons do Jogador")]
  [SerializeField]
  private List<PlayerSFX> _playerSFX = new();
  public readonly SoundsWorker<PlayerAudioType> PlayerSoundSystem = new();
  #endregion

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

  #region Wall Running
  [Header("Wall Exit")]
  internal float WallExitDuration;
  #endregion

  #region Camera
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

  #region Unity Lifecycle
  public override void Awake()
  {
    base.Awake();
    canPulse = false;

    CharacterController = GetComponent<CharacterController>();
    AnimatorComponent = GetComponent<Animator>();
    PlayerInput = GetComponent<PlayerInput>();

    _railLayerMask = LayerMask.GetMask(LAYER_DEFAULT);
    DetectarDispositivo(PlayerInput);
  }

  public override void Start()
  {
    InitialGravityValue = GravityValue;
    base.Start();

    DOTween.Init();
    SetVisibilityLockOnOverlay(false);
    StartCoroutine(DelayedSetupHUD(HUD_INIT_DELAY));

    SetupDashHUD();
    SetupCinemachine();
    SetupScanners();

    TrailsSystem.InitTrails(transform.Find("Trails"));
    PlayerSoundSystem.Init(_playerSFX, playersfx => playersfx.Type, _playerAudioSource);
    _modelTransform = transform.Find("Model");
    BoostValue = _initialBoostValue;

    LocomotionLayer.ChangeState(Moving, this);
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

  #region Helpers de Inicialização
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

    GameObject go = GameObject.FindWithTag(TAG_DASH_HUD);
    if (go)
      DashHudScript = go.GetComponent<ShiftDashScript>();
    else
      Debug.LogWarning(
        "[Player] DashHUDIcon não encontrado. Arraste a instância ou coloque a tag."
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
    TickDirector.Instance.OnTick.AddListener(_ => ScanRailEntry());
    TickDirector.Instance.OnFiveTick.AddListener(_ => _enemyScanner.Scan(transform.position));
    TickDirector.Instance.OnFiveTick.AddListener(_ => ScanWalls());

    _cameraScanner = new Scanner<Ray, (bool, RaycastHit)>(BuildCameraScanner());
    _enemyScanner = new Scanner<Vector3, bool>(ScanEnemies);
    _wallScanner = new Scanner<(Ray, Ray), RaycastHit?>(ScanWallRays);
    _railEntryScanner = new Scanner<Vector3, RailObject>(BuildRailEntryScanner());
  }

  private RaycastHit? ScanWallRays((Ray left, Ray right) rays)
  {
    int mask = LayerMask.GetMask(LAYER_RUNNING_WALL);
    if (
      Physics.Raycast(
        rays.left,
        out RaycastHit hit,
        WallScanDistance,
        mask,
        QueryTriggerInteraction.Ignore
      )
    )
      return hit;
    if (
      Physics.Raycast(rays.right, out hit, WallScanDistance, mask, QueryTriggerInteraction.Ignore)
    )
      return hit;
    return null;
  }

  private Func<Ray, (bool, RaycastHit)> BuildCameraScanner() =>
    ray =>
    {
      LayerMask targetsMask = LayerMask.GetMask(LAYER_OBJECT, LAYER_ENTITY);
      LayerMask obstacleMask = LayerMask.GetMask(LAYER_DEFAULT);

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
        if (col.CompareTag(TAG_PLAYER))
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
        // Adiciona buffer constante ao distance para evitar falhas de precisão
        if (
          Physics.Raycast(
            ray.origin,
            finalDir,
            out RaycastHit finalHit,
            CameraScanMaxDistance + CAMERA_SCAN_BUFFER,
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

  private Func<Vector3, RailObject> BuildRailEntryScanner() =>
    playerPos =>
    {
      if (
        RailSlide.CurrentRail != null
        || ActionLayer.GetActive<PlayerActionStateRailSlide>() != null
      )
        return null;

      Vector3 moveDir =
        MovementVector.sqrMagnitude > SQR_EPSILON
          ? MovementVector.normalized
          : CharacterController.velocity.normalized;
      if (moveDir.sqrMagnitude < SQR_EPSILON)
        return null;

      Vector3 scanOrigin = playerPos + moveDir * _railEntryForwardOffset;
      var hits = Physics.OverlapSphere(
        scanOrigin,
        _railEntryRadius,
        _railLayerMask,
        QueryTriggerInteraction.Ignore
      );

      RailObject bestRail = null;
      float bestScore = -1f;

      foreach (var hit in hits)
      {
        if (!hit.TryGetComponent(out RailObject rail))
          continue;
        if (!rail.GetNearestPointOnSpline(playerPos, out Vector3 nearestPoint, out float t))
          continue;

        float distance = Vector3.Distance(playerPos, nearestPoint);
        if (distance > _railEntryRadius)
          continue;

        float alignment = Vector3.Dot((nearestPoint - playerPos).normalized, moveDir);
        if (alignment >= _railEntryMinDot)
        {
          // Cálculo de score usando peso constante
          float score = alignment - (distance / _railEntryRadius) * RAIL_SCORE_WEIGHT;
          if (score > bestScore)
          {
            bestScore = score;
            bestRail = rail;
          }
        }
      }
      return bestRail;
    };

  private void ScanRailEntry()
  {
    var (executed, rail) = _railEntryScanner.Scan(transform.position);
    if (executed && rail != null)
    {
      RailSlide.CurrentRail = rail.GetComponent<SplineContainer>();
      ActionLayer.PushState(RailSlide, this);
    }
  }
  #endregion

  #region Input Callbacks
  public void OnMove(InputAction.CallbackContext context)
  {
    if (IgnoreGameplayInputThisFrame)
      return;
    MoveInput = context.ReadValue<Vector2>();
  }

  public void OnDash(InputAction.CallbackContext context)
  {
    if (!context.performed)
      return;

    if (IsGrounded)
    {
      ActionLayer.PushState(Boost, this);
      return;
    }

    if (!CanDash || CurrentDashCount >= MaxDashCount || IsDashBlocked)
    {
      return;
    }
    ActionLayer.PushState(Dash, this);
  }

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
      TrailsSystem.PlayEffect(TrailType.MovementTrail);
      RunningShake.Invoke(true);
    }
    else if (context.canceled)
    {
      IsRunning = false;
      TrailsSystem.StopEffect(TrailType.MovementTrail);
      RunningShake.Invoke(false);
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

  public void OnDisable() => PlayerInput.onControlsChanged -= DetectarDispositivo;

  private void DetectarDispositivo(PlayerInput input)
  {
    switch (input.currentControlScheme)
    {
      case "Keyboard&Mouse":
        _ultimoDispositivo = InputType.Keyboard;
        break;
      case "Gamepad":
        var gp = Gamepad.current;
        if (gp != null)
        {
          _ultimoDispositivo =
            (gp.displayName.Contains("DualSense") || gp.displayName.Contains("DualShock"))
              ? InputType.JoystickPlaystation
              : InputType.JoystickXbox;
        }
        break;
      default:
        _ultimoDispositivo = InputType.Keyboard;
        break;
    }
    GlobalEventBus.Instance.PLAYERINPUTCHANGED.Invoke(_ultimoDispositivo.ToString());
  }
  #endregion

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
    ActionLayer.PushState(Jump, this);
  }
  #endregion

  #region Lock-On
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
    if (EnemySpawner.Instance == null)
      return false;
    int amount = EnemySpawner.Instance.GetAmountPool();

    for (int i = 0; i < amount; i++)
    {
      GameObject enemy = EnemySpawner.Instance.GetDisabledObject();
      if (enemy != null && Vector3.Distance(enemy.transform.position, playerPos) <= enemyScanRadius)
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

  #region HUD & Feedback
  private IEnumerator DelayedSetupHUD(float duration)
  {
    yield return new WaitForSeconds(duration);
    SetupHUD();
  }

  private void SetupHUD()
  {
    if (!GameObject.FindWithTag(TAG_GAME_CONTROLLER).TryGetComponent(out HudDirector hudDir))
      return;

    SpeedLines.AddListener(hudDir.GetCameraScript(ID).SpeedlinesFX);
    _OnDamage.AddListener(() =>
      hudDir.CameraShake(ID, Damage.Amplitude, Damage.Frequency, Damage.Duration)
    );
    CustomShake.AddListener(
      (id, amplitude, frequency, duration) => hudDir.CameraShake(id, amplitude, frequency, duration)
    );
    RunningShake.AddListener(active => hudDir.RunningShake(ID, active));
  }
  #endregion

  #region Morte
  public override void DeathHandler()
  {
    base.DeathHandler();
    GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.Invoke();
  }
  #endregion
}
