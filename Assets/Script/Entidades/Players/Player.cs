using System;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
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
  internal QualityTier wallSpeedMultiplier = QualityTier.RARE;

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
  internal float _acceleration = 5f;
  internal float _accelerationRunning = 10;
  internal float _friction = 2f;
  internal float _airFriction = 2f;

  [Header("Pulo")]
  private float _jumpForce = 10f;
  internal float _wallJumpMultiplier = 5;

  [HideInInspector]
  [Stat(nameof(JumpForce))]
  public float JumpForce
  {
    get => _jumpForce;
    set => _jumpForce = value;
  }

  internal int _maxJumpCount = 2;
  internal float _gravityValue = -16.62f;
  internal float _gravityUpMultiplier = 2.2f; // sobe rápido, perde força cedo
  internal float _gravityDownMultiplier = 0.6f; // cai mais lento
  internal float _maxFallSpeed = -26f; // limite da queda
  internal float _initialGravityValue;

  [Header("Dash")]
  internal float DashSpeed = 30f;
  internal float _dashDistance = 5f;
  internal float dashCooldown = 1f;
  internal ShiftDashScript dashHUDScript; // adicionado para ter uma animação no Shift

  [Header("Componentes")]
  [SerializeField]
  private Transform _cameraTarget;
  protected internal CharacterController _characterController;
  protected internal CinemachineCamera _cinemachineCamera;
  protected internal CinemachineCamera _lockOnCamera;
  protected internal CinemachineTargetGroup _lockOnGroup;
  protected internal CinemachineInputAxisController _cinemachineInput;
  protected internal CinemachineOrbitalFollow _cinemachineOrbital;
  protected Camera _myCamera;

  public void SetCamera(
    CinemachineCamera cincam,
    CinemachineCamera lockOn,
    CinemachineTargetGroup group,
    Camera camera
  )
  {
    _cinemachineCamera = cincam;
    _lockOnCamera = lockOn;
    _lockOnGroup = group;
    _myCamera = camera;
  }

  protected internal Animator animatorComp;

  internal PlayerInput playerInput;
  public InputType _ultimoDispositivo = InputType.Keyboard;

  #endregion

  #region === Overrides ===
  // === GLOBAL ===
  public bool OverrideGlobal { get; set; } = false;
  public float GlobalOverride { get; set; } = 0f;

  // === HORIZONTAL ===
  public bool OverrideHorizontal { get; set; } = false;

  // === VERTICAL ===
  public bool OverrideVertical { get; set; } = false;
  public float VerticalOverride { get; set; } = 0f;
  #endregion

  #region === Estados Internos ===
  internal StateMachine<PlayerContext> HorizontalLayer;
  internal StateMachine<PlayerContext> VerticalLayer;
  internal StackStateMachine<PlayerContext> ActionLayer;

  //!: Action
  internal PlayerActionStateIdle _idleActionS = new();
  internal PlayerActionStateDash _dashActionS = new();
  internal PlayerActionStateInteraction _interactionActionS = new();
  internal PlayerActionStateWallSliding _wallSlidingActionS = new();

  //!:Horizontal
  internal PlayerHorizontalStateIdle _idleHorizontalS = new();
  internal PlayerHorizontalStateMoviment _movementHorizontalS = new();

  //!:Vertical
  internal PlayerGroundedState _groundedVerticalS = new();
  internal PlayerFallingState _fallingVerticalS = new();
  internal PlayerJumpingState _jumpingVerticalS = new();

  public PlayerContext Context { get; internal set; }

  internal Vector3 _movementVector;
  internal Vector3 _direction;
  internal Vector3 _dashDirection;
  internal Vector2 _moveInput;
  internal bool _isRunning;
  internal Vector3 _lastWallNormal;
  internal Transform _modelTransform;

  internal int _currentJumpCount;

  [SerializeField]
  internal bool _isGrounded;
  internal bool _wallSpeedApplied;
  internal bool _touchingWall;

  internal bool _canDash = true;
  internal bool _canMove = true;

  [Stat(nameof(CanMove))]
  public bool CanMove
  {
    get => _canMove;
    set => _canMove = value;
  } // nova flag para controle de movimento

  [Stat(nameof(CanDash))]
  public bool CanDash
  {
    get => _canDash;
    set => _canDash = value;
  }
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
  private readonly float interactionScanCooldown = .1f;
  private Camera selectedcamera = null;
  #endregion

  #region === Inventário ===
  private readonly Inventory _inventory = new();
  public Inventory Inventory => _inventory;

  #endregion

  #region  === Scanner ===
  private Scanner<Ray, (bool, RaycastHit)> _cameraScanner;
  private Scanner<Vector3, bool> _enemyScanner;
  private Scanner<(Ray, Ray), RaycastHit?> _wallScanner;
  #endregion

  #region === WallSlide ===
  private readonly float wallScanInterval = .05f;

  #endregion

  #region === Events ===
  public readonly UnityEvent IsRunning = new();
  public readonly UnityEvent StoppedRunning = new();
  #endregion
  #region Coletáveis


  // === AMETISTAS ===
  private int amethysts = 0;
  public int Amethysts => amethysts;

  public void SetAmethysts(int value, Vector3? amethystPos)
  {
    if (amethysts == value)
    {
      return;
    }
    Vector3? positionInCamera = null;
    if (amethystPos != null)
    {
      positionInCamera = _myCamera.WorldToScreenPoint((Vector3)amethystPos);
    }
    amethysts = Mathf.Max(0, value); // evita negativo
    GlobalEventBus.Instance.AMETHYSTSAMOUNTCHANGED.Invoke(amethysts, positionInCamera);
  }

  public void AddAmethysts(int amount, Vector3? amethystPos)
  {
    SetAmethysts(amethysts + amount, amethystPos);
  }

  public bool SpendAmethysts(int amount)
  {
    if (amount <= 0 || amethysts < amount)
    {
      return false;
    }
    SetAmethysts(amethysts - amount, null);
    return true;
  }

  #endregion
  #region === Inicialização Unity ===


  public override void Awake()
  {
    base.Awake();
    canPulse = false;
    _initialGravityValue = _gravityValue;
    Context = new(this);

    _characterController = GetComponent<CharacterController>();
    animatorComp = GetComponent<Animator>();
    playerInput = GetComponent<PlayerInput>();
    DetectarDispositivo(playerInput);

    VerticalLayer = new(_fallingVerticalS, Context);
    HorizontalLayer = new(_idleHorizontalS, Context);
    ActionLayer = new(_idleActionS, Context);
    SetupCamera();
  }

  public override void Start()
  {
    base.Start();
    DOTween.Init();
    SetVisibilityLockOnOverlay(false);
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

    _cinemachineInput = _cinemachineCamera.GetComponent<CinemachineInputAxisController>();
    _cinemachineOrbital = _cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();

    TickDirector.Instance.OnFiveTick.AddListener(_ =>
      _enemyScanner.Scan(Time.deltaTime, transform.position)
    );
    TickDirector.Instance.OnFiveTick.AddListener(_ => ScanWalls());
    TickDirector.Instance.OnFiveTick.AddListener(_ => ScanWithCamera());

    _cameraScanner = new Scanner<Ray, (bool, RaycastHit)>(
      interactionScanCooldown,
      r =>
      {
        // 1. Usamos um raio um pouco menor para não "atropelar" objetos laterais por erro
        bool hit = Physics.SphereCast(
          r,
          2f,
          out RaycastHit info,
          40f,
          LayerMask.GetMask("Object", "Enemy")
        );

        if (hit)
        {
          Vector3 direcaoParaAlvo = (info.collider.transform.position - r.origin).normalized;
          float dot = Vector3.Dot(r.direction, direcaoParaAlvo);

          if (dot >= 0.8f)
          {
            if (
              !Physics.Linecast(
                r.origin,
                info.collider.transform.position,
                LayerMask.GetMask("Default")
              )
            )
            {
              return (true, info);
            }
          }
        }
        return (false, default);
      }
    );

    _enemyScanner = new Scanner<Vector3, bool>(enemyScanInterval, ScanEnemies);

    _wallScanner = new Scanner<(Ray, Ray), RaycastHit?>(
      wallScanInterval,
      rays =>
      {
        float distance = 5f;
        int mask = LayerMask.GetMask("RunningWall");
        var interaction = QueryTriggerInteraction.Ignore;

        if (Physics.Raycast(rays.Item1, out RaycastHit hit, distance, mask, interaction))
        {
          return hit;
        }

        // Se o primeiro falhar, tenta o segundo (ex: Esquerda)
        if (Physics.Raycast(rays.Item2, out hit, distance, mask, interaction))
        {
          return hit;
        }

        // Se nenhum bater, retorna null
        return null;
      }
    );

    _modelTransform = transform.Find("Model");
  }

  public override void Update()
  {
    base.Update();
    KnockbackTimer();
    //ChangeCharacterTimer();
    if (_willUpdateLockOverlay)
    {
      UpdateLockOnOverlayTarget();
    }

    VerticalLayer.Update(Context);
    HorizontalLayer.Update(Context);
    ActionLayer.Update(Context);
  }

  public void FixedUpdate()
  {
    if (Input.GetKeyDown(KeyCode.F1))
    {
      SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    if (!_characterController.enabled)
      return;

    _isGrounded = _characterController.isGrounded;
    animatorComp.SetFloat(Constants.AnimatorFloatNames.VelocityY, _characterController.velocity.y);
    animatorComp.SetFloat(
      Constants.AnimatorFloatNames.VelocityX,
      Vector2.SqrMagnitude(new(_characterController.velocity.x, _characterController.velocity.z))
    );
    animatorComp.SetBool(Constants.AnimatorBoolNames.IsGrounded, _isGrounded);
    KnockbackTimer();
    HorizontalLayer.FixedUpdate(Context);
    VerticalLayer.FixedUpdate(Context);
    ActionLayer.FixedUpdate(Context);

    // MOVEMENT
    _characterController.Move(_movementVector * Time.deltaTime);
  }

  public void OnDestroy() => DOTween.KillAll();
  #endregion

  #region === Input Callbacks ===

  public void OnMove(InputAction.CallbackContext context)
  {
    // if (Context.IsHardLocked) return;
    if (Context.IgnoreGameplayInputThisFrame)
      return;

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
    {
      StartDash();
    }
  }

  public void OnRunning(InputAction.CallbackContext context)
  {
    if (context.performed)
    {
      _isRunning = true;
      IsRunning.Invoke();
    }
    else if (context.canceled)
    {
      _isRunning = false;
      StoppedRunning.Invoke();
    }
  }

  public void OnLockInTarget(InputAction.CallbackContext context)
  {
    if (context.performed)
    {
      ToggleLockIn();
    }
  }

  public void OnEnable()
  {
    playerInput.onControlsChanged += DetectarDispositivo;

    // Força atualização inicial
    DetectarDispositivo(playerInput);

    // Atualiza no primeiro frame
  }

  public void OnDisable()
  {
    playerInput.onControlsChanged -= DetectarDispositivo;
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
        {
          break;
        }

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
    if (Context.IsHardLocked)
      return;
    if (Context.IgnoreGameplayInputThisFrame)
      return;
    if (Context.BlockJumpByDialogue)
      return;

    if (Context.WaitForJumpRelease) // segura o input do pulo do tutorial
    {
      if (context.canceled)
        Context.WaitForJumpRelease = false;
      return;
    }
    if (context.started)
      Jump();
  }

  public void OnInteract(InputAction.CallbackContext context)
  {
    if (_interactionObject && context.started)
    {
      ActionLayer.PushState(_interactionActionS, Context);
    }
    GlobalEventBus.Instance.PLAYERTRIGGEREDSKIPDIALOGUE.Invoke(Context);
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
    if (context.started)
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

  #region === Lock in ===

  private void ToggleLockIn()
  {
    if (_isLockOnActive)
    {
      DisableLockIn();
    }
    else if (_lastValidResult.success && _lockedTarget != null)
    {
      _isLockOnActive = true;
      SetLockOn(_lockedTarget);
    }
  }

  private void SetLockOn(ILockable target)
  {
    _lockedTarget = target;

    if (_lockedTarget != null)
    {
      _lockOnCamera.Priority = 20;
      _lockOnGroup.AddMember(transform, 1, 1);
      _lockOnGroup.AddMember(target.transform, .8f, 1);
      _willUpdateLockOverlay = true;
      SetVisibilityLockOnOverlay(true);
      if (_cinemachineInput != null)
      {
        foreach (var controller in _cinemachineInput.Controllers)
        {
          controller.Enabled = false;
        }
      }
    }
    else
    {
      _lockOnCamera.Priority = 0;
      _lockOnGroup.Targets = new();
      SetVisibilityLockOnOverlay(false);
      _willUpdateLockOverlay = false;
      if (_cinemachineInput != null)
      {
        foreach (var controller in _cinemachineInput.Controllers)
        {
          controller.Enabled = true;
        }
      }
    }
  }

  private void SetVisibilityLockOnOverlay(bool set)
  {
    Vector3 scaleTarget = set ? Vector3.one : Vector3.zero;
    if (_lockOnOverlay.TryGetComponent(out RectTransform rect))
    {
      rect.DOScale(scaleTarget, .5f).SetEase(Ease.OutExpo);
    }
  }

  private void UpdateLockOnOverlayTarget()
  {
    if (_lockedTarget != null)
    {
      _lockOnOverlay.transform.position = _lockedTarget.transform.position;
    }
  }

  private void DisableLockIn()
  {
    if (_isLockOnActive)
    {
      _isLockOnActive = false;
      SetLockOn(null);
    }
  }

  #endregion

  #region === Movimento & Pulo ===
  private void Move()
  {
    if (
      _cinemachineCamera == null
      || OverrideGlobal
      || OverrideHorizontal
      || HorizontalLayer.CurrentState is PlayerActionStateDash
    )
    {
      return;
    }
    HorizontalLayer.ChangeState(_movementHorizontalS, Context);
  }

  private void Jump()
  {
    if (DialogueGlobal.Instance != null)
    {
      if (DialogueGlobal.Instance.IsDialogueActive)
        return;

      if (DialogueGlobal.Instance._bloquearJumpTemporariamente)
        return;
    }

    if (OverrideGlobal)
      return;

    if (_touchingWall)
    {
      // wall-jump permitido — segue pra VerticalLayer.ChangeState(...)
      VerticalLayer.ChangeState(_jumpingVerticalS, Context);
      return;
    }

    if (OverrideVertical)
      return;
    if (!(_isGrounded || _currentJumpCount < _maxJumpCount))
      return;
    VerticalLayer.ChangeState(_jumpingVerticalS, Context);
  }
  #endregion

  #region  === PAUSE ===
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
    if (_isDashBlocked)
    {
      return;
    }
    ActionLayer.PushState(_dashActionS, Context);
  }
  #endregion

  #region === KNOCKBACK ===
  private Vector3 _knockbackVelocity;
  private readonly float _knockbackDuration = 0.2f;
  private readonly Timer _knockbackTimer = new();
  private bool isKnockbackActive;
  internal bool _isDashBlocked;

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

  private void BlockPlayerDashToRoutine(float duration)
  {
    if (_isDashBlocked)
    {
      return;
    } // já está bloqueado, não chama de novo
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
      print("TESTE");
      _OnDamage.AddListener(hudDir.DamageShake);
      IsRunning.AddListener(hudDir.RunningShake);
      IsRunning.AddListener(hudDir.GetCameraScript(ID).SpeedFX);
      StoppedRunning.AddListener(hudDir.StopRunningShake);
      StoppedRunning.AddListener(hudDir.GetCameraScript(ID).StopSpeedFX);
    }
  }

  #endregion

  #region Scan
  private void ScanWalls()
  {
    (bool executed, RaycastHit? hit) = _wallScanner.Scan(
      Time.deltaTime,
      (new Ray(transform.position, transform.right), new(transform.position, -transform.right))
    );

    if (executed)
    {
      if (hit.HasValue)
      {
        ActionLayer.PushState(_wallSlidingActionS, Context);
        _lastWallNormal = hit.Value.normal;
      }
      else
      {
        _touchingWall = false;
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
        {
          enemytmp.SetActive(true);
        }
      }
    }
    return true; // só para cumprir TOutput
  }

  [SerializeField]
  private GameObject _lockOnOverlay;
  private bool _willUpdateLockOverlay = false;
  protected internal ILockable _lockedTarget;
  private RaycastHit _lastLockHit;
  private bool _isLockOnActive = false;
  protected RaycastHit _playerRayHit;
  protected internal InteractableObject _interactionObject;
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

    if (_isLockOnActive && _lockedTarget != null)
    {
      if (_lockedTarget.IsActive)
      {
        float dist = Vector3.Distance(transform.position, _lockedTarget.transform.position);
        if (dist <= _lockedTarget.LockRange)
        {
          return (true, _lastLockHit);
        }
      }

      DisableLockIn();
    }

    Ray ray = new(selectedcamera.transform.position, selectedcamera.transform.forward);
    var (executed, scanResult) = _cameraScanner.Scan(Time.deltaTime, ray);

    if (!executed)
      return _lastValidResult;

    if (!scanResult.Item1)
    {
      // Se não bateu em nada, limpa TUDO
      ClearInteractable();
      _lockedTarget = null; // Garante limpeza do alvo de camera
      _lastValidResult = (false, default);
      return _lastValidResult;
    }

    RaycastHit hit = scanResult.Item2;

    // Tenta pegar o Lockable (obrigatório para ser detectado aqui)
    if (hit.collider.TryGetComponent(out ILockable lockable))
    {
      if (lockable.IsActive && hit.distance <= lockable.LockRange)
      {
        _lockedTarget = lockable;
        _lastLockHit = hit;
        if (hit.collider.TryGetComponent(out InteractableObject interactable))
        {
          _interactionObject = interactable;
        }

        _lastValidResult = (true, hit);
        return _lastValidResult;
      }
    }

    ClearInteractable();
    return (false, default);
  }

  // === Método auxiliar para limpar estado === //
  protected void ClearInteractable()
  {
    _interactionObject = null;
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

  public PlayerContext(Player player)
    : base(player)
  {
    this.player = player;
  }

  public CharacterController PlayerController
  {
    get => player._characterController;
  }
  public CinemachineCamera PlayerCamera
  {
    get => player._cinemachineCamera;
  }
  public InteractableObject PlayerInteractionReference
  {
    get => player._interactionObject;
  }
  public ILockable PlayerLockedTarget
  {
    get => player._lockedTarget;
  }
  public Animator PlayerAnimator
  {
    get => player.animatorComp;
  }
  public Transform PlayerModelTransform
  {
    get => player._modelTransform;
  }
  public PlayerInput PlayerInput
  {
    get => player.playerInput;
  }
  public float PlayerSpeed
  {
    get => player.Speed;
    set => player.Speed = value;
  }
  public float PlayerRunningSpeed
  {
    get => player.RunningSpeed;
    set => player.RunningSpeed = value;
  }
  public float PlayerRunningAcceleration
  {
    get => player._accelerationRunning;
    set => player._accelerationRunning = value;
  }
  public QualityTier PlayerWallSpeedMultiplier
  {
    get => player.wallSpeedMultiplier;
    set => player.wallSpeedMultiplier = value;
  }
  public float PlayerWallJumpMultiplier
  {
    get => player._wallJumpMultiplier;
    set => player._wallJumpMultiplier = value;
  }
  public float PlayerWallExitDuration
  {
    get => player.wallExitDuration;
    set => player.wallExitDuration = value;
  }
  public float PlayerJumpForce
  {
    get => player.JumpForce;
    set => player.JumpForce = value;
  }
  public bool PlayerIsRunning
  {
    get => player._isRunning;
  }
  public int PlayerMaxJumpCount
  {
    get => player._maxJumpCount;
    set => player._maxJumpCount = value;
  }
  public float PlayerGravity
  {
    get => player._gravityValue;
    set => player._gravityValue = value;
  }
  public float PlayerGravityUpMultiplier
  {
    get => player._gravityUpMultiplier;
    set => player._gravityUpMultiplier = value;
  }
  public float PlayerGravityDownMultiplier
  {
    get => player._gravityDownMultiplier;
    set => player._gravityDownMultiplier = value;
  }
  public float PlayerMaxFallSpeed
  {
    get => player._maxFallSpeed;
    set => player._maxFallSpeed = value;
  }
  public float InitialGravityValue
  {
    get => player._initialGravityValue;
  }
  public float PlayerDashSpeed
  {
    get => player.DashSpeed;
    set => player.DashSpeed = value;
  }
  public bool IsDashBlocked
  {
    get => player._isDashBlocked;
    set => player._isDashBlocked = value;
  }
  public float PlayerDashCooldown
  {
    get => player.dashCooldown;
    set => player.dashCooldown = value;
  }
  public float DashDistance
  {
    get => player._dashDistance;
  }
  public float PlayerAcceleration
  {
    get => player._acceleration;
    set => player._acceleration = value;
  }
  public float PlayerFriction
  {
    get => player._friction;
    set => player._friction = value;
  }
  public float PlayerAirFriction
  {
    get => player._airFriction;
    set => player._airFriction = value;
  }
  public Vector3 PlayerMovementVector
  {
    get => player._movementVector;
    set => player._movementVector = value;
  }
  public Vector3 PlayerDirection
  {
    get => player._direction;
    set => player._direction = value;
  }
  public Vector3 PlayerDashDirection
  {
    get => player._dashDirection;
    set => player._dashDirection = value;
  }
  public float PlayerDashCurrent
  {
    get => player._dashCurrent;
    set => player._dashCurrent = value;
  }
  public float PlayerDashDuration
  {
    get => player._dashDuration;
    set => player._dashDuration = value;
  }
  public Vector2 PlayerMoveInput
  {
    get => player._moveInput;
    set => player._moveInput = value;
  }
  public Vector3 PlayerLastWallNormal
  {
    get => player._lastWallNormal;
    set => player._lastWallNormal = value;
  }
  public int PlayerCurrentJumpCount
  {
    get => player._currentJumpCount;
    set => player._currentJumpCount = value;
  }
  public bool PlayerIsGrounded
  {
    get => player._isGrounded;
    set => player._isGrounded = value;
  }
  public bool PlayerWallSpeedApplied
  {
    get => player._wallSpeedApplied;
    set => player._wallSpeedApplied = value;
  }
  public bool PlayerTouchingWall
  {
    get => player._touchingWall;
    set => player._touchingWall = value;
  }
  public bool PlayerCanMove
  {
    get => player.CanMove;
    set => player.CanMove = value;
  }
  public bool PlayerCanDash
  {
    get => player._canDash;
    set => player._canDash = value;
  }
  public bool PlayerWillAttack
  {
    get => player.willAttack;
    set => player.willAttack = value;
  }
  public bool PlayerCanAttack
  {
    get => player.canAttack;
    set => player.canAttack = value;
  }

  public ShiftDashScript PlayerDashScript
  {
    get => player.dashHUDScript;
  }
  public bool PlayerIsDashing
  {
    get => player._isDashing;
    set => player._isDashing = value;
  }
  public float PlayerAttackCooldown
  {
    get => player.attackCooldown;
    set => player.attackCooldown = value;
  }
  public StateMachine<PlayerContext> PlayerHorizontalLayer
  {
    get => player.HorizontalLayer;
  }
  public StateMachine<PlayerContext> PlayerVerticalLayer
  {
    get => player.VerticalLayer;
  }
  public StackStateMachine<PlayerContext> PlayerActionLayer
  {
    get => player.ActionLayer;
  }
  public bool OverrideHorizontal
  {
    get => player.OverrideHorizontal;
    set => player.OverrideHorizontal = value;
  }
  public bool OverrideVertical
  {
    get => player.OverrideVertical;
    set => player.OverrideVertical = value;
  }
  public bool OverrideGlobal
  {
    get => player.OverrideGlobal;
    set => player.OverrideGlobal = value;
  }
  public GameObject PlayerObject => player.gameObject;
  public bool CameraLocked { get; set; } = false;
  public bool IsHardLocked; // Trava tudo (movimento, dash, ações)

  public bool IgnoreGameplayInputThisFrame { get; set; }

  public bool WaitForJumpRelease;

  public bool BlockJumpByDialogue = false;
}
