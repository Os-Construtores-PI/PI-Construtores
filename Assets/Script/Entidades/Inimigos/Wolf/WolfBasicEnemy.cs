using System.Collections;
using DG.Tweening;
using UnityEngine;

public class WolfBasicEnemy : RigidbodyBasedEnemy
{
  #region Serialized Fields

  [Header("References")]
  [SerializeField]
  private EyeWolf _vision;

  [SerializeField]
  private Animator _animator;

  [Header("Movement Settings")]
  [SerializeField]
  private float _stopDistance = 10f;

  [SerializeField]
  private float _patrolRadius = 5f;

  [SerializeField]
  private float _chaseSpeed = 4f;

  [SerializeField]
  private float _patrolSpeed = 2f;

  [Header("Chase Memory")]
  [SerializeField]
  private float _chaseMemoryTime = 3f;

  [Header("Combat")]
  [SerializeField]
  private float _attackDistance = 10f;

  [SerializeField]
  private float _minAttackDistance = 2f;

  [Header("Rush (DOTween)")]
  [SerializeField]
  private float _prepTime = 0.6f;

  [SerializeField]
  private float _rushDistance = 4f;

  [SerializeField]
  private float _rushDuration = 0.35f;

  [SerializeField]
  private Ease _rushEase = Ease.OutQuad;

  [SerializeField]
  private float _dashCooldown = 1.2f;

  [Header("Patrol & Idle")]
  [SerializeField]
  private float _minIdleTime = 1.5f;

  [SerializeField]
  private float _maxIdleTime = 4f;

  [SerializeField]
  private int _minPatrolsBeforeIdle = 5;

  [SerializeField]
  private int _maxPatrolsBeforeIdle = 9;

  #endregion

  #region Private Fields & State Machine

  private float _memoryTimer;
  private float _dashTimer;
  private bool _isWaiting;
  private Tweener _currentTweener;
  private Coroutine _attackCoroutine;
  private Coroutine _idleCoroutine;
  private Transform _playerTransform;
  private Transform _currentTarget;
  private Vector3 _startPosition;

  private int _currentPatrolCount;
  private int _patrolsBeforeIdle;
  private Vector3 _patrolTarget;

  public WolfStateMachine<WolfBasicEnemy> MainMachine = new();
  public WolfStateChase Chase = new();
  public WolfStatePatrol Patrol = new();
  public WolfStateAttack Attack = new();

  #endregion

  #region Public API (Used by States)

  public EyeWolf Vision => _vision;
  public bool IsWaiting => _isWaiting;
  public float DashTimer => _dashTimer;
  public float MemoryTimer => _memoryTimer;
  public float AttackDistanceSqr => _attackDistance * _attackDistance;
  public float MinAttackDistanceSqr => _minAttackDistance * _minAttackDistance;

  public void ChangeState(IWolfState<WolfBasicEnemy> type) => MainMachine.ChangeState(type, this);

  public void SetCurrentTarget(Transform target) => _currentTarget = target;

  public void ResetMemoryTimer() => _memoryTimer = _chaseMemoryTime;

  public void DecrementMemoryTimer() => _memoryTimer -= Time.deltaTime;

  public void StopIdleCoroutine()
  {
    if (_idleCoroutine != null)
    {
      StopCoroutine(_idleCoroutine);
      _idleCoroutine = null;
      _isWaiting = false;
    }
  }

  public void PickNewPatrolPoint()
  {
    Vector3 randomPoint = _startPosition + Random.insideUnitSphere * _patrolRadius;
    randomPoint.y = transform.position.y;
    _patrolTarget = randomPoint;
  }

  public void MoveToPatrolPoint()
  {
    float distance = Vector3.Distance(transform.position, _patrolTarget);
    if (distance < 1f)
    {
      _currentPatrolCount++;
      if (_currentPatrolCount >= _patrolsBeforeIdle)
      {
        StartIdleWait();
        return;
      }
      PickNewPatrolPoint();
      return;
    }
    MoveWithRigidbody(_patrolTarget, _patrolSpeed);
  }

  public void MoveToTarget()
  {
    if (_currentTarget == null)
      return;
    MoveWithRigidbody(_currentTarget.position, _chaseSpeed);
  }

  #endregion

  #region Unity Lifecycle

  public override void Awake()
  {
    base.Awake();
    _vision ??= GetComponentInChildren<EyeWolf>();
    _animator ??= GetComponentInChildren<Animator>();
    _startPosition = transform.position;
  }

