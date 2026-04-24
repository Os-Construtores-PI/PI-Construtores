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
  #region === Configurações de Movimento ===
  private float _speed = 10f;
  private float _runningSpeed = 20;
  internal QualityTier WallSpeedMultiplier = QualityTier.RARE;

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

  internal float Acceleration = 5f;
  internal float AccelerationRunning = 10;
  internal float Friction = 2f;
  internal float AirFriction = 2f;

  [Header("Pulo")]
  private float _jumpForce = 10f;
  internal float WallJumpMultiplier = 5;

  [HideInInspector]
  [Stat(nameof(JumpForce))]
  public float JumpForce
  {
    get => _jumpForce;
    set => _jumpForce = value;
  }

  internal int MaxJumpCount = 2;
  internal float GravityValue = -16.62f;
  internal float GravityUpMultiplier = 2.2f;
  internal float GravityDownMultiplier = 0.6f;
  internal float MaxFallSpeed = -26f;
  internal float InitialGravityValue;

  [Header("Dash")]
  internal float DashSpeed = 30f;
  internal float DashDistance = 5f;
  internal float DashCooldown = 1f;
  internal ShiftDashScript DashHudScript;

  [Header("Componentes")]
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
  protected Camera _myCamera;
  public Collider HitboxCollider;

  public void SetCamera(CinemachineCamera cincam, Camera camera)
  {
    CinemachineCamera = cincam;
    _myCamera = camera;
  }

  public Animator AnimatorComponent;
  public PlayerInput PlayerInput;
  public InputType _ultimoDispositivo = InputType.Keyboard;
  #endregion

  #region === Estados Internos ===
  public StateMachine<Player> LocomotionLayer;
  public StackStateMachine<Player> ActionLayer;

  //!: Action
  private PlayerActionStateIdle _idleActionS = new();
  private PlayerActionStateDash _dashActionS = new();
  private PlayerActionStateInteraction _interactionActionS = new();
  private PlayerActionStateWallSliding _wallSlidingActionS = new();

  // !: Locomotion
  private PlayerLocomotionStateGrounded _groundedState = new();
  private PlayerLocomotionStateAirborne _airborneState = new();
  private PlayerLocomotionStateLocked _lockedState = new();

  internal Vector3 MovementVector;
  internal Vector3 Direction;
  internal Vector3 DashDirection;
  internal Vector2 MoveInput;
  internal bool IsRunning;
  internal bool IsImpulsioned;
  internal Vector3 LastWallNormal;
  public Transform _modelTransform;

  internal int CurrentJumpCount;

  [SerializeField]
  internal bool IsGrounded;
  internal bool WallSpeedApplied;
  internal bool TouchingWall;

  internal bool _canDash = true;
  internal bool _canMove = true;

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
  public bool JumpInputPressed = false;
  private float _dashCount = 1;
  public float CurrentDashCount = 0;
  public float DashDuration;
  #endregion

  #region === Flags de Contexto ===
  // Flags que antes viviam só no PlayerContext
  public bool CameraLocked { get; set; } = false;
  public bool IsHardLocked { get; set; } = false;
  public bool IgnoreGameplayInputThisFrame { get; set; } = false;
  public bool WaitForJumpRelease { get; set; } = false;
  public bool BlockJumpByDialogue { get; set; } = false;
  #endregion

  #region === EnemyScan ===
  [Header("SCANNER DE SPAWN DE INIMIGOS PARÂMETROS")]
  [SerializeField, Min(10)]
  private float enemyScanRadius = 10;
  #endregion

  #region === Interação ===
  [Header("SCANNER DE OBJETOS INTERAGÍVEIS PARÂMETROS")]
  private Camera selectedcamera = null;
  private readonly RaycastHit[] _sphereCastResults = new RaycastHit[20];
  #endregion

  [Header("Inverter Y Camera")]
  [SerializeField]
  private bool _willInvertYAxis = false;

  #region === Inventário ===
  private readonly Inventory _inventory = new();
  public Inventory Inventory => _inventory;
  #endregion

  #region  === Knockback ===
  public HurtboxComponent HurtboxCollider;
  #endregion

  #region === Scanner ===
  private Scanner<Ray, (bool, RaycastHit)> _cameraScanner;
  private Scanner<Vector3, bool> _enemyScanner;
  private Scanner<(Ray, Ray), RaycastHit?> _wallScanner;
  #endregion

  #region === WallSlide ===
  private readonly float wallScanInterval = .05f;
  #endregion

  #region === Events ===
  public readonly UnityEvent IsRunningEv = new();
  public readonly UnityEvent StoppedRunningEv = new();
  #endregion

  #region Coletáveis
  private int amethysts = 0;
  public int Amethysts => amethysts;

  public void SetAmethysts(int value, Vector3? amethystPos)
  {
    if (amethysts == value)
      return;

    Vector3? positionInCamera = null;
    if (amethystPos != null)
      positionInCamera = _myCamera.WorldToScreenPoint((Vector3)amethystPos);

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

  #region === Inicialização Unity ===
  public override void Awake()
  {
    base.Awake();
    canPulse = false;
    InitialGravityValue = GravityValue;

    CharacterController = GetComponent<CharacterController>();
    AnimatorComponent = GetComponent<Animator>();
    PlayerInput = GetComponent<PlayerInput>();
    DetectarDispositivo(PlayerInput);

    LocomotionLayer = new(_groundedState, this);
    ActionLayer = new(_idleActionS, this);
  }

  public override void Start()
  {
    base.Start();
    DOTween.Init();
    SetVisibilityLockOnOverlay(false);
    StartCoroutine(DelayedSetupHUD(.1f));

    if (DashHudScript == null)
    {
      GameObject go = GameObject.FindWithTag("DashHUDIcon");
      if (go)
        DashHudScript = go.GetComponent<ShiftDashScript>();
      else
        Debug.LogWarning(
          "[Player] DashHUDIcon não encontrado em cena. Arraste a instância ou coloque tag."
        );
    }

    InputAction lookAction = InputSystem.actions.FindAction("Look");
    lookAction.ApplyParameterOverride((InvertVector2Processor p) => p.invertY, _willInvertYAxis);

    _cinemachineInput = CinemachineCamera.GetComponent<CinemachineInputAxisController>();
    _cinemachineOrbital = CinemachineCamera.GetComponent<CinemachineOrbitalFollow>();

    TickDirector.Instance.OnFiveTick.AddListener(_ => _enemyScanner.Scan(transform.position));
    TickDirector.Instance.OnFiveTick.AddListener(_ => ScanWalls());
    TickDirector.Instance.OnTick.AddListener(_ => ScanWithCamera());

    _cameraScanner = new Scanner<Ray, (bool, RaycastHit)>(r =>
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
        var col = _sphereCastResults[i].collider;
        if (col.CompareTag("Player"))
          continue;

        Vector3 targetCenter = col.bounds.center;

        Vector3 direcaoParaAlvo = (targetCenter - r.origin).normalized;
        float dot = Vector3.Dot(r.direction.normalized, direcaoParaAlvo);
        if (dot < 0.5f)
          continue;

        float distance = Vector3.Distance(r.origin, targetCenter);
        if (distance > maxDistance)
          continue;

        if (!Physics.Linecast(r.origin, targetCenter, obstacleMask))
        {
          if (distance < closestDistance)
          {
            closestDistance = distance;
            bestTarget = col;
          }
        }
      }

      if (bestTarget != null)
      {
        Vector3 direcaoFinal = (bestTarget.bounds.center - r.origin).normalized;
        if (
          Physics.Raycast(
            r.origin,
            direcaoFinal,
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
    });

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

    _modelTransform = transform.Find("Model");
  }

  public override void Update()
  {
    base.Update();
    KnockbackTimer();

    if (Input.GetKeyDown(KeyCode.F1))
      SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    if (_willUpdateLockOverlay)
      UpdateLockOnOverlayTarget();

    LocomotionLayer.Update(this);
    ActionLayer.Update(this);
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
      Vector2.SqrMagnitude(new(CharacterController.velocity.x, CharacterController.velocity.z))
    );
    AnimatorComponent.SetBool(Constants.AnimatorBoolNames.IsGrounded, IsGrounded);

    LocomotionLayer.FixedUpdate(this);
    ActionLayer.FixedUpdate(this);

    CharacterController.Move(MovementVector * Time.deltaTime);
  }

  public void OnDestroy() => DOTween.Kill(this);
  #endregion

  #region === Input Callbacks ===
  public void OnMove(InputAction.CallbackContext context)
  {
    if (IgnoreGameplayInputThisFrame)
      return;
    MoveInput = context.ReadValue<Vector2>();
  }

  public void LockCamera(bool state) => CameraLocked = state;

  public void OnDash(InputAction.CallbackContext context)
  {
    if (context.started && _canDash && CurrentDashCount < _dashCount)
      StartDash();
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
    string last = input.currentControlScheme;
    switch (last)
    {
      case "Keyboard&Mouse":
        _ultimoDispositivo = InputType.Keyboard;
        break;
      case "Gamepad":
        var gp = Gamepad.current;
        if (gp == null)
          break;
        if (gp.displayName.Contains("DualSense") || gp.displayName.Contains("DualShock"))
          _ultimoDispositivo = InputType.JoystickPlaystation;
        else
          _ultimoDispositivo = InputType.JoystickXbox;
        break;
      default:
        _ultimoDispositivo = InputType.Keyboard;
        break;
    }
    GlobalEventBus.Instance.PLAYERINPUTCHANGED.Invoke(_ultimoDispositivo.ToString());
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
    if (context.started)
      Jump();
  }

  public void OnInteract(InputAction.CallbackContext context)
  {
    if (InteractionObject && context.started)
      ActionLayer.PushState(_interactionActionS, this);
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
  #endregion

  #region === Lock in ===
  private void ToggleLockIn()
  {
    if (_isLockOnActive)
      DisableLockIn();
    else if (_lastValidResult.success && _lockCandidate != null)
    {
      _isLockOnActive = true;
      SetLockOn(_lockCandidate);
    }
  }

  private void SetLockOn(ILockable target)
  {
    LockedTarget = target;

    if (LockedTarget != null)
    {
      _willUpdateLockOverlay = true;
      SetVisibilityLockOnOverlay(true);
      _isLockOnActive = true;
    }
    else
    {
      SetVisibilityLockOnOverlay(false);
      _willUpdateLockOverlay = false;
      _isLockOnActive = false;
    }
  }

  private void SetVisibilityLockOnOverlay(bool set)
  {
    Vector3 scaleTarget = set ? Vector3.one : Vector3.zero;
    if (_lockOnOverlay.TryGetComponent(out RectTransform rect))
      rect.DOScale(scaleTarget, .5f).SetEase(Ease.OutExpo);
  }

  private void UpdateLockOnOverlayTarget()
  {
    if (LockedTarget != null)
      _lockOnOverlay.transform.position = LockedTarget.transform.position;
  }

  public void DisableLockIn()
  {
    if (_isLockOnActive)
    {
      _isLockOnActive = false;
      SetLockOn(null);
    }
  }
  #endregion

  #region === Movimento & Pulo ===

  private void Jump()
  {
    if (DialogueGlobal.Instance != null)
    {
      if (DialogueGlobal.Instance.IsDialogueActive)
        return;
      if (DialogueGlobal.Instance._bloquearJumpTemporariamente)
        return;
    }
    if (CurrentJumpCount < MaxJumpCount)
    {
      JumpInputPressed = true;
    }
  }
  #endregion

  #region === PAUSE ===
  private void Pause()
  {
    if (TutorialGlobal.Instance != null && TutorialGlobal.Instance.IsTutorialActive)
      return;
    if (DialogueGlobal.Instance != null && DialogueGlobal.Instance.IsDialogueActive)
      return;
    GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.Invoke(!GameState.IsPaused);
  }
  #endregion

  #region === Dash ===
  private void StartDash()
  {
    if (IsDashBlocked)
      return;
    ActionLayer.PushState(_dashActionS, this);
  }
  #endregion

  #region === KNOCKBACK ===
  private Vector3 _knockbackVelocity;
  private readonly float _knockbackDuration = 0.2f;
  private readonly Timer _knockbackTimer = new();
  private bool isKnockbackActive;
  internal bool IsDashBlocked;

  public void ApplyKnockback(Vector3 direction, float force)
  {
    if (isKnockbackActive)
      return;
    _knockbackVelocity = direction * force;
    _knockbackTimer.Start(_knockbackDuration);
    isKnockbackActive = true;
  }

  private void KnockbackTimer()
  {
    if (!isKnockbackActive)
      return;
    transform.position += _knockbackVelocity * Time.deltaTime;
    if (_knockbackTimer.Tick(Time.deltaTime))
      isKnockbackActive = false;
  }

  #endregion

  [Header("WALL EXIT")]
  #region === WALLRUNNING ===
  internal float WallExitDuration =  .2f;
  #endregion

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

    if (GameObject.FindWithTag("GameController").TryGetComponent(out HudDirector hudDir))
    {
      _OnDamage.AddListener(hudDir.DamageShake);
      IsRunningEv.AddListener(hudDir.RunningShake);
      IsRunningEv.AddListener(hudDir.GetCameraScript(ID).SpeedFX);
      StoppedRunningEv.AddListener(hudDir.StopRunningShake);
      StoppedRunningEv.AddListener(hudDir.GetCameraScript(ID).StopSpeedFX);
    }
  }
  #endregion

  #region Scan
  private void ScanWalls()
  {
    (bool executed, RaycastHit? hit) = _wallScanner.Scan(
      (new Ray(transform.position, transform.right), new(transform.position, -transform.right))
    );

    if (executed)
    {
      if (hit.HasValue)
      {
        ActionLayer.PushState(_wallSlidingActionS, this);
        LastWallNormal = hit.Value.normal;
      }
      else
      {
        TouchingWall = false;
      }
    }
  }

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
          enemytmp.SetActive(true);
      }
    }
    return true;
  }

  [SerializeField]
  private GameObject _lockOnOverlay;
  private bool _willUpdateLockOverlay = false;
  public ILockable LockedTarget;
  private ILockable _lockCandidate;
  private RaycastHit _lastLockHit;
  private bool _isLockOnActive = false;
  protected RaycastHit _playerRayHit;
  public InteractableObject InteractionObject;
  protected InteractableObject _lastInteractionObject = null;
  protected Type _interactionObjectType;
  protected (bool success, RaycastHit hit) _lastValidResult;

  protected virtual (bool success, RaycastHit hit) ScanWithCamera()
  {
    if (!selectedcamera)
    {
      SetupCamera();
      return (false, default);
    }

    Ray ray = new(transform.position, transform.forward);
    var (executed, scanResult) = _cameraScanner.Scan(ray);

    if (!executed)
      return _lastValidResult;

    if (!scanResult.Item1)
    {
      ClearInteractable();
      DisableLockIn(); // ← estava faltando aqui
      _lastValidResult = (false, default);
      return _lastValidResult;
    }

    RaycastHit hit = scanResult.Item2;
    if (hit.collider == null)
    {
      DisableLockIn(); // ← e aqui
      return (false, default);
    }

    bool foundSomething = false;

    if (hit.collider.TryGetComponent(out ILockable lockable))
    {
      if (lockable.IsActive && hit.distance <= lockable.LockRange)
      {
        if (LockedTarget != lockable)
          SetLockOn(lockable);

        _lockCandidate = lockable;
        _lastLockHit = hit;
        foundSomething = true;
      }
      else
      {
        DisableLockIn();
      }
    }
    else
    {
      DisableLockIn();
    }

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
      float distToLocked = Vector3.Distance(transform.position, LockedTarget.transform.position);
      if (!LockedTarget.IsActive || distToLocked > LockedTarget.LockRange)
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

  #region === Ataque ===
  [Header("ATAQUE PARÂMETROS")]
  internal float AttackCooldown;
  public bool CanAttack = true;
  public bool WillAttack = true;

  protected virtual void Attack() { }
  #endregion

  #region === Camera ===
  private void SetupCamera()
  {
    foreach (Camera camera in Camera.allCameras)
    {
      camera.TryGetComponent(out CameraLogic cameraLogic);
      if (cameraLogic && cameraLogic.ID == ID)
      {
        selectedcamera = camera;
        return;
      }
    }
    Debug.LogError("[Player] Câmera com ID correspondente não encontrada.");
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
