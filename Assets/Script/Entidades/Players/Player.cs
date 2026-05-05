using System;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.SceneManagement;
using static TutorialGlobal;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput), typeof(Collider))]
[RequireComponent(typeof(Animator))]
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

  internal QualityTier WallSpeedMultiplier = QualityTier.RARE;
  internal float Acceleration = 5f;
  internal float AccelerationRunning = 10f;
  internal float Friction = 2f;
  internal float AirFriction = 2f;

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

  [SerializeField]
  private Transform _cameraTarget;

  [HideInInspector]
  public CharacterController CharacterController;

  [HideInInspector]
  public CinemachineCamera CinemachineCamera;

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

  public void SetCamera(CinemachineCamera cincam, Camera camera)
  {
    CinemachineCamera = cincam;
    _myCamera = camera;
  }

  #endregion

  // ─────────────────────────────────────────────────────────────
  //  STATE MACHINES
  // ─────────────────────────────────────────────────────────────
  #region State Machines & Estados

  public StateMachine<Player> LocomotionLayer;
  public StackStateMachine<Player> ActionLayer;

  // Action states
  public PlayerActionStateIdle IdleAS = new();
  public PlayerActionStateDash DashAS = new();
  public PlayerActionStateInteraction InteractionAS = new();
  public PlayerActionStateWallSliding WallSlidingAS = new();
  public PlayerActionStateGroundSlam GroundSlamAS = new();
  public BoostSlashDashButton DashSlashBoostButton;

  // Locomotion states
  public PlayerLocomotionStateGrounded GroundedS = new();
  public PlayerLocomotionStateAirborne AirborneS = new();
  public PlayerLocomotionStateLocked LockedS = new();

  #endregion

  // ─────────────────────────────────────────────────────────────
  //  ESTADO INTERNO DO PLAYER
  // ─────────────────────────────────────────────────────────────
  #region Estado Interno

  internal Vector3 MovementVector;
  internal Vector3 Direction;
  internal Vector3 DashDirection;
  internal Vector2 MoveInput;
  internal Vector3 LastWallNormal;

  internal bool IsRunning;
  internal bool IsImpulsioned;
  internal bool WallSpeedApplied;
  internal bool TouchingWall;
  internal bool IsDashBlocked;

  internal int CurrentJumpCount = 0;

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

  public bool IsDashing = false;
  public float MaxDashCount = 1f;
  public float CurrentDashCount = 0f;
  public float DashDuration;
  public float GroundSlamImpactSpeed { get; set; } = 0f;

  public Transform _modelTransform;

  // Ultimo dispositivo de input detectado
  public InputType _ultimoDispositivo = InputType.Keyboard;

  #endregion

  // ─────────────────────────────────────────────────────────────
  //  FLAGS DE INPUT DE PULO
  // ─────────────────────────────────────────────────────────────
  #region Flags de Input – Pulo
  public bool JumpInputPressed = false;
  public bool JumpInteractionPressed = false;

  public void ConsumeJumpInteraction() => JumpInteractionPressed = false;

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

  public readonly UnityEvent IsRunningEv = new();
  public readonly UnityEvent StoppedRunningEv = new();

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
      ActionLayer.PushState(DashAS, this);
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
    GravityValue = -16.62f; // valor padrão antes de ser sobrescrito
    InitialGravityValue = GravityValue;

    CharacterController = GetComponent<CharacterController>();
    AnimatorComponent = GetComponent<Animator>();
    PlayerInput = GetComponent<PlayerInput>();

    DetectarDispositivo(PlayerInput);
    DashSlashBoostButton = new(this, 100, 20, .5f);
    LocomotionLayer = new(GroundedS, this);
    ActionLayer = new(IdleAS, this);
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

    AnimatorComponent.SetFloat(
      Constants.AnimatorFloatNames.VelocityY,
      CharacterController.velocity.y
    );
    AnimatorComponent.SetFloat(
      Constants.AnimatorFloatNames.VelocityX,
      Vector2.SqrMagnitude(
        new Vector2(CharacterController.velocity.x, CharacterController.velocity.z)
      )
    );
    AnimatorComponent.SetBool(Constants.AnimatorBoolNames.IsGrounded, IsGrounded);

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

    _cinemachineInput = CinemachineCamera.GetComponent<CinemachineInputAxisController>();
    _cinemachineOrbital = CinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
  }

  private void SetupScanners()
  {
    TickDirector.Instance.OnFiveTick.AddListener(_ => _enemyScanner.Scan(transform.position));
    TickDirector.Instance.OnFiveTick.AddListener(_ => ScanWalls());

    DashSlashBoostButton.StartedChargingEv.AddListener(() =>
      EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Charging, 1)
    );
    DashSlashBoostButton.StoppedChargingEv.AddListener(() =>
      EffectsWorker.StopEffect(Constants.EffectsNames.Player.Charging)
    );

    _cameraScanner = new Scanner<Ray, (bool, RaycastHit)>(BuildCameraScanner());
    _enemyScanner = new Scanner<Vector3, bool>(ScanEnemies);
    _wallScanner = new Scanner<(Ray, Ray), RaycastHit?>(rays =>
    {
      float distance = 5f;
      int mask = LayerMask.GetMask("RunningWall");
      var interaction = QueryTriggerInteraction.Ignore;

      if (Physics.Raycast(rays.Item1, out RaycastHit hit, distance, mask, interaction))
        return hit;
      if (Physics.Raycast(rays.Item2, out hit, distance, mask, interaction))
        return hit;
      return null;
    });
  }

  /// <summary>
  /// Fábrica do delegate do camera scanner, separado para manter o Start legível.
  /// </summary>
  private Func<Ray, (bool, RaycastHit)> BuildCameraScanner() =>
    r =>
    {
      float radius = 6f;
      float maxDistance = 20f;
      LayerMask targetsMask = LayerMask.GetMask("Object", "Entity");
      LayerMask obstacleMask = LayerMask.GetMask("Default");

      int hitCount = Physics.SphereCastNonAlloc(
        r.origin,
        radius,
        r.direction,
        _sphereCastResults,
        maxDistance,
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
        Vector3 dirToTarget = (targetCenter - r.origin).normalized;
        float dot = Vector3.Dot(r.direction.normalized, dirToTarget);
        if (dot < 0.5f)
          continue;

        float distance = Vector3.Distance(r.origin, targetCenter);
        if (distance > maxDistance)
          continue;

        if (!Physics.Linecast(r.origin, targetCenter, obstacleMask) && distance < closestDistance)
        {
          closestDistance = distance;
          bestTarget = col;
        }
      }

      if (bestTarget != null)
      {
        Vector3 finalDir = (bestTarget.bounds.center - r.origin).normalized;
        if (
          Physics.Raycast(
            r.origin,
            finalDir,
            out RaycastHit finalHit,
            maxDistance + 2f,
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
      ActionLayer.PushState(GroundSlamAS, this);
  }

  public void OnRunning(InputAction.CallbackContext context)
  {
    if (context.performed)
    {
      IsRunning = true;
      IsRunningEv.Invoke();
    }
    else if (context.canceled)
    {
      IsRunning = false;
      StoppedRunningEv.Invoke();
    }
  }

  public void OnJump(InputAction.CallbackContext context)
  {
    if (IsHardLocked)
      return;
    if (IgnoreGameplayInputThisFrame)
      return;
    if (BlockJumpByDialogue)
      return;

    if (WaitForJumpRelease)
    {
      if (context.canceled)
        WaitForJumpRelease = false;
      return;
    }

    if (!context.started)
    {
      return;
    }

    if (!IsGrounded)
    {
      JumpInteractionPressed = true;
    }

    // ── Pulo normal ──────────────────────────────────────────────────
    TryJump();
  }

  public void OnInteract(InputAction.CallbackContext context)
  {
    if (InteractionObject && context.started)
      ActionLayer.PushState(InteractionAS, this);

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

    bool didHit = Physics.Raycast(
      new Ray(transform.position, Vector3.down),
      out RaycastHit hit,
      Mathf.Infinity,
      LayerMask.GetMask("Default", "Ground"),
      QueryTriggerInteraction.Ignore
    );

    if (!didHit)
      return;

    float distanceToGround = hit.distance;
    float currentVelocityY = CharacterController.velocity.y;

    // Próximo do chão ou subindo → pulo imediato
    if (distanceToGround <= 1.1f || currentVelocityY > 0.01f)
    {
      JumpInputPressed = true;
      return;
    }

    // Caindo e perto de aterrissar → registra pulo antecipado (coyote-like)
    if (currentVelocityY < -0.01f)
    {
      float timeToReach = distanceToGround / Mathf.Abs(currentVelocityY);
      if (timeToReach <= 0.2f)
      {
        CurrentJumpCount = 3;
        JumpInputPressed = true;
      }
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
      ActionLayer.PushState(WallSlidingAS, this);
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
      _lastValidResult = (false, default);
      return _lastValidResult;
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

    if (foundSomething)
    {
      _lastValidResult = (true, hit);
      return _lastValidResult;
    }

    return (false, default);
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

    _OnDamage.AddListener(hudDir.DamageShake);
    IsRunningEv.AddListener(hudDir.RunningShake);
    IsRunningEv.AddListener(hudDir.GetCameraScript(ID).SpeedFX);
    StoppedRunningEv.AddListener(hudDir.StopRunningShake);
    StoppedRunningEv.AddListener(hudDir.GetCameraScript(ID).StopSpeedFX);
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