  public override void Start()
  {
    base.Start();
    _playerTransform ??= GameObject.FindGameObjectWithTag("Player")?.transform;
    _patrolsBeforeIdle = Random.Range(_minPatrolsBeforeIdle, _maxPatrolsBeforeIdle + 1);

    MainMachine.ChangeState(Patrol, this);
  }

  public override void Update()
  {
    base.Update();
    if (_playerTransform == null)
      _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

    if (_dashTimer > 0f)
      _dashTimer -= Time.deltaTime;

    MainMachine.Update(this);
  }

  private void FixedUpdate()
  {
    MainMachine.FixedUpdate(this);
  }

  protected void OnDisable() => Cleanup();

  public override void OnDestroy()
  {
    base.OnDestroy();
    Cleanup();
  }

  #endregion

  #region Attack & Animation

  public void BeginAttackSequence()
  {
    _dashTimer = _dashCooldown;
    if (_attackCoroutine != null)
      StopCoroutine(_attackCoroutine);
    _attackCoroutine = StartCoroutine(PrepareThenRush(_vision.DetectedPlayer));
  }

  public void StopAttackCoroutine()
  {
    if (_attackCoroutine != null)
    {
      StopCoroutine(_attackCoroutine);
      _attackCoroutine = null;
    }
    _currentTweener?.Kill();
    _currentTweener = null;
  }

  private IEnumerator PrepareThenRush(Transform playerTransform)
  {
    _animator.SetBool("isAttacking", true);
    _animator.SetTrigger("AttackCombo");

    Vector3 dir = playerTransform.position - transform.position;
    dir.y = 0f;
    if (dir.sqrMagnitude > 0.001f)
    {
      Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
      transform.DORotateQuaternion(targetRot, 0.25f);
    }

    yield return new WaitForSeconds(_prepTime);

    Vector3 toPlayer = playerTransform.position - transform.position;
    toPlayer.y = 0f;
    float rushMagnitude = Mathf.Min(_rushDistance, toPlayer.magnitude - _minAttackDistance);
    Vector3 desiredPos = transform.position + toPlayer.normalized * rushMagnitude;

    _currentTweener?.Kill();
    _currentTweener = transform.DOMove(desiredPos, _rushDuration).SetEase(_rushEase);
    yield return _currentTweener.WaitForCompletion();
    yield return new WaitForSeconds(0.05f);

    _animator.SetBool("isAttacking", false);
    _attackCoroutine = null;

    bool seesPlayer = _vision != null && _vision.DetectedPlayer != null;
    if (seesPlayer)
    {
      float distSqr = Vector3.SqrMagnitude(transform.position - _vision.DetectedPlayer.position);
      if (distSqr <= MinAttackDistanceSqr || distSqr <= AttackDistanceSqr)
        yield return new WaitForSeconds(0.15f);

      ChangeState(Chase);
    }
    else
    {
      ChangeState(Patrol);
    }
  }

  #endregion

  #region Idle Handling

  private void StartIdleWait()
  {
    if (_idleCoroutine != null)
      StopCoroutine(_idleCoroutine);
    _idleCoroutine = StartCoroutine(WaitOnPatrol());
  }

  private IEnumerator WaitOnPatrol()
  {
    if (MainMachine.CurrentState != Patrol)
      yield break;

    _isWaiting = true;
    SetAnimationState(isWalking: false, isIdle: true);

    yield return null;
    yield return new WaitForSeconds(0.5f);

    float waitTime = Random.Range(_minIdleTime, _maxIdleTime);
    yield return new WaitForSeconds(waitTime);

    if (MainMachine.CurrentState != Patrol)
      yield break;

    SetAnimationState(isWalking: true, isIdle: false);

    _currentPatrolCount = 0;
    _patrolsBeforeIdle = Random.Range(_minPatrolsBeforeIdle, _maxPatrolsBeforeIdle + 1);

    PickNewPatrolPoint();
    _isWaiting = false;
    _idleCoroutine = null;
  }

  #endregion

  #region Helpers

  public void SetAnimationState(bool isWalking, bool isIdle)
  {
    if (_animator == null)
      return;
    _animator.SetBool("isWalking", isWalking);
    _animator.SetBool("isIdle", isIdle);
  }

  private void Cleanup()
  {
    _currentTweener?.Kill();
    _currentTweener = null;
    if (_attackCoroutine != null)
      StopCoroutine(_attackCoroutine);
    StopIdleCoroutine();
  }

  #endregion
}
